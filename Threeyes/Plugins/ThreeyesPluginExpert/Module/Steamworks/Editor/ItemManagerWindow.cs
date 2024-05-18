#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using UMod.ModTools.Export;
using UMod.BuildEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Newtonsoft.Json;
using Threeyes.Decoder;
using Threeyes.IO;
using Threeyes.SceneTemplate;
using UnityEngine.SceneManagement;
using UMod.Shared;
using UnityEditor.PackageManager;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using UnityEditor.PackageManager.Requests;
using Threeyes.Core;
using Threeyes.Core.Editor;

namespace Threeyes.Steamworks
{
    public abstract class ItemManagerWindowInfo<TSOEditorSettingManager, TSOItemInfo>
    {
        public abstract TSOEditorSettingManager SOEditorSettingManagerInst { get; }
        public abstract string WindowAssetPath { get; }
        public virtual void BeforeBuild(ref TSOItemInfo soItemInfo) { }

        public abstract void AfterBuild(ModBuildResult result, ref TSOItemInfo soItemInfo);

    }
    public abstract class ItemManagerWindow : EditorWindow
    {
        protected static SORuntimeSettingManager SORuntimeManagerInst { get { return SORuntimeSettingManager.Instance; } }

        protected VisualTreeAsset uxmlAsset = default;

        //——Item Manager Group——
        protected VisualElement visualElementItemManagerGroup;
        protected DropdownField dropdownFieldActiveItem;
        protected TextField textFieldNewItemName;
        protected Button buttonCreateItem;
        protected HelpBox helpBoxItemManager;//提示框

        //——SOWorkshopItemInfo Group——
        protected VisualElement visualElementSOWorkshopItemInfoGroup;
        protected VisualElement visualElementPreviewArea;
        protected Label labelPreviewRemark;
        protected HelpBox helpBoxPreview;//预览图提示框


        //——Interaction Group——
        //Edit
        protected Button buttonSelectItemDirButton;
        protected Button buttonEditScene;//Create/Edit Scene

        //Build
        protected HelpBox helpBoxBuild;//打包提示框
        protected TextField textFieldExePath;
        protected Button buttonSelectExe;
        protected Button buttonItemBuild;
        protected Button buttonItemBuildAndRun;
        protected Button buttonItemRun;
        //Button buttonItemBuildAll;
        //Upload
        protected TextField textFieldChangeLog;
        protected Button buttonItemUpload;
        protected Button buttonItemReuploadAll;
        protected Button buttonItemOpenUrl;
        //进度条相关组件
        protected ProgressBar progressBarUpload;

        protected Label labelAgreement;//Steam上传相关协议，可以点击

        //——SDK Info Group——
        protected Label labelSDKVersion;
        protected Button buttonUpdateSDK;

        protected abstract void InitUIWithCurInfo();
    }

    public abstract class ItemManagerWindow<TItemManagerWindow, TItemManagerWindowInfo, TSOEditorSettingManager, TSOItemInfo, TItemInfo> : ItemManagerWindow
        where TItemManagerWindow : ItemManagerWindow<TItemManagerWindow, TItemManagerWindowInfo, TSOEditorSettingManager, TSOItemInfo, TItemInfo>
        where TItemManagerWindowInfo : ItemManagerWindowInfo<TSOEditorSettingManager, TSOItemInfo>, new()
        where TSOEditorSettingManager : SOEditorSettingManager<TSOEditorSettingManager, TSOItemInfo>
        where TSOItemInfo : SOWorkshopItemInfo<TItemInfo>
        where TItemInfo : WorkshopItemInfo, new()
    {
        public static ItemManagerWindow<TItemManagerWindow, TItemManagerWindowInfo, TSOEditorSettingManager, TSOItemInfo, TItemInfo> WindowInstance;

        public static TSOEditorSettingManager SOManagerInst { get { return info.SOEditorSettingManagerInst; } }
        public static readonly TItemManagerWindowInfo info = new TItemManagerWindowInfo();


        //——Runtime——
        protected static readonly Vector2 k_MinWindowSize = new Vector2(450, 600);
        protected static List<TSOItemInfo> listValidItemInfo = new List<TSOItemInfo>();//扫描到的信息
        protected static TSOItemInfo curSOWorkshopItemInfo;

        private void OnEnable()
        {
            WindowInstance = this;
            uxmlAsset = Resources.Load<VisualTreeAsset>(info.WindowAssetPath);
            uxmlAsset.CloneTree(rootVisualElement);
            InitUXMLField();

            //InitUI
            InitUI(SOManagerInst.CurWorkshopItemInfo);


            //版本提示。ToAdd：增加Package更新后执行
            InitSDKVersionUI();
        }

        /// <summary>
        /// 初始化UXML的相关元素
        /// </summary>
        protected virtual void InitUXMLField()
        {
            //——Item Manager Group——
            visualElementItemManagerGroup = rootVisualElement.Q<VisualElement>("ItemManagerGroup");
            dropdownFieldActiveItem = rootVisualElement.Q<DropdownField>("ActiveItemDropdownField");
            dropdownFieldActiveItem.RegisterValueChangedCallback<string>(OnActiveItemDropdownFieldChanged);
            textFieldNewItemName = rootVisualElement.Q<TextField>("NewItemNameTextField");
            textFieldNewItemName.RegisterCallback<ChangeEvent<string>>(OnNewItemNameTextFieldChanged);
            textFieldNewItemName.RegisterCallback<KeyDownEvent>(OnNewItemNameTextFieldKeyDown);
            buttonCreateItem = rootVisualElement.Q<Button>("CreateItemButton");
            buttonCreateItem.RegisterCallback<ClickEvent>(OnCreateItemButtonClick);
            buttonCreateItem.SetInteractable(false);//设置默认状态
            helpBoxItemManager = rootVisualElement.Q<HelpBox>("ItemManagerHelpBox");

            //——SOWorkshopItemInfo Group——
            visualElementSOWorkshopItemInfoGroup = rootVisualElement.Q<VisualElement>("SOWorkshopItemInfoGroup");
            var textFieldTitle = rootVisualElement.Q<TextField>("TitleTextField");
            textFieldTitle.RegisterCallback<ChangeEvent<string>>(OnTitleTextFieldChanged);

            //Preview
            var objectFieldPreview = rootVisualElement.Q<ObjectField>("PreviewObjectField");
            objectFieldPreview.RegisterValueChangedCallback<Object>(OnPreviewObjectFieldChanged);
            visualElementPreviewArea = rootVisualElement.Q<VisualElement>("ItemPreviewArea");
            labelPreviewRemark = visualElementPreviewArea.Q<Label>("PreviewRemarkLabel");
            var buttonSelectPreview = rootVisualElement.Q<Button>("SelectPreviewButton");
            buttonSelectPreview.RegisterCallback<ClickEvent>(OnSelectPreviewButtonClick);
            var buttonCreateScreenshot = rootVisualElement.Q<Button>("CreateScreenshotButton");
            buttonCreateScreenshot.RegisterCallback<ClickEvent>(OnCreateScreenshotButtonClick);
            var togglePlayGif = rootVisualElement.Q<Toggle>("PlayGifToggle");
            togglePlayGif.value = SOManagerInst.ItemWindow_IsPreviewGif;
            togglePlayGif.RegisterCallback<ChangeEvent<bool>>(OnPlayGifToggleChanged);
            helpBoxPreview = rootVisualElement.Q<HelpBox>("PreviewHelpBox");

            //Tags
            Foldout foldoutItemTags = rootVisualElement.Q<Foldout>("ItemTagsFoldout");

            //——Interaction Group——
            buttonSelectItemDirButton = rootVisualElement.Q<Button>("SelectItemDirButton");
            buttonSelectItemDirButton.RegisterCallback<ClickEvent>(OnSelectItemDirButtonClick);
            buttonEditScene = rootVisualElement.Q<Button>("EditSceneButton");
            buttonEditScene.RegisterCallback<ClickEvent>(OnEditSceneButtonClick);

            //Build
            helpBoxBuild = rootVisualElement.Q<HelpBox>("BuildHelpBox");
            textFieldExePath = rootVisualElement.Q<TextField>("ExePathTextField");
            textFieldExePath.value = SOManagerInst.ItemWindow_ExePath;
            textFieldExePath.RegisterCallback<ChangeEvent<string>>(OnExePathTextFieldChanged);
            buttonSelectExe = rootVisualElement.Q<Button>("SelectExeButton");
            buttonSelectExe.RegisterCallback<ClickEvent>(OnSelectExeButtonClick);

            Toggle toggleShowOutputDirectory = rootVisualElement.Q<Toggle>("ShowOutputDirectoryToggle");
            toggleShowOutputDirectory.value = SOManagerInst.ItemWindow_ShowOutputDirectory;
            toggleShowOutputDirectory.RegisterCallback<ChangeEvent<bool>>(OnShowOutputDirectoryToggleChanged);

            buttonItemBuild = rootVisualElement.Q<Button>("ItemBuildButton");
            buttonItemBuild.RegisterCallback<ClickEvent>(OnBuildButtonClick);
            buttonItemBuildAndRun = rootVisualElement.Q<Button>("ItemBuildAndRunButton");
            buttonItemBuildAndRun.RegisterCallback<ClickEvent>(OnBuildAndRunButtonClick);
            buttonItemRun = rootVisualElement.Q<Button>("ItemRunButton");
            buttonItemRun.RegisterCallback<ClickEvent>(OnRunButtonClick);
            //buttonItemBuildAll = rootVisualElement.Q<Button>("ItemBuildAllButton");
            //buttonItemBuildAll.RegisterCallback<ClickEvent>(OnBuildAllButtonClick);

            textFieldChangeLog = rootVisualElement.Q<TextField>("ChangeLogTextField");
            buttonItemUpload = rootVisualElement.Q<Button>("ItemUploadButton");//PS:只有所有必要信息填写完成后，按键才能点击
            buttonItemUpload.RegisterCallback<ClickEvent>(OnUploadButtonClick);
            buttonItemReuploadAll = rootVisualElement.Q<Button>("ItemReuploadAllButton");
            buttonItemReuploadAll.RegisterCallback<ClickEvent>(OnReuploadAllButtonClick);

            buttonItemOpenUrl = rootVisualElement.Q<Button>("ItemOpenUrlButton");//PS:只有所有必要信息填写完成后，按键才能点击
            buttonItemOpenUrl.RegisterCallback<ClickEvent>(OnOpenUrlButtonClick);

            //——Upload Group——
            labelAgreement = rootVisualElement.Q<Label>("AgreementLabel");
            labelAgreement.RegisterCallback<ClickEvent>(OnAgreementLabelClick);
            progressBarUpload = rootVisualElement.Q<ProgressBar>("UploadProcessBar");

            //——SDK Info Group——
            labelSDKVersion = rootVisualElement.Q<Label>("SDKVersionLabel");
            buttonUpdateSDK = rootVisualElement.Q<Button>("UpdateSDKButton");
            buttonUpdateSDK.RegisterCallback<ClickEvent>(OnUpdateSDKButtonClick);
        }

        private void Update()
        {
            Update_CreateScreenshot();
            Update_PackageInfo();
        }

        #region Init&Update UI
        /// <summary>
        /// 刷新所有UI，使用上次选中的信息（适用于全局状态更新）
        /// </summary>
        protected override void InitUIWithCurInfo()
        {
            InitUI(curSOWorkshopItemInfo);
        }
        void InitUI(TSOItemInfo target)
        {
            InitItemDropdownState(target);
            SetItemManagerHelpBoxInfoFunc(false);//默认隐藏
        }
        /// <summary>
        /// 
        /// 设置：
        /// -listValidItemInfo
        /// -SOManagerInst.CurWorkshopItemInfo（通过InitCurInfo方法）
        /// </summary>
        /// <param name="defaultSelect">需要默认选中的物体</param>
        void InitItemDropdownState(TSOItemInfo defaultSelect = null)
        {
            ///功能:
            ///1.搜索所有的Item并呈现在下拉框中
            ///2.当选中一个Item时，自动找到对应的SOWorkshopItemInfo，如果没有则隐藏SOWorkshopItemInfo Group UI

            //#1 扫描有效的Item
            listValidItemInfo = GetListValidInfo();

            //#2 尝试找到默认选中的Item（有可能因为手动删除导致无法找到，这时就使用默认Info）
            TSOItemInfo targetInfo = listValidItemInfo.FirstOrDefault();
            int defaultSelectIndex = 0;
            if (defaultSelect)
            {
                var matchInfo = listValidItemInfo.Find((so) => so.itemName == defaultSelect.itemName);
                if (matchInfo)
                {
                    targetInfo = matchInfo;
                    defaultSelectIndex = listValidItemInfo.IndexOf(targetInfo);
                }
            }

            //#3 重新创建popupField
            dropdownFieldActiveItem.choices.Clear();
            if (listValidItemInfo.Count > 0)
            {
                List<string> listChoices = listValidItemInfo.Select(i => i.itemName).ToList();
                dropdownFieldActiveItem.choices = listChoices;
                dropdownFieldActiveItem.value = targetInfo.itemName;
            }
            else
            {
                string choiceTips = "Please Create Item...";
                dropdownFieldActiveItem.choices = new List<string>() { choiceTips };
                dropdownFieldActiveItem.value = choiceTips;
            }

            InitCurInfo(targetInfo);
        }


        /// <summary>
        /// 初始化当前选中Item信息
        /// </summary>
        /// <param name="target">可空</param>
        void InitCurInfo(TSOItemInfo target)
        {
            SOManagerInst.CurWorkshopItemInfo = target;//存储选中的值
            curSOWorkshopItemInfo = target;

            if (target)
            {
                SerializedObject so = new SerializedObject(target);
                rootVisualElement.Bind(so);// Bind it to the root of the hierarchy. It will find the right object to bind to...【适用于各Element的BindingPath】
                RefreshItemInfoGroupUIState();

                //Init
                buttonEditScene.text = (File.Exists(curSOWorkshopItemInfo.SceneFilePath) ? "Edit" : "Create") + " Scene";
                bool isItemUploaded = curSOWorkshopItemInfo.IsItemUploaded;
                buttonItemUpload.text = (isItemUploaded ? "" : "Create&") + "Upload";//通过label标明Item尚未上传到Workshop
                ShowUploadProcess(false);
                buttonItemOpenUrl.Show(isItemUploaded);
                if (isItemUploaded)
                {
                    buttonItemOpenUrl.text = $"Open URL({curSOWorkshopItemInfo.itemId})";
                }
            }
            else
            {
                rootVisualElement.Unbind();// Unbind the object from the actual visual element
            }

            //只有Item可用才显示InfoGroup
            visualElementSOWorkshopItemInfoGroup.Show(curSOWorkshopItemInfo != null);
        }


        /// <summary>
        /// 刷新ItemInfoGroupUI状态（适用于修改信息后进行局部更新，如上传、打包）
        /// </summary>
        protected virtual void RefreshItemInfoGroupUIState()
        {
            UpdatePreviewStateFunc();
            UpdatePreviewHelperBoxStateFunc();
            UpdateBuildAndUploadStateFunc();//更新受该必须字段影响的UI
        }

        SearchRequest searchRequest;
        void InitSDKVersionUI()
        {
            //Reset
            buttonUpdateSDK.Show(false);
            labelSDKVersion.text = "";
            searchRequest = Client.Search(SDKIdentifier, true);//在线查询
        }
        void Update_PackageInfo()
        {
            if (searchRequest != null)
            {
                if (!searchRequest.IsCompleted)
                {
                    //Debug.Log("Waiting");
                }
                else
                {
                    //Debug.Log("Completed");
                    if (searchRequest.Status == StatusCode.Success)//Success
                    {
                        if (!this)//被销毁
                            return;

                        string labelContent = "";
                        PackageInfo packageInfoSDKCache = GetCacheSDKPackageInfo();//获取本地缓存的版本
                        if (packageInfoSDKCache != null)
                        {
                            labelContent = $"SDK Version: {packageInfoSDKCache.version}.";

                            PackageInfo packageInfoSDKOnline = searchRequest.Result.FirstOrDefault();
                            if (packageInfoSDKOnline != null)
                            {
                                //检查是否需要更新
                                string latestVersion = packageInfoSDKOnline.versions?.latest;
                                if (latestVersion.NotNullOrEmpty())
                                {
                                    try
                                    {
                                        System.Version versionCur = new System.Version(packageInfoSDKCache.version);
                                        System.Version versionLatest = new System.Version(latestVersion);
                                        if (versionLatest > versionCur)
                                        {
                                            labelContent += $" The latest version is <color=orange>{latestVersion}</color>";
                                            buttonUpdateSDK.Show(true);
                                        }
                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                        }

                        labelSDKVersion.text = labelContent;
                    }
                    //else//Failed
                    //{
                    //Debug.LogError("Error receiving package info: " + searchRequest.Error.message);
                    //}
                    searchRequest = null;//重置
                }
            }

        }

        void OnUpdateSDKButtonClick(ClickEvent evt)
        {
            Debug.Log("Begin update ACSDK on background...");
            Client.Add(SDKIdentifier);//更新到最新版。（无论成功失败都会重新刷新UI并调用InitSDKVersionUI，因此不需要监听回调）
        }

        #endregion

        #region MenuItem
        static TItemManagerWindow windowInstance;
        public static void OpenWindow()
        {
            windowInstance = GetWindow<TItemManagerWindow>("Item Manager");
            windowInstance.minSize = k_MinWindowSize;
        }
        public static void BuildAndRunCurItem()
        {
            bool isBuildSuccess = BuildCurItem();
            if (isBuildSuccess)
            {
                RunCurItem();
            }
        }

        public static void BuildAll()
        {
            string sumError = null;
            var listTempValidItemInfo = GetListValidInfo();
            foreach (var so in listTempValidItemInfo)
            {
                string errorLog;
                BuildItemFunc(so, out errorLog);
                if (errorLog != null)
                {
                    sumError += $"Build Item {so?.Title} with error: {errorLog}" + "\r\n";
                }
            }
            //PS：因为每次打包单个Item完成后都会清空Log Console，因此需要先存储每个BuildItem的错误信息，最后再统一Log（Todo：如果报错就直接终止后续打包）
            if (sumError != null)
            {
                sumError = "Build All Complete, some Items are failed with error:\r\n" + sumError;
                Debug.LogError(sumError);
            }

            if (windowInstance)
                windowInstance.RefreshItemInfoGroupUIState();//刷新UI
        }

        public static void OpenSDKWiki(string url)
        {
            Application.OpenURL(url);
        }
        #endregion

        #region Callback
        private void OnPlayGifToggleChanged(ChangeEvent<bool> evt)
        {
            SOManagerInst.ItemWindow_IsPreviewGif = evt.newValue;
            UpdatePreviewStateFunc();
        }

        private void OnShowOutputDirectoryToggleChanged(ChangeEvent<bool> evt)
        {
            SOManagerInst.ItemWindow_ShowOutputDirectory = evt.newValue;
        }

        private void OnAgreementLabelClick(ClickEvent evt)
        {
            //https://partner.steamgames.com/doc/features/workshop/implementation#Legal
            //	string agreementUrlPath=https://steamcommunity.com/sharedfiles/workshoplegalagreement;
            //string urlPath = "steam://url/CommunityFilePage/" + curSOWorkshopItemInfo.ItemID;//打开Item对应页面
            Application.OpenURL("https://steamcommunity.com/sharedfiles/workshoplegalagreement");

        }

        private void OnProjectChange()
        {
            if (isBuildingMod)
                return;

            ///监听项目的文件变化，每当有变化就自动刷新，并保持选中当前Item
            //Actions that trigger this message include creating, renaming, or reparenting assets, as well as moving or renaming folders in the project.
            InitUIWithCurInfo();
        }
        #endregion

        #region Item Manager Group
        private void OnActiveItemDropdownFieldChanged(ChangeEvent<string> evt)
        {
            var info = listValidItemInfo.Find(i => i.itemName == evt.newValue);
            if (info)
            {
                InitCurInfo(info); //显示Item信息
            }
            else
            {
                Debug.LogError($"Can't find Info for {evt.newValue} !");
            }
        }

        private void OnNewItemNameTextFieldChanged(ChangeEvent<string> evt)
        {
            string inputFileName = evt.newValue;
            string errInfo = "";
            bool isValid = true;//默认为true，避免名称为空的问题（为空时不弹出helpbox，避免界面臃肿）

            if (inputFileName.NotNullOrEmpty())// PS:名字长度为0不报错，避免出现空内容的Helpbox
            {
                //判断文件名是否有效
                isValid = PathTool.IsValidDirName(inputFileName, ref errInfo, SOWorkshopItemInfo.GetItemDirPath(inputFileName));
            }
            buttonCreateItem.SetInteractable(inputFileName.NotNullOrEmpty() && isValid);

            SetItemManagerHelpBoxInfoFunc(!isValid, errInfo);
        }
        private void OnNewItemNameTextFieldKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Return)
            {
                CreateItemFunc();
            }
        }
        private void OnCreateItemButtonClick(ClickEvent evt)
        {
            CreateItemFunc();
        }


        private void CreateItemFunc()
        {
            string inputFileName = textFieldNewItemName.value;
            string errInfo = "";
            if (inputFileName.NotNullOrEmpty() && PathTool.IsValidDirName(inputFileName, ref errInfo, SOWorkshopItemInfo.GetItemDirPath(inputFileName)))
            {
                //尝试创建文件夹
                try
                {
                    //#1 创建该Item根文件夹
                    DirectoryInfo directoryInfoItem = GetOrCreate(SOWorkshopItemInfo.GetItemDirPath(inputFileName));
                    string itemName = directoryInfoItem.Name;//PS:创建文件夹后，要通过DirInfo获取它返回的名称，因为有可能用户输入的名字含有其他字符，导致某些字符被系统删掉（如空格）

                    //#2 创建Data文件夹
                    DirectoryInfo directoryInfoData = GetOrCreate(SOWorkshopItemInfo.GetDataDirPath(itemName));

                    //#3 创建SOWorkshopItemInfo文件
                    FileInfo fileInfoItemInfo = new FileInfo(SOWorkshopItemInfo.GetItemInfoFilePath(itemName));
                    TSOItemInfo infoInst = null;
                    string localItemInfoFilePath = EditorPathTool.AbsToUnityRelatePath(fileInfoItemInfo.FullName);
                    if (!fileInfoItemInfo.Exists)
                    {
                        infoInst = CreateInstance<TSOItemInfo>();
                        infoInst.itemName = itemName;
                        infoInst.GenerateNewIDIfNull();//创建唯一ID
                        infoInst.Title = itemName;//设置默认Title

                        curSOWorkshopItemInfo = infoInst;//主动更新，避免OnProjectChange被调用，导致下拉框无法更新
                        AssetDatabase.CreateAsset(infoInst, localItemInfoFilePath);
                        AssetDatabase.SaveAssets();

                        //重置UI
                        textFieldNewItemName.value = "";
                    }
                    else
                    {
                        infoInst = AssetDatabase.LoadAssetAtPath<TSOItemInfo>(localItemInfoFilePath);
                    }
                    AssetDatabase.Refresh();
                    InitUI(infoInst);//重新创建
                    AfterCreateItem(infoInst);//文件创建成功后
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Create Item failed with error:\r\n" + e);
                }
            }
        }

        protected virtual void AfterCreateItem(TSOItemInfo infoInst)
        {

        }

        void SetItemManagerHelpBoxInfoFunc(bool isShow, string content = "")
        {
            helpBoxItemManager.Show(isShow);
            helpBoxItemManager.text = content;
        }
        #endregion

        #region SOWorkshopItemInfo Group
        private void OnTitleTextFieldChanged(ChangeEvent<string> evt)
        {
            UpdateBuildAndUploadStateFunc();//更新受该必须字段影响的UI
        }
        #region Preview

        private void OnPreviewObjectFieldChanged(ChangeEvent<Object> evt)
        {
            //更新对应的UI
            UpdatePreviewStateFunc();
            UpdatePreviewHelperBoxStateFunc();
            UpdateBuildAndUploadStateFunc();//更新受该必须字段影响的UI
        }
        private void OnSelectPreviewButtonClick(ClickEvent evt)
        {
            if (!curSOWorkshopItemInfo)
                return;
            //打开默认文件夹里的选择菜单
            DirectoryInfo directoryInfoDataPath = new DirectoryInfo(curSOWorkshopItemInfo.DataDirPath);
            string resultFilePath = EditorUtility.OpenFilePanelWithFilters("Select Preview...", directoryInfoDataPath.FullName, new string[] { "Image files", SOWorkshopItemInfo.arrStrValidPreviewFileExtension.ConnectToString(",") });
            if (resultFilePath.NotNullOrEmpty())
            {
                FileInfo fileInfoSelected = new FileInfo(resultFilePath);
                string targetFilePath = fileInfoSelected.FullName;//目标文件

                //如果不是本项目文件,则需要拷贝到Data目录下
                if (!Steamworks_PathDefinition.IsProjectAssetFile(fileInfoSelected.FullName))
                {
                    targetFilePath = curSOWorkshopItemInfo.GetDefaultPreviewFilePath(fileInfoSelected.Extension);
                    DebugLog($"The select file is not inside project! try to copy from {fileInfoSelected.FullName} to {targetFilePath} ...");
                    try
                    {
                        File.Copy(fileInfoSelected.FullName, targetFilePath, true);
                        AssetDatabase.Refresh();
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Copy {fileInfoSelected.FullName} to {targetFilePath} failed: \r\n" + e);
                        return;
                    }
                    //Debug.Log("Copy file to : " + targetFilePath);
                }

                //加载资源并设置引用
                curSOWorkshopItemInfo.TexturePreview = Steamworks_PathDefinition.LoadAssetAtAbsPath<Texture2D>(targetFilePath);
                EditorUtility.SetDirty(curSOWorkshopItemInfo);//PS:需要调用该方法保存更改
            }
        }

        /// <summary>
        /// PS:
        /// -支持任意含有主相机的场景
        /// </summary>
        /// <param name="evt"></param>
        void OnCreateScreenshotButtonClick(ClickEvent evt)
        {
            //Todo:截正方形的图，尺寸为4的倍数（PS：可以对图片进行二次处理）
            //ToUpdate:应该判断当前场景及SimulatorManager场景是否已经打开，如果已经打开则不需要保存；如果未全部打开才提示
            //if (TryOpenScreen())//打开场景
            {
                ////创建临时相机
                //tempGOCaptureCamera = new GameObject("TempCamera");
                ////tempGOCaptureCamera.hideFlags = HideFlags.HideAndDontSave;//The GameObject is not shown in the Hierarchy, not saved to to Scenes, and not unloaded by Resources.UnloadUnusedAssets.
                //Camera camera = tempGOCaptureCamera.AddComponent<Camera>();
                //camera.clearFlags = CameraClearFlags.Nothing;//避免背景为默认颜色

                tempGOCaptureCamera = Camera.main;
                if (!tempGOCaptureCamera)
                {
                    Debug.LogError("Can't find main Camera in Scene!");
                    return;
                }
                OnBeforeCreateScreenshot();

                absPreviewFilePath = curSOWorkshopItemInfo.GetDefaultPreviewFilePath(".png");
                delayFrameCaptureScreenshot = 3;
            }
        }

        protected virtual void OnBeforeCreateScreenshot()
        {
            //临时禁用Simulator的UI，避免截图会截到UI
            assistantManagerSimulator = FindObjectOfType<AssistantManagerSimulator>();//尝试查找该组件
            if (assistantManagerSimulator)
                assistantManagerSimulator.TempShowInfoGroup(false);
        }
        protected virtual void OnAfterCreateScreenshot()
        {
            if (assistantManagerSimulator)
                assistantManagerSimulator.TempShowInfoGroup(true);
        }

        AssistantManagerSimulator assistantManagerSimulator;
        protected Camera tempGOCaptureCamera;
        int delayFrameCaptureScreenshot = 0;//N帧后延后Capture及销毁相机
        string absPreviewFilePath;//ToAdd:存储为默认预览图

        void Update_CreateScreenshot()
        {
            //CreateScreenshot
            if (delayFrameCaptureScreenshot > 0 && curSOWorkshopItemInfo)
            {
                delayFrameCaptureScreenshot--;
                if (delayFrameCaptureScreenshot == 2)//Capture
                {
                    tempGOCaptureCamera.GetComponent<Camera>().Render();//强制渲染
                    ScreenCapture.CaptureScreenshot(absPreviewFilePath);
                }
                else if (delayFrameCaptureScreenshot == 1)//Load and saveTexture
                {
                    AssetDatabase.Refresh();
                    string relatePreviewFilePath = EditorPathTool.AbsToUnityRelatePath(absPreviewFilePath);
                    Texture2D texturePreview = AssetDatabase.LoadAssetAtPath<Texture2D>(relatePreviewFilePath);
                    if (texturePreview)
                    {
                        curSOWorkshopItemInfo.TexturePreview = texturePreview;
                        EditorUtility.SetDirty(curSOWorkshopItemInfo);
                    }
                }
                else if (delayFrameCaptureScreenshot == 0)//Reset
                {
                    OnAfterCreateScreenshot();
                }
            }
        }

        void UpdatePreviewStateFunc()
        {
            TryStopGifSchedule();

            Texture2D previeTex = null;
            if (curSOWorkshopItemInfo && curSOWorkshopItemInfo.TexturePreview)
            {
                if (SOManagerInst.ItemWindow_IsPreviewGif)
                {
                    TryPlayGif();
                }
                previeTex = curSOWorkshopItemInfo.TexturePreview;//设置首帧图
            }

            visualElementPreviewArea.style.backgroundImage = previeTex;
            labelPreviewRemark.text = curSOWorkshopItemInfo && IsGifPath(curSOWorkshopItemInfo.PreviewFilePath) ? "Gif" : "";//提示是否为Gif
        }

        IVisualElementScheduledItem scheduledPlayGif;
        public List<GifFrameData> listGifFrameData = new List<GifFrameData>();
        int curIndex = 0;
        long lastPlayMS = 0;

        static bool IsGifPath(string filePath) { return filePath.EndsWith("gif"); }
        bool TryPlayGif()
        {
            string gifFilePath = curSOWorkshopItemInfo.PreviewFilePath;
            if (!IsGifPath(gifFilePath))
                return false;
            try
            {
                listGifFrameData = FileIO.ReadAllBytes(gifFilePath).ToListGifFrameData();
                if (listGifFrameData.Count > 0)
                {
                    scheduledPlayGif = visualElementPreviewArea.schedule.Execute(PlayGifFrame).Every(10);
                }
            }
            catch
            {
            }//解码失败不影响（因为设置了默认图
            return true;
        }
        void PlayGifFrame(TimerState timerState)
        {
            if (listGifFrameData.Count == 0)
                return;

            float curDelayMs = listGifFrameData[curIndex].frameDelaysSeconds * 1000;//PS:常见值为0.03
            if ((timerState.now - lastPlayMS) > curDelayMs)
            {
                curIndex = (curIndex + 1) % listGifFrameData.Count;
                visualElementPreviewArea.style.backgroundImage = listGifFrameData[curIndex].texture;
                lastPlayMS = timerState.start;
            }
        }
        void TryStopGifSchedule()
        {
            scheduledPlayGif?.Pause();//停掉旧的task
            if (listGifFrameData.Count > 0)
            {
                foreach (var d in listGifFrameData)
                {
                    DestroyImmediate(d.texture);//非运行时执行
                }
                Resources.UnloadUnusedAssets();
            }
            listGifFrameData.Clear();
            curIndex = 0;
            lastPlayMS = 0;
        }


        void UpdatePreviewHelperBoxStateFunc()
        {
            if (curSOWorkshopItemInfo && curSOWorkshopItemInfo.TexturePreview)
            {
                //Todo：检测文件是否包含指定后缀
                if (curSOWorkshopItemInfo.IsPreviewExtensionVaild)
                {
                    //检查是否文件大于1MB
                    string previewFilePath = curSOWorkshopItemInfo.PreviewFilePath;
                    FileInfo fileInfoPreview = new FileInfo(previewFilePath);
                    if (fileInfoPreview.Length > SOWorkshopItemInfo.MaxPreviewFileSize)
                    {
                        SetPreviewHelpBoxInfo($"The size of the preview file can't be larger than {SOWorkshopItemInfo.MaxPreviewFileSize / 1024}KB!\r\n Cur file size: {fileInfoPreview.Length / 1024}KB.");
                    }
                    else
                    {
                        SetPreviewHelpBoxInfoFunc(false);
                    }
                }
                else
                {
                    SetPreviewHelpBoxInfo($"Only {SOWorkshopItemInfo.arrStrValidPreviewFileExtension.ConnectToString(",")} is supported!\r\n Cur file: {Path.GetFileName(curSOWorkshopItemInfo.PreviewFilePath)}");
                }
            }
            else
            {
                SetPreviewHelpBoxInfo("Preview file is required! Please select image file!");
            }
        }

        /// <summary>
        /// （当必须属性更新时）同步更新Build按钮的状态
        /// </summary>
        void UpdateBuildAndUploadStateFunc()
        {
            textFieldChangeLog.value = "";
            if (!curSOWorkshopItemInfo)
            {
                textFieldChangeLog.Show(false);
                return;
            }

            //PS:这里调用的函数涉及FileInfo，所以要尽量减少调用频率
            string errorLog = "";
            bool isBuildValid = curSOWorkshopItemInfo.CheckIfBuildValid(out errorLog);

            //在HelpBox上提示待完成事项
            helpBoxBuild.Show(!isBuildValid);
            if (!isBuildValid)
                helpBoxBuild.text = errorLog;

            buttonItemBuild.SetInteractable(isBuildValid); // 确保Build前所有必填内容都有效，否则禁用
            buttonItemBuildAndRun.SetInteractable(isBuildValid && IsExePathValid(SOManagerInst.ItemWindow_ExePath));//在上面的基础上，需要确定exe已经配置完成
            buttonItemRun.SetInteractable(IsExePathValid(SOManagerInst.ItemWindow_ExePath) && curSOWorkshopItemInfo.IsExported);//确认exe已经Mod是否存在

            textFieldChangeLog.Show(curSOWorkshopItemInfo.IsItemUploaded);
            buttonItemUpload.SetInteractable(curSOWorkshopItemInfo.IsUploadValid);////PS:仅简单检查导出目录是否存在即可，具体错误在点击“Upload‘按键后会打印出来
            buttonItemReuploadAll.SetInteractable(listValidItemInfo.Any(so => so && so.IsItemUploaded));//确保任意Item已上传，则表示可以重新上传
        }

        void SetPreviewHelpBoxInfo(string content = "")
        {
            SetPreviewHelpBoxInfoFunc(true, content);
        }

        void SetPreviewHelpBoxInfoFunc(bool isShow, string content = "")
        {
            helpBoxPreview.Show(isShow);
            helpBoxPreview.text = content;
        }

        #endregion

        #endregion

        #region Interaction Group
        private void OnSelectItemDirButtonClick(ClickEvent evt)
        {
            if (!curSOWorkshopItemInfo)
                return;

            Object obj = AssetDatabase.LoadAssetAtPath(EditorPathTool.AbsToUnityRelatePath(curSOWorkshopItemInfo.ItemDirPath), typeof(Object));// Load dir as object
            if (obj)
            {
                Selection.activeObject = obj;// Select the object in the project folder          
                EditorGUIUtility.PingObject(obj);// Also flash the folder yellow to highlight it
            }
        }

        private void OnEditSceneButtonClick(ClickEvent evt)
        {
            if (!curSOWorkshopItemInfo)
                return;

            //退出播放
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                //ToAdd:需要等待退出完成或直接return，否则会报错
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())//提示用户保存Scene
                return;

            string absItemSceneFilePath = curSOWorkshopItemInfo.SceneFilePath;
            string relateItemSceneFilePath = EditorPathTool.AbsToUnityRelatePath(absItemSceneFilePath);

            if (File.Exists(absItemSceneFilePath))//Item场景存在：打开
            {
                OpenMultiScenes(relateItemSceneFilePath);
            }
            else//不存在：打开新建Scene菜单，提示通过 SceneTemplate 创建
            {
                SceneTemplateManagerWindow.ShowWindow(curSOWorkshopItemInfo.SceneFilePath,
                    (isSuccess) =>
                    {
                        if (isSuccess)
                            OpenMultiScenes(relateItemSceneFilePath);
                    }
                    );
            }
        }

        public static void RunCurSceneWithSimulator()
        {
            //ToUpdate:研究如何设置LightingSettings中对应SimulatorScene，而不是像现在一样需要重新加载(参考LightingWindowEnvironmentSection+LightingWindow）
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())//提示用户保存Scene
                return;


            string itemSceneFilePath = null;//缓存需要编辑的Item场景信息
            for (int i = 0; i != SceneManager.sceneCount; i++)
            {
                Scene activeScene = SceneManager.GetSceneAt(i);
                if (activeScene.path.Contains(SORuntimeManagerInst.SimulatorSceneName))//忽略Simulator场景
                    continue;
                else
                    itemSceneFilePath = activeScene.path;
            }
            if (itemSceneFilePath.IsNullOrEmpty())//如果当前没有Item场景，则代表仅有Simulator场景：跳过
                return;

            OpenMultiScenes(itemSceneFilePath);

            //bool isSimulaterHubSceneLoaded = OpenHubSimulatorScene();//#1 先打开模拟场景，以便LightingSettings能被正常设置
            //EditorSceneManager.OpenScene(itemSceneFilePath, isSimulaterHubSceneLoaded ? OpenSceneMode.Additive : OpenSceneMode.Single);//#2 重新打开Item场景
        }

        static void OpenMultiScenes(string relateItemSceneFilePath)
        {
            if (SORuntimeManagerInst.isOpenSimulatorBeforeItemScene)
            {
                bool isSimulaterHubSceneLoaded = OpenHubSimulatorScene();//#1 尝试打开模拟场景
                EditorSceneManager.OpenScene(relateItemSceneFilePath, isSimulaterHubSceneLoaded ? OpenSceneMode.Additive : OpenSceneMode.Single);//#2 打开Item场景
            }
            else
            {
                EditorSceneManager.OpenScene(relateItemSceneFilePath, OpenSceneMode.Single);
                OpenHubSimulatorScene(OpenSceneMode.Additive);
            }
        }
        static bool OpenHubSimulatorScene(OpenSceneMode openSceneMode = OpenSceneMode.Single)//打开模拟场景
        {
            bool isSimulaterHubSceneLoaded = false;

            try
            {
                //不缓存，避免升级Sample版本导致缓存路径变化
                string simulatorSceneAssetRealPath = null;

                //查找项目中所以可能存在的资源文件路径(Assets或Packages文件夹）
                List<string> listSceneAssetPath = new List<string>();
                foreach (string targetGUID in AssetDatabase.FindAssets(SORuntimeManagerInst.SimulatorSceneName))
                {
                    if (targetGUID.NotNullOrEmpty())
                    {
                        string scenePath = AssetDatabase.GUIDToAssetPath(targetGUID);
                        listSceneAssetPath.Add(scenePath);
                    }
                }

                ///查找首个可读写的Simulator场景文件，优先级如下：
                ///#1 可读写的Package中【如Packages/com.threeyes.alivecursor.sdk/Threeyes/HubSimulator/AliveCursorHub_Simulator.unity】
                string sceneAssetInPackage = listSceneAssetPath.FirstOrDefault((s) => s.StartsWith(PackageSDKPath));
                if (sceneAssetInPackage.NotNullOrEmpty())
                {
                    listSceneAssetPath.Remove(sceneAssetInPackage);
                    if (CheckIfPackageAssetReadWriteable(sceneAssetInPackage))//检查是否可读写
                    {
                        simulatorSceneAssetRealPath = sceneAssetInPackage;
                    }
                }
                ///#2 Asset文件夹中，有以下2种存在形式：
                ///——Hub程序中的位置
                ///——Samples中【如Assets/Samples/AliveCursorSDK/X.X.X/HubSimulator/AliveCursorHub_Simulator.unity】
                if (simulatorSceneAssetRealPath.IsNullOrEmpty())
                {
                    string sceneAssetInAsset = listSceneAssetPath.FirstOrDefault();
                    if (sceneAssetInAsset.NotNullOrEmpty())
                    {
                        if (sceneAssetInAsset.StartsWith(ProjectSamplesSDKPath))//如果是Sample中的文件
                        {
                            //获取Sample文件夹中SDK的版本
                            string assetVersion = sceneAssetInAsset.Replace(ProjectSamplesSDKPath + "/", "");
                            assetVersion = assetVersion.Substring(0, assetVersion.IndexOf("/"));
                            PackageInfo packageInfoSDK = GetCacheSDKPackageInfo();//获取当前SDK的version（PS：如果SDK在Project（管理程序），则返回可能为空）
                            if (packageInfoSDK != null)
                            {
                                string curSDKVersion = packageInfoSDK.version;
                                if (curSDKVersion != assetVersion)//如果Asset/Samples中的版本与Package中的版本不一致，则不报错，通过Warning提示更新。（Modder在PackageManager中点击Update后，会自动升级到最新版本并删除旧版本）
                                {
                                    Debug.LogWarning($"{SORuntimeManagerInst.productName}'s Simulator assets obsolete! Please open PackageManager window and update '{SORuntimeManagerInst.sdkName}/Samples' to latest version [{curSDKVersion}]!");
                                }
                            }
                        }

                        simulatorSceneAssetRealPath = sceneAssetInAsset;
                    }
                    else//无法找到：提醒导入
                    {
                        Debug.LogError($"Can't find Simulator Scene in project! Please open PackageManager window and import the latest Simulator assets via {SORuntimeManagerInst.productName}SDK/Samples!");
                    }
                }

                if (simulatorSceneAssetRealPath.NotNullOrEmpty())
                {
                    //	Debug.Log("Find Simulator scene in " + simulatorSceneAssetRealPath);
                    EditorSceneManager.OpenScene(simulatorSceneAssetRealPath, openSceneMode);
                    isSimulaterHubSceneLoaded = true;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to open Simulator scene with error: " + e);
            }
            return isSimulaterHubSceneLoaded;
        }

        static string PackageSDKPath { get { return "Packages/" + SDKIdentifier; } }//在Package中的sdk路径（不管是否Embedded都是这个路径）
        static string SDKIdentifier { get { return SORuntimeManagerInst.sDKIdentifier; } }
        /// <summary>
        /// 项目文件夹中导入的Samples资源
        /// </summary>
        static string ProjectSamplesSDKPath { get { return "Assets/Samples/" + SORuntimeManagerInst.sdkName; } }
        /// <summary>
        /// 返回缓存到Packages中的SDK信息
        /// </summary>
        /// <returns></returns>
        static PackageInfo GetCacheSDKPackageInfo()
        {
            return PackageInfo.FindForAssetPath(PackageSDKPath);
        }


        /// <summary>
        /// 检查Asset能否读写（如存储在Library/PackageCache文件夹中）
        /// </summary>
        /// <param name="assetPath"></param>
        static bool CheckIfPackageAssetReadWriteable(string assetPath)
        {
            bool canReadWrite = false;
            if (assetPath.NotNullOrEmpty())
            {
                //检查是否为Package资源可读写的相关信息（https://forum.unity.com/threads/check-if-asset-inside-package-is-readonly.900902/）
                PackageInfo packageInfo = PackageInfo.FindForAssetPath(assetPath);//通过资源找到对应的Package信息（后期可从中获取版本信息）（可能为空）
                if (packageInfo != null)
                {
                    var packageSource = packageInfo.source;
                    ///只有local及embeded的文件可读写，原因：https://github.com/microsoft/MixedReality-WebRTC/issues/526）
                    ///-注意事项：如果通过 Embedded 方式导入到Package文件夹中，则可以直接访问
                    canReadWrite = packageSource == PackageSource.Embedded || packageSource == PackageSource.Local;
                }
            }
            return canReadWrite;
        }

        void OnExePathTextFieldChanged(ChangeEvent<string> evt)
        {
            SOManagerInst.ItemWindow_ExePath = evt.newValue;//存储选中的值
            UpdateBuildAndUploadStateFunc();//更新受该必须字段影响的UI
        }
        void OnSelectExeButtonClick(ClickEvent evt)
        {
            string defaultDir = SOManagerInst.ItemWindow_ExePath;
            if (defaultDir.IsNullOrEmpty())
                defaultDir = Application.dataPath;

            string resultFilePath = EditorUtility.OpenFilePanelWithFilters($"Select {SORuntimeManagerInst.productName}.exe...", defaultDir, new string[] { "Exe files", "exe" });
            if (resultFilePath.NotNullOrEmpty())
            {
                FileInfo fileInfoSelected = new FileInfo(resultFilePath);
                string exePath = fileInfoSelected.FullName;//目标文件;
                if (IsExePathValid(exePath))
                {
                    textFieldExePath.value = exePath;
                }
            }
        }
        void OnBuildButtonClick(ClickEvent evt)
        {
            BuildCurItem();
            RefreshItemInfoGroupUIState();
        }
        void OnBuildAndRunButtonClick(ClickEvent evt)
        {
            bool isBuildSuccess = BuildCurItem();
            RefreshItemInfoGroupUIState();

            if (isBuildSuccess)
            {
                RunCurItem();
            }
        }

        void OnRunButtonClick(ClickEvent evt)
        {
            RunCurItem();
        }
        static bool IsExePathValid(string exePath)
        {
            if (exePath.NotNullOrEmpty())
            {
                if (File.Exists(exePath) && exePath.EndsWith($"{SORuntimeManagerInst.productName}.exe"))//检查主程序是否存在
                    return true;
            }
            return false;
        }
        void OnBuildAllButtonClick(ClickEvent evt)
        {
            BuildAll();
        }

        protected static bool BuildCurItem()
        {
            if (!SOManagerInst.CurWorkshopItemInfo)
                return false;

            string absItemSceneFilePath = SOManagerInst.CurWorkshopItemInfo.SceneFilePath;
            bool isModSceneLoaded = false;//Check if cur mod scene loaded
            for (int i = 0; i != SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (absItemSceneFilePath.Contains(scene.path))
                {
                    isModSceneLoaded = true;
                    break;
                }
            }

            string errorLog;
            bool isBuildSuccess = BuildItemFunc(SOManagerInst.CurWorkshopItemInfo, out errorLog);
            if (errorLog != null)//打包失败
            {
                Debug.LogError($"Build Item {SOManagerInst.CurWorkshopItemInfo?.Title} with error: {errorLog}");
            }
            else//打包成功
            {
                //因为UMod打包完成后会把场景关掉，因此需要重新打开Mod场景
                if (isModSceneLoaded)
                {
                    if (File.Exists(absItemSceneFilePath))
                    {
                        string relateFilePath = EditorPathTool.AbsToUnityRelatePath(absItemSceneFilePath);
                        EditorSceneManager.OpenScene(relateFilePath, OpenSceneMode.Additive);//不管管理器场景是否已经存在都可调用
                    }
                }
            }
            return isBuildSuccess;
        }
        static void RunCurItem()
        {
            ///PS:
            ///-WorkingDirectory可以设置程序的运行路径（UseShellExecute要设置为false）（https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.processstartinfo.workingdirectory?view=net-7.0）
            var processStartInfo = new System.Diagnostics.ProcessStartInfo(SOManagerInst.ItemWindow_ExePath, $"-umod \"{SOManagerInst.CurWorkshopItemInfo.ExportItemDirPath}\"");
            System.Diagnostics.Process.Start(processStartInfo);
        }

        //static void TestCacheLog()
        //{
        //    Application.logMessageReceivedThreaded -= ReceivedLog;
        //    Application.logMessageReceivedThreaded += ReceivedLog;
        //}
        //private static void ReceivedLog(string condition, string stackTrace, LogType type)
        //{
        //    if (type == LogType.Error)
        //    {
        //        int a = 1;
        //    }
        //}
        static bool isBuildingMod = false;
        static bool BuildItemFunc(TSOItemInfo sOWorkshopItemInfo, out string errorLog)
        {

            //TestCacheLog();//【遇到打包失败且无错误提示时激活该行】编辑器调试时，通过在ReceivedLog中打断点可及时获取未打印的报错信息，避免被清空

            errorLog = null;
            if (!sOWorkshopItemInfo)
            {
                errorLog = "sOWorkshopItemInfo is null" + "\r\n";
                return false;
            }

            ////#针对uMod 2.9.7或更高版本:Cache
            string curSOInfoPath = AssetDatabase.GetAssetPath(curSOWorkshopItemInfo);
            string sOInfoPath = AssetDatabase.GetAssetPath(sOWorkshopItemInfo);


            //——检查基本配置是否完成——
            string itemBuildValidErrorLog;
            if (!sOWorkshopItemInfo.CheckIfBuildValid(out itemBuildValidErrorLog))
            {
                errorLog = itemBuildValidErrorLog;
                return false;
            }
            try
            {
                isBuildingMod = true;

                if (GetActiveExportProfileSettings(sOWorkshopItemInfo) != null)
                {
                    info.BeforeBuild(ref sOWorkshopItemInfo);//进行打包前的预处理（如更新SOAssetPack）

                    //检查或创建文件夹
                    string exportDirPath = sOWorkshopItemInfo.ExportItemDirPath;
                    DirectoryInfo directoryInfo = GetOrCreate(exportDirPath);

                    //开始打包
                    ExportSettings activeExportSettings = ExportSettings.Active;
                    activeExportSettings.ClearConsoleOnBuild = false;//避免意外清空错误信息
                    activeExportSettings.ShowOutputDirectory = SOManagerInst.ItemWindow_ShowOutputDirectory;//可以避免打包完成后打开文件夹
                    EditorUtility.SetDirty(activeExportSettings);//确保修改的设置被保存


                    SetUpExportProfileSettings(sOWorkshopItemInfo, GetActiveExportProfileSettings(sOWorkshopItemInfo));
                    activeExportSettings.SetActiveExportProfile(GetActiveExportProfileSettings(sOWorkshopItemInfo));
                    ModBuildResult result = ModToolsUtil.StartBuild(activeExportSettings);

                    ////#针对uMod 2.9.7或更高版本（经测试有效，但因UMod打包失败会导致SO发生变化而暂时不升级到此版本）:
                    ////-调用StartBuild后该版本增加了对自定义SO的支持，因此会对SO进行修改(临时变为LinkScriptableObjectV2，之后才会改回原状)。因此需要刷新后重新链接
                    //AssetDatabase.Refresh();
                    //sOWorkshopItemInfo = AssetDatabase.LoadAssetAtPath<TSOItemInfo>(sOInfoPath);
                    //curSOWorkshopItemInfo = AssetDatabase.LoadAssetAtPath<TSOItemInfo>(curSOInfoPath);
                    //WindowInstance.InitUI(curSOWorkshopItemInfo);//会自动初始化listValidItemInfo和SOManagerInst.CurWorkshopItemInfo

                    if (result.Successful)
                    {
                        //设置额外信息（如其他Tags）
                        info.AfterBuild(result, ref sOWorkshopItemInfo);

                        //#拷贝预览图
                        //ToAdd:先删掉旧的预览图(非必须，只是有残留影响不大。删除时的出错情况太多（如占用），暂不实现）
                        //ToUpdate：图片使用同样的名称：Preview.XXX
                        string sourcePreviewFilePath = sOWorkshopItemInfo.PreviewFilePath;
                        File.Copy(sourcePreviewFilePath, sOWorkshopItemInfo.ExportItemPreviewFilePath, true);

                        //#生成Json文件
                        var json = JsonConvert.SerializeObject(sOWorkshopItemInfo.ItemInfo, Formatting.Indented);
                        File.WriteAllText(sOWorkshopItemInfo.ExportItemInfoFilePath, json);
                    }
                    else //Warning:单个打包可能其报错会被清空，建议在调试模式中查看！
                    {
                        errorLog = "Build Failed with error:\r\n" + result.ErrorMessage;
                        return false;
                    }
                }
                else
                {
                    errorLog = "Can'f find ExportProfileSettings!";
                    return false;
                }
            }
            catch (System.Exception e)
            {
                errorLog = "Build Failed:\r\n" + e;
                return false;
            }
            finally
            {
                isBuildingMod = false;//Reset
            }
            return true;
        }

        bool isUploadingAll = false;
        void OnReuploadAllButtonClick(ClickEvent evt)
        {
            if (isUploadingAll)
                return;
            OnUploadAllButtonClickAsync(evt);
        }
        async void OnUploadAllButtonClickAsync(ClickEvent evt)
        {
            //只重新上传已经在Workshop创建的Item
            ///Todo:
            ///1.保存当前选择的Info
            TSOItemInfo lastSelectSO = curSOWorkshopItemInfo;
            isUploadingAll = true;
            ///2.遍历并重新Upload
            foreach (var so in listValidItemInfo)
            {
                if (so.IsItemUploaded)
                {
                    InitCurInfo(so);
                    await Task.Yield();
                    Debug.Log("Begin Upload: " + so.Title);
                    await AsyncUploadItem(so);
                }
            }
            ///3.恢复默认的Info
            InitCurInfo(lastSelectSO);
            isUploadingAll = false;
            Debug.Log("All Item upload completed.");

        }

        Task uploadTask;
        void OnUploadButtonClick(ClickEvent evt)
        {
            if (uploadTask != null && !uploadTask.IsCompleted)//避免频繁点击
            {
                //Debug.Log("The Upload is not complete!");
                return;
            }

            uploadTask = AsyncUploadItem(curSOWorkshopItemInfo);
        }

        async Task AsyncUploadItem(TSOItemInfo soWorkshopItemInfo)
        {
            if (soWorkshopItemInfo)
            {
                string itemUploadValidErrorLog;
                if (!soWorkshopItemInfo.CheckIfUploadValid(out itemUploadValidErrorLog))//检查并提示文件是否缺失等
                {
                    Debug.LogError($"Item {soWorkshopItemInfo?.Title} is not ready to upload because:\r\n{itemUploadValidErrorLog}");
                    return;
                }

                //ToAdd:ChangeLog
                string cacheChangeLog = textFieldChangeLog.value;
                string itemUploadErrorLog = await WorkshopItemUploader.RemoteUploadItem(soWorkshopItemInfo, SetUploadProcessInfo, cacheChangeLog);

                //刷新UI，进度条会默认隐藏
                InitUIWithCurInfo();

                if (itemUploadErrorLog.NotNullOrEmpty())
                {
                    Debug.LogError($"Upload Item {soWorkshopItemInfo?.Title} with error: {itemUploadErrorLog}");
                    textFieldChangeLog.value = cacheChangeLog;//还原ChangeLog，避免要重新设置
                }
            }
        }

        void OnOpenUrlButtonClick(ClickEvent evt)
        {
            if (!curSOWorkshopItemInfo)
                return;

            string itemUrl = WorkshopItemTool.GetUrl(curSOWorkshopItemInfo.itemId);//默认使用浏览器的方式打开，避免需要调用Steam客户端
            if (itemUrl != null)
                Application.OpenURL(itemUrl);
        }


        void SetUploadProcessInfo(float percent)
        {
            SetUploadProcessFunc(true, percent);
        }
        void ShowUploadProcess(bool isShow)
        {
            SetUploadProcessFunc(isShow);
        }
        void SetUploadProcessFunc(bool isShow, float percent = 0)
        {
            progressBarUpload.Show(isShow);
            string strPercent = ((int)(percent * 100)).ToString() + "%";
            progressBarUpload.title = strPercent;
            progressBarUpload.value = percent * 100;
        }

        #endregion

        #region Mod Building
        ///PS:Mod的配置应该是一致的：
        ///名称：Scene.acmod
        static void SetUpExportProfileSettings(SOWorkshopItemInfo so, ExportProfileSettings exportProfileSettings)
        {
            // 设置当前Profile为SO的对应信息
            exportProfileSettings.ModAssetsPath = so.ItemDirPath;
            exportProfileSettings.ModExportPath = so.ExportItemDirPath;
            exportProfileSettings.ModIcon = so.TexturePreview;
        }

        public static ExportProfileSettings GetActiveExportProfileSettings(SOWorkshopItemInfo so)
        {
            ExportProfileSettings actSetting = GetExportProfileSettings(so);
            if (actSetting == null)
                CreateExportProfileSettings(so);//创建默认的Setting
            return GetExportProfileSettings(so);
        }
        static ExportProfileSettings CreateExportProfileSettings(SOWorkshopItemInfo so)
        {
            ExportSettings settings = ExportSettings.Active;
            ExportProfileSettings exportProfileSettings = settings.CreateNewExportProfile(true);//创建新的Profile(注意：会替换重名）
            exportProfileSettings.ModName = so.ItemModName;
            return exportProfileSettings;
        }
        static ExportProfileSettings GetExportProfileSettings(SOWorkshopItemInfo so)
        {
            ExportSettings settings = ExportSettings.Active;
            for (int i = 0; i != settings.ExportProfileCount; i++)
            {
                ExportProfileSettings exportProfileSettingsTemp = settings.ExportProfiles[i];
                if (exportProfileSettingsTemp.ModName == so.ItemModName)
                {
                    return exportProfileSettingsTemp;
                }
            }
            return null;
        }
        #endregion

        #region Utility
        /// <summary>
        /// 扫描有效的Item
        /// </summary>
        /// <returns></returns>
        static List<TSOItemInfo> GetListValidInfo()
        {
            List<TSOItemInfo> listCurValidInfo = new List<TSOItemInfo>();
            DirectoryInfo directoryInfoItemRoot = new DirectoryInfo(Steamworks_PathDefinition.ItemParentDirPath);
            if (!directoryInfoItemRoot.Exists)//自动创建，避免出错
            {
                directoryInfoItemRoot.Create();
                AssetDatabase.Refresh();
            }
            foreach (DirectoryInfo dISub in directoryInfoItemRoot.GetDirectories())
            {
                ///1.检查文件是否齐全，只有包含Item的才能显示出来
                string itemName = dISub.Name;
                string infoFilePath = SOWorkshopItemInfo.GetItemInfoFilePath(itemName);
                if (File.Exists(infoFilePath))
                {
                    FileInfo fileInfoInfo = new FileInfo(infoFilePath);
                    var inst = Steamworks_PathDefinition.LoadAssetAtAbsPath<TSOItemInfo>(fileInfoInfo.FullName);
                    if (inst)
                    {
                        listCurValidInfo.Add(inst);
                    }
                }
            }
            return listCurValidInfo;
        }
        /// <summary>
        /// 尝试打开场景
        /// </summary>
        /// <returns>是否正常打开场景</returns>
        bool TryOpenScreen()
        {
            //判断场景是否存在
            if (curSOWorkshopItemInfo)
            {
                string absPath = curSOWorkshopItemInfo.SceneFilePath;
                if (File.Exists(absPath))
                {
                    string relateFilePath = EditorPathTool.AbsToUnityRelatePath(absPath);
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        EditorSceneManager.OpenScene(relateFilePath, OpenSceneMode.Single);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 将Space字符换成Non-breaking Space字符,避免输入空格后导致错误换行
        /// ToUse：放在描述里，适用于空格后跟着一长段英文
        /// Ref： https://www.jianshu.com/p/2960f30b3bca?from=singlemessage + https://forum.unity.com/threads/non-breaking-space-in-unity.73765/
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        static string ReplaceSpaceCharToNonBreakSpace(string origin)
        {
            return origin.Replace(" ", "\u00A0");
        }
        public static TSOItemInfo GetInst(string itemName)
        {
            TSOItemInfo inst = null;

            string infoFilePath = SOWorkshopItemInfo.GetItemInfoFilePath(itemName);
            if (File.Exists(infoFilePath))
            {
                FileInfo fileInfoInfo = new FileInfo(infoFilePath);
                inst = Steamworks_PathDefinition.LoadAssetAtAbsPath<TSOItemInfo>(fileInfoInfo.FullName);
            }
            return inst;
        }

        //Ref: PathTool
        public static DirectoryInfo GetOrCreate(string dirPath, UnityAction actionOnCreate = null)
        {
            DirectoryInfo dI = new DirectoryInfo(dirPath);
            if (!dI.Exists)
            {
                dI.Create();
                actionOnCreate.Execute();
            }
            return dI;
        }


        static bool isDebugLog = false;
        static void DebugLog(string content)
        {
            if (!isDebugLog)
                return;
            Debug.Log(content);
        }

        #endregion
    }

    /// <summary>
    /// Todo:生成独立类，放到ThreeyesPlugin，使用宏定义封装
    /// </summary>
    public static class UIToolkitLazyExtension
    {
        /// <summary>
        /// Show/Hide Element
        /// </summary>
        /// <param name="visualElement"></param>
        /// <param name="isShow"></param>
        public static void Show(this VisualElement visualElement, bool isShow)
        {
            visualElement.style.display = isShow ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public static void SetInteractable(this VisualElement visualElement, bool isInteractable, bool isSetOpacity = true)
        {
            visualElement.pickingMode = isInteractable ? PickingMode.Position : PickingMode.Ignore;
            if (isSetOpacity)
            {
                visualElement.style.opacity = isInteractable ? 1 : 0.5f;
            }
        }
    }
}
#endif