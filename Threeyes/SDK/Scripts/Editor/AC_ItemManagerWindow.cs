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
using Threeyes.Editor;
using Newtonsoft.Json;
using Threeyes.Decoder;
using Threeyes.IO;
using UnityEngine.SceneManagement;
using UMod.Shared;
using UnityEditor.PackageManager;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
namespace Threeyes.AliveCursor.SDK.Editor
{
	/// <summary>
	///
	///ToAdd:
	///1.临时的
	/// 
	/// UIBuilder注意：
	/// 1.（Bug）TextFiled/Label通过bingdingPath绑定ulong后会显示错误，因此暂时不显示ItemID（应该是官方的bug：https://forum.unity.com/threads/binding-ulong-serializedproperty-to-inotifyvaluechanged-long.1005417/）
	/// 2.ViewDataKey只对特定UI有效（PS：This key only really applies to ScrollView, ListView, and Foldout. If you give any of these a unique key (not enforced, but recommended （https://forum.unity.com/threads/can-someone-explain-the-view-data-key-and-its-uses.855145/）)）
	///
	/// ToUpdate:
	/// 1.ChangeLog输入框只有上传成功后才清空
	/// </summary>
	public class AC_ItemManagerWindow : EditorWindow
	{
		AC_SOAliveCursorSDKManager SOManagerInst { get { return AC_SOAliveCursorSDKManager.Instance; } }

		private VisualTreeAsset uxmlAsset = default;

		//——Item Manager Group——
		VisualElement visualElementItemManagerGroup;
		DropdownField dropdownFieldActiveItem;
		TextField textFieldNewItemName;
		Button buttonCreateItem;
		HelpBox helpBoxItemManager;//提示框

		//——SOWorkshopItemInfo Group——
		VisualElement visualElementSOWorkshopItemInfoGroup;
		VisualElement visualElementPreviewArea;
		Label labelPreviewRemark;
		HelpBox helpBoxPreview;//提示框


		//——Interaction Group——
		//Build
		Button buttonSelectItemDirButton;
		Button buttonEditScene;
		Button buttonItemBuild;
		Button buttonItemBuildAll;
		//Upload
		TextField textFieldChangeLog;
		Button buttonItemUpload;
		Button buttonItemReuploadAll;
		Button buttonItemOpenUrl;
		//进度条相关组件
		ProgressBar progressBarUpload;

		Label labelAgreement;//Steam上传相关协议，可以点击

		//——Runtime——
		public List<AC_SOWorkshopItemInfo> listValidItemInfo = new List<AC_SOWorkshopItemInfo>();//扫描到的信息
		AC_SOWorkshopItemInfo curSOWorkshopItemInfo;
		private static readonly Vector2 k_MinWindowSize = new Vector2(450, 700);

		[MenuItem("Alive Cursor/Item Manager")]
		public static void OpenWindow()
		{
			var window = GetWindow<AC_ItemManagerWindow>("Item Manager");
			window.minSize = k_MinWindowSize;
		}
		[MenuItem("Alive Cursor/Add Simulator Scene")]
		public static void RunCurSceneWithSimulator()
		{
			//ToUpdate:研究如何设置LightingSettings中对应SimulatorScene，而不是像现在一样需要重新加载(参考LightingWindowEnvironmentSection+LightingWindow）
			if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())//提示用户保存Scene
				return;

			//缓存当前非Item场景信息
			string itemSceneFilePath = null;
			for (int i = 0; i != SceneManager.sceneCount; i++)
			{
				Scene activeScene = SceneManager.GetSceneAt(i);
				if (activeScene.path.Contains(AC_SceneManagerSimulator.SimulatorSceneName))
					continue;
				else
					itemSceneFilePath = activeScene.path;
			}
			if (itemSceneFilePath.IsNullOrEmpty())
				return;

			//#1 先打开模拟场景，以便LightingSettings能被正常设置
			bool isSimulaterHubSceneLoaded = OpenSimulatorScene_Solo();

			//#2 重新打开Item场景
			EditorSceneManager.OpenScene(itemSceneFilePath, isSimulaterHubSceneLoaded ? OpenSceneMode.Additive : OpenSceneMode.Single);
		}

		private void OnEnable()
		{
			uxmlAsset = Resources.Load<VisualTreeAsset>("Layouts/AC_ItemManagerWindow");
			uxmlAsset.CloneTree(rootVisualElement);

			//##Setup UI
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
			helpBoxPreview = rootVisualElement.Q<HelpBox>("PreviewHelpBox");//Todo:改为全局

			//Tags
			Foldout foldoutItemTags = rootVisualElement.Q<Foldout>("ItemTagsFoldout");

			//——Interaction Group——
			buttonSelectItemDirButton = rootVisualElement.Q<Button>("SelectItemDirButton");
			buttonSelectItemDirButton.RegisterCallback<ClickEvent>(OnSelectItemDirButtonClick);
			buttonEditScene = rootVisualElement.Q<Button>("EditSceneButton");
			buttonEditScene.RegisterCallback<ClickEvent>(OnEditSceneButtonClick);
			buttonItemBuild = rootVisualElement.Q<Button>("ItemBuildButton");
			buttonItemBuild.RegisterCallback<ClickEvent>(OnBuildButtonClick);
			buttonItemBuildAll = rootVisualElement.Q<Button>("ItemBuildAllButton");
			buttonItemBuildAll.RegisterCallback<ClickEvent>(OnBuildAllButtonClick);

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

			//InitUI
			InitUI(SOManagerInst.CurWorkshopItemInfo);
		}

		#region UpdateUI
		/// <summary>
		/// 刷新所有UI，使用上次选中的信息（适用于全局状态更新）
		/// </summary>
		void InitUIWithCurInfo()
		{
			InitUI(curSOWorkshopItemInfo);
		}
		void InitUI(AC_SOWorkshopItemInfo target)
		{
			InitItemDropdownState(target);
			SetItemManagerHelpBoxInfoFunc(false);//默认隐藏
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="defaultSelect">需要默认选中的物体</param>
		void InitItemDropdownState(AC_SOWorkshopItemInfo defaultSelect = null)
		{
			///功能:
			///1.搜索所有的Item并呈现在下拉框中
			///2.当选中一个Item时，自动找到对应的SOWorkshopItemInfo，如果没有则隐藏SOWorkshopItemInfo Group UI

			List<AC_SOWorkshopItemInfo> listCurValidInfo = new List<AC_SOWorkshopItemInfo>();
			DirectoryInfo directoryInfoItemRoot = new DirectoryInfo(AC_PathDefinition.ItemParentDirPath);
			if (!directoryInfoItemRoot.Exists)//自动创建，避免出错
			{
				directoryInfoItemRoot.Create();
				AssetDatabase.Refresh();
			}

			//#1 扫描有效的Item
			foreach (DirectoryInfo dISub in directoryInfoItemRoot.GetDirectories())
			{
				///1.检查文件是否齐全，只有包含Item的才能显示出来
				string itemName = dISub.Name;
				string infoFilePath = AC_SOWorkshopItemInfo.GetItemInfoFilePath(itemName);
				if (File.Exists(infoFilePath))
				{
					FileInfo fileInfoInfo = new FileInfo(infoFilePath);
					var inst = AC_PathDefinition.LoadAssetAtAbsPath<AC_SOWorkshopItemInfo>(fileInfoInfo.FullName);
					if (inst)
					{
						listCurValidInfo.Add(inst);
					}
				}
			}
			listValidItemInfo = listCurValidInfo;

			//#2 尝试找到默认选中的Item（有可能因为手动删除导致无法找到，这时就使用默认Info）
			AC_SOWorkshopItemInfo targetInfo = listValidItemInfo.FirstOrDefault();
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

			InitCurInfo(targetInfo);//显示选中的Item信息
		}

		/// <summary>
		/// 显示当前选中Item信息
		/// </summary>
		/// <param name="target">可空</param>
		void InitCurInfo(AC_SOWorkshopItemInfo target)
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
		void RefreshItemInfoGroupUIState()
		{
			UpdatePreviewStateFunc();
			UpdatePreviewHelperBoxStateFunc();
			UpdateBuildAndUploadStateFunc();//更新受该必须字段影响的UI
		}
		#endregion

		#region Callback
		private void OnPlayGifToggleChanged(ChangeEvent<bool> evt)
		{
			SOManagerInst.ItemWindow_IsPreviewGif = evt.newValue;
			UpdatePreviewStateFunc();
		}

		private void OnAgreementLabelClick(ClickEvent evt)
		{
			//https://partner.steamgames.com/doc/features/workshop/implementation#Legal
			//string urlPath = "steam://url/CommunityFilePage/" +curSOWorkshopItemInfo.ItemID;//打开Item对应页面
			Application.OpenURL("https://steamcommunity.com/sharedfiles/workshoplegalagreement");

		}

		private void OnProjectChange()
		{
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
				InitCurInfo(info); //显示Item信息
		}

		private void OnNewItemNameTextFieldChanged(ChangeEvent<string> evt)
		{
			string inputFileName = evt.newValue;
			string errInfo = "";
			bool isValid = true;//默认为true，避免名称为空的问题（为空时不弹出helpbox，避免界面臃肿）

			if (inputFileName.NotNullOrEmpty())// PS:名字长度为0不报错，避免出现空内容的Helpbox
			{
				//判断文件名是否有效
				isValid = PathTool.IsValidDirName(inputFileName, ref errInfo, AC_SOWorkshopItemInfo.GetItemDirPath(inputFileName));
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
			if (inputFileName.NotNullOrEmpty() && PathTool.IsValidDirName(inputFileName, ref errInfo, AC_SOWorkshopItemInfo.GetItemDirPath(inputFileName)))
			{
				//尝试创建文件夹
				try
				{
					//#1 创建该Item根文件夹
					DirectoryInfo directoryInfoItem = GetOrCreate(AC_SOWorkshopItemInfo.GetItemDirPath(inputFileName));
					string itemName = directoryInfoItem.Name;//PS:创建文件夹后，要通过DirInfo获取它返回的名称，因为有可能用户输入的名字含有其他字符，导致某些字符被系统删掉（如空格）

					//#2 创建Data文件夹
					DirectoryInfo directoryInfoData = GetOrCreate(AC_SOWorkshopItemInfo.GetDataDirPath(itemName));

					//#3 创建SOWorkshopItemInfo文件
					FileInfo fileInfoItemInfo = new FileInfo(AC_SOWorkshopItemInfo.GetItemInfoFilePath(itemName));
					AC_SOWorkshopItemInfo infoInst = null;
					string localItemInfoFilePath = EditorPathTool.AbsToUnityRelatePath(fileInfoItemInfo.FullName);
					if (!fileInfoItemInfo.Exists)
					{
						infoInst = CreateInstance<AC_SOWorkshopItemInfo>();
						infoInst.itemName = itemName;
						infoInst.Title = itemName;//设置默认Title

						curSOWorkshopItemInfo = infoInst;//主动更新，避免OnProjectChange被调用，导致下拉框无法更新
						AssetDatabase.CreateAsset(infoInst, localItemInfoFilePath);
						AssetDatabase.SaveAssets();

						//重置UI
						textFieldNewItemName.value = "";
					}
					else
					{
						infoInst = AssetDatabase.LoadAssetAtPath<AC_SOWorkshopItemInfo>(localItemInfoFilePath);
					}
					AssetDatabase.Refresh();
					InitUI(infoInst);//重新创建
				}
				catch (System.Exception e)
				{
					Debug.LogError("Create Item failed with error:\r\n" + e);
				}
			}
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
			//打开默认文件夹里的选择菜单(ToUpdate:使用通用的工具类)
			DirectoryInfo directoryInfoDataPath = new DirectoryInfo(curSOWorkshopItemInfo.DataDirPath);
			string resultFilePath = EditorUtility.OpenFilePanelWithFilters("Select Preview...", directoryInfoDataPath.FullName, new string[] { "Image files", AC_SOWorkshopItemInfo.arrStrValidPreviewFileExtension.ConnectToString(",") });
			if (resultFilePath.NotNullOrEmpty())
			{
				FileInfo fileInfoSelected = new FileInfo(resultFilePath);
				string targetFilePath = fileInfoSelected.FullName;//目标文件

				//如果不是本项目文件,则需要拷贝到Data目录下
				if (!AC_PathDefinition.IsProjectAssetFile(fileInfoSelected.FullName))
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
				curSOWorkshopItemInfo.TexturePreview = AC_PathDefinition.LoadAssetAtAbsPath<Texture2D>(targetFilePath);
				EditorUtility.SetDirty(curSOWorkshopItemInfo);//PS:需要调用该方法保存更改
			}
		}

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

				//ToUpdate:使用当前场景的相机（默认是SimulaterManager的相机，也可以是用户自定义相机）

				tempGOCaptureCamera = Camera.main;
				if (!tempGOCaptureCamera)
				{
					Debug.LogError("Can't find main Camera in Scene!");
					return;
				}
				cacheCameraLocalPos = tempGOCaptureCamera.transform.localPosition;
				tempGOCaptureCamera.transform.position = new Vector3(0, -0.5f, -1.5f);

				absPreviewFilePath = curSOWorkshopItemInfo.GetDefaultPreviewFilePath(".png");
				delayFrameCaptureScreenshot = 3;
			}
		}
		Camera tempGOCaptureCamera;
		Vector3 cacheCameraLocalPos;
		int delayFrameCaptureScreenshot = 0;//延后Capture及销毁相机
		string absPreviewFilePath;//ToAdd:存储为默认预览图
		private void Update()
		{
			if (delayFrameCaptureScreenshot > 0 && curSOWorkshopItemInfo)//CreateScreenshot
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
				else if (delayFrameCaptureScreenshot == 0)//Complete
				{
					tempGOCaptureCamera.transform.localPosition = cacheCameraLocalPos;
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
			labelPreviewRemark.text = IsGifPath(curSOWorkshopItemInfo.PreviewFilePath) ? "Gif" : "";//提示是否为Gif
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
					if (fileInfoPreview.Length > AC_SOWorkshopItemInfo.MaxPreviewFileSize)
					{
						SetPreviewHelpBoxInfo($"The size of the preview file can't be larger than {AC_SOWorkshopItemInfo.MaxPreviewFileSize / 1024 }KB!\r\n Cur file size: {fileInfoPreview.Length / 1024 }KB.");
					}
					else
					{
						SetPreviewHelpBoxInfoFunc(false);
					}
				}
				else
				{
					SetPreviewHelpBoxInfo($"Only {AC_SOWorkshopItemInfo.arrStrValidPreviewFileExtension.ConnectToString(",")} is supported!\r\n Cur file: {Path.GetFileName(curSOWorkshopItemInfo.PreviewFilePath)}");
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
			buttonItemBuild.SetInteractable(curSOWorkshopItemInfo.IsBuildValid); // 确保Build前所有必填内容都有效，否则禁用
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

			if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())//提示用户保存Scene
				return;

			//#1 尝试打开模拟场景
			bool isSimulaterHubSceneLoaded = OpenSimulatorScene_Solo();

			//#2 打开Item场景
			string absItemSceneFilePath = curSOWorkshopItemInfo.SceneFilePath;
			//判断是否存在
			if (File.Exists(absItemSceneFilePath))//Item场景存在：打开
			{
				string relateFilePath = EditorPathTool.AbsToUnityRelatePath(absItemSceneFilePath);
				EditorSceneManager.OpenScene(relateFilePath, isSimulaterHubSceneLoaded ? OpenSceneMode.Additive : OpenSceneMode.Single);
			}
			else//不存在：打开新建Scene菜单，提示通过 SceneTemplate 创建
			{
				AC_SceneTemplateManagerWindow.ShowWindow(curSOWorkshopItemInfo);
			}
		}

		static bool OpenSimulatorScene_Solo()//打开模拟场景
		{
			bool isSimulaterHubSceneLoaded = false;

			try
			{
				//不缓存，避免升级Sample版本导致缓存路径变化
				string simulatorSceneAssetRealPath = null;

				//查找项目中所以可能存在的资源文件路径
				List<string> listSceneAssetPath = new List<string>();
				foreach (string targetGUID in AssetDatabase.FindAssets(AC_SceneManagerSimulator.SimulatorSceneName))
				{
					if (targetGUID.NotNullOrEmpty())
					{
						string scenePath = AssetDatabase.GUIDToAssetPath(targetGUID);
						listSceneAssetPath.Add(scenePath);
					}
				}

				///查找首个可读写的Simulator场景文件，优先级如下：
				///#1 可读写的Package中【Packages/com.threeyes.alivecursor.sdk/Threeyes/HubSimulator/AliveCursorHub_Simulator.unity】
				string sceneAssetInPackage = listSceneAssetPath.FirstOrDefault((s) => s.StartsWith(PackageSDKPath));
				if (sceneAssetInPackage.NotNullOrEmpty())
				{
					listSceneAssetPath.Remove(sceneAssetInPackage);
					if (CheckIfPackageAssetReadWriteable(sceneAssetInPackage))//检查是否可读写
					{
						simulatorSceneAssetRealPath = sceneAssetInPackage;
					}
				}
				///#2 Asset文件夹中，有以下2中存在形式：
				///——Samples中【Assets/Samples/AliveCursorSDK/1.0.6/HubSimulator/AliveCursorHub_Simulator.unity】
				///——开发程序中的位置
				if (simulatorSceneAssetRealPath.IsNullOrEmpty())
				{
					string sceneAssetInAsset = listSceneAssetPath.FirstOrDefault();
					if (sceneAssetInAsset.NotNullOrEmpty())
					{
						if (sceneAssetInAsset.StartsWith(SamplesSDKPath))//如果是Sample中的文件
						{
							//获取Sample文件夹中SDK的版本
							string assetVersion = sceneAssetInAsset.Replace(SamplesSDKPath + "/", "");
							assetVersion = assetVersion.Substring(0, assetVersion.IndexOf("/"));
							PackageInfo packageInfoSDK = GetSDKPackageInfo();//获取当前SDK的version（PS：如果SDK在Project（管理程序），则返回可能为空）
							if (packageInfoSDK != null)
							{
								string curSDKVersion = packageInfoSDK.version;
								if (curSDKVersion != assetVersion)//如果Asset/Samples中的版本与Package中的版本不一致，则不报错，通过Warning提示更新。（Modder在PackageManager中点击Update后，会自动升级到最新版本并删除旧版本）
								{
									Debug.LogWarning($"Please open PackageManager window and update the Simulator assets to latest version [{curSDKVersion}] via AliveCursorSDK/Samples!");
								}
							}
						}

						simulatorSceneAssetRealPath = sceneAssetInAsset;
					}
					else//无法找到：提醒导入
					{
						Debug.LogError("Can't find Simulator Scene in project! Please open PackageManager window and import the latest Simulator assets via AliveCursorSDK/Samples!");
					}
				}

				if (simulatorSceneAssetRealPath.NotNullOrEmpty())
				{
					//	Debug.Log("Find Simulator scene in " + simulatorSceneAssetRealPath);
					EditorSceneManager.OpenScene(simulatorSceneAssetRealPath, OpenSceneMode.Single);
					isSimulaterHubSceneLoaded = true;
				}
			}
			catch (System.Exception e)
			{
				Debug.LogError("Failed to open Simulator scene with error: " + e);
			}
			return isSimulaterHubSceneLoaded;
		}

		const string SamplesSDKPath = "Assets/Samples/AliveCursorSDK";
		const string PackageSDKPath = "Packages/com.threeyes.alivecursor.sdk";//在Package中的sdk路径（不管是否Embedded都是这个路径）
		static PackageInfo GetSDKPackageInfo()
		{
			return PackageInfo.FindForAssetPath(PackageSDKPath);
		}
		/// <summary>
		/// 检查Asset是否为ReadOnly（如存储在Library/PackageCache文件夹中）
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

		void OnBuildButtonClick(ClickEvent evt)
		{
			string errorLog;
			BuildItemFunc(curSOWorkshopItemInfo, out errorLog);
			if (errorLog != null)
			{
				Debug.LogError($"Build Item {curSOWorkshopItemInfo?.Title} with error: {errorLog}");
			}
			RefreshItemInfoGroupUIState();
		}
		void OnBuildAllButtonClick(ClickEvent evt)
		{
			string sumError = null;
			foreach (var so in listValidItemInfo)
			{
				string errorLog;
				BuildItemFunc(so, out errorLog);
				if (errorLog != null)
				{
					sumError += $"Build Item {so?.Title} with error: {errorLog}" + "\r\n";
				}
			}
			//PS：因为每次打包单个Item完成后都会清空Log Console，因此需要先存储每个BuildItem的错误信息，最后再统一Log
			if (sumError != null)
			{
				sumError = "Build All Complete, some Items are failed with error:\r\n" + sumError;
				Debug.LogError(sumError);
			}
			RefreshItemInfoGroupUIState();
		}

		bool BuildItemFunc(AC_SOWorkshopItemInfo sOWorkshopItemInfo, out string errorLog)
		{
			errorLog = null;
			if (!sOWorkshopItemInfo)
			{
				errorLog = "sOWorkshopItemInfo is null" + "\r\n";
				return false;
			}

			//——检查基本配置是否完成——
			string itemBuildValidErrorLog;
			if (!sOWorkshopItemInfo.CheckIfBuildValid(out itemBuildValidErrorLog))
			{
				errorLog = itemBuildValidErrorLog;
				return false;
			}
			try
			{
				if (ActiveExportProfileSettings != null)
				{
					//检查或创建文件夹
					string exportDirPath = sOWorkshopItemInfo.ExportItemDirPath;
					DirectoryInfo directoryInfo = GetOrCreate(exportDirPath);

					//开始打包
					ExportSettings activeExportSettings = ExportSettings.Active;
					SetUpExportProfileSettings(sOWorkshopItemInfo, ActiveExportProfileSettings);
					activeExportSettings.SetActiveExportProfile(ActiveExportProfileSettings);
					ModBuildResult result = ModToolsUtil.StartBuild(activeExportSettings);

					//设置额外信息
					ModContent modContent = result.BuiltMod.GetModContentMask();
					{
						sOWorkshopItemInfo.itemSafety = modContent.Has(ModContent.Scripts) ? AC_WSItemAdvance.IncludeScripts : AC_WSItemAdvance.None;
						EditorUtility.SetDirty(sOWorkshopItemInfo);
					}

					if (result.Successful)
					{
						//#拷贝预览图
						//ToAdd:先删掉旧的预览图(非必须，只是有残留影响不大。删除时的出错情况太多（如占用），暂不实现）
						//ToUpdate：图片使用同样的名称：Preview.XXX
						string sourcePreviewFilePath = sOWorkshopItemInfo.PreviewFilePath;
						File.Copy(sourcePreviewFilePath, sOWorkshopItemInfo.ExportItemPreviewFilePath, true);

						//#生成Json文件
						var json = JsonConvert.SerializeObject(sOWorkshopItemInfo.ItemInfo, Formatting.Indented);
						File.WriteAllText(sOWorkshopItemInfo.ExportItemInfoFilePath, json);
					}
					else
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
			AC_SOWorkshopItemInfo lastSelectSO = curSOWorkshopItemInfo;
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

		async Task AsyncUploadItem(AC_SOWorkshopItemInfo soWorkshopItemInfo)
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
				string itemUploadErrorLog = await AC_WorkshopItemUploader.RemoteUploadItem(soWorkshopItemInfo, SetUploadProcessInfo, textFieldChangeLog.value);
				if (itemUploadErrorLog.NotNullOrEmpty())
				{
					Debug.LogError($"Upload Item {soWorkshopItemInfo?.Title} with error: {itemUploadErrorLog}");
				}

				//刷新UI，进度条会默认隐藏
				InitUIWithCurInfo();
			}
		}

		void OnOpenUrlButtonClick(ClickEvent evt)
		{
			if (!curSOWorkshopItemInfo)
				return;

			string itemUrl = AC_WorkshopItemTool.GetUrl(curSOWorkshopItemInfo.itemId);//默认使用浏览器的方式打开，避免需要调用Steam客户端
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
		public void SetUpExportProfileSettings(AC_SOWorkshopItemInfo so, ExportProfileSettings exportProfileSettings)
		{
			// 设置当前Profile为SO的对应信息
			exportProfileSettings.ModAssetsPath = so.ItemDirPath;
			exportProfileSettings.ModExportPath = so.ExportItemDirPath;
			exportProfileSettings.ModIcon = so.TexturePreview;
		}

		public static ExportProfileSettings ActiveExportProfileSettings
		{
			get
			{
				ExportProfileSettings actSetting = GetExportProfileSettings();
				if (actSetting == null)
					CreateExportProfileSettings();//创建默认的Setting
				return GetExportProfileSettings();
			}
		}
		static ExportProfileSettings GetExportProfileSettings()
		{
			ExportSettings settings = ExportSettings.Active;
			for (int i = 0; i != settings.ExportProfileCount; i++)
			{
				ExportProfileSettings exportProfileSettingsTemp = settings.ExportProfiles[i];
				if (exportProfileSettingsTemp.ModName == AC_WorkshopItemInfo.ItemModName)
				{
					return exportProfileSettingsTemp;
				}
			}
			return null;
		}
		static ExportProfileSettings CreateExportProfileSettings()
		{
			ExportSettings settings = ExportSettings.Active;
			ExportProfileSettings exportProfileSettings = settings.CreateNewExportProfile(true);//创建新的Profile(注意：会替换重名）
			exportProfileSettings.ModName = AC_WorkshopItemInfo.ItemModName;
			return exportProfileSettings;
		}

		#endregion

		#region Utility

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
		public static AC_SOWorkshopItemInfo GetInst(string itemName)
		{
			AC_SOWorkshopItemInfo inst = null;

			string infoFilePath = AC_SOWorkshopItemInfo.GetItemInfoFilePath(itemName);
			if (File.Exists(infoFilePath))
			{
				FileInfo fileInfoInfo = new FileInfo(infoFilePath);
				inst = AC_PathDefinition.LoadAssetAtAbsPath<AC_SOWorkshopItemInfo>(fileInfoInfo.FullName);
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
	/// Todo:生成独立类
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
#endif
}