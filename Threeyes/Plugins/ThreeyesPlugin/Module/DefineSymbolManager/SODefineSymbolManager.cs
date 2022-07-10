#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Threeyes.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 管理宏定义
/// 
/// PS:
/// 1.本脚本只是为插件提供宏定义支持；如果插件已经内置宏定义（如MIRROR），那就不需要自行实现
/// 
/// 参考：https://docs.unity3d.com/Manual/PlatformDependentCompilation.html
/// (ToUpdate:参考EventPlayer的宏定义升级写法，将宏定义存到本文件中，这样可以避免丢失的问题）
/// </summary>
[CreateAssetMenu(menuName = "SO/Manager/DefineSymbolManager")]
public class SODefineSymbolManager : SOInstacneBase<SODefineSymbolManager>
{
    ///根据平台获取对应宏定义 
    public static SODefineSymbolManager Instance
    {
        get
        {
            SODefineSymbolManager result = null;
            //针对安卓平台
            if (EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.Android)
            {
                result = GetOrCreateInstance(ref _instance_Android, defaultName_Android,
                actionOnCreate: (dsm) =>
                {
                    dsm.buildTargetGroup = BuildTargetGroup.Android;
                    EditorUtility.SetDirty(dsm);//！需要调用该方法保存更改
                });
            }
            else
            {
                result = GetOrCreateInstance(ref _instance, defaultName,
                actionOnCreate: (dsm) =>
                {
                    dsm.buildTargetGroup = BuildTargetGroup.Standalone;
                    dsm.useCustomKeyStore = false;
                    EditorUtility.SetDirty(dsm);//！需要调用该方法保存更改
                });
            }
            return result;
        }
    }
    public string ScriptingDefineSymbols
    {
        get
        {
            return PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
        }
        set
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, value);
        }
    }
    /// <summary>
    /// 所有可用的宏定义
    /// （ToAdd：通过Box将以下进行分类）
    /// </summary>
    public static List<DefineSymbol> ListAvaliableDS = new List<DefineSymbol>()
    {
        //通用VR类
        new DefineSymbol("USE_OpenXR", "OpenXR(2020)", "Unity2020及以上的通用AR/VR插件",packageName:"com.unity.xr.openxr"),//PS：从packagemanage中检查
        new DefineSymbol("USE_VRTK", "VRTK", "第三方支持多种VR的插件", "VRTK", "SDK/VRTK_SDK_Bridge.cs"),
        new DefineSymbol("USE_VIU", "ViveInputUtility", "Steam官方的通用输入插件", "ViveInputUtility", "Scripts/VIUSettings.cs"),
        new DefineSymbol("VIU_WAVEVR_2_0_32_OR_NEWER", "VIU_WAVEVR_2_0_32_OR_NEWER", "VIU支持WaveVR的宏定义", "VRModule", "Modules/WaveVRModule.cs"),
        new DefineSymbol("VIU_WAVEVR_2_1_0_OR_NEWER", "VIU_WAVEVR_2_1_0_OR_NEWER", "VIU支持WaveVR的宏定义", "VRModule", "Modules/WaveVRModule.cs"),

        //特定VR类
        new DefineSymbol("USE_PicoEncrypt", "PicoEncry", "第三方支持多种VR的插件", "PicoMobileSDK", "Pvr_CheckDevice.cs"),
        new DefineSymbol("USE_PicoMobileSDK","PicoMobileSDK","Pico插件","PicoMobileSDK","Pvr_UnitySDK/Pvr_UnitySDKManager.cs"),
        new DefineSymbol("USE_RhinoX","RhinoX","RhinoX插件","RhinoX","OpenSource/InteractionSystem/RXInteractionSystem.cs"),

         //new DefineSymbol("USE_SteamVR", "SteamVR", "Steam官方的VR插件", "SteamVR", "Scripts/SteamVR.cs"),//ToDelete（与其相关的代码已经删掉,暂不需要引用其组件）
        //new DefineSymbol("USE_WaveVR", "WaveVR", "Steam官方的通用WaveVR插件", "WaveVR", "Scripts/WaveVR.cs"),//暂不需要引用其组件

        //工具类
        new DefineSymbol("USE_Timeline","Timeline(2020)","时间轴工具",packageName:"com.unity.timeline"),//ToUpdate:等待官方提供宏定义，方便针对通用的BehaviourBase进行显示、隐藏
        new DefineSymbol("USE_RTVoice","RTVoice","语音生成插件","RTVoice","Plugins/RTVoice.dll"),
        new DefineSymbol("USE_LeanPool", "LeanPool", "对象池", "LeanPool", "Scripts/LeanPool.cs"),
        new DefineSymbol("USE_LeanTouch","LeanTouch","触摸屏插件","Touch","Scripts/LeanTouch.cs"),
        new DefineSymbol("USE_SimpleInput","SimpleInput","触摸屏UI插件","SimpleInput","Scripts/SimpleInput.cs"),
        new DefineSymbol("USE_NaughtyAttributes", "NaughtyAttributes", "Editor显示优化插件", "NaughtyAttributes", "Scripts/Core/NaughtyAttribute.cs"),
        new DefineSymbol("USE_DOTween", "DOTween", "优秀的动画插件", "DOTween", "DoTween.dll"),
        new DefineSymbol("USE_DOTweenPro", "DOTweenPro", "优秀的动画插件", "DOTweenPro", "DOTweenPro.dll"),
        new DefineSymbol("USE_PopulationSystem","PopulationSystem","人群生成插件","PopulationSystem","Code/Population.dll"),
        new DefineSymbol("USE_BezierSolution","BezierSolution","曲线插件","BezierSolution","Code/BezierSpline.cs"),
        new DefineSymbol("USE_AVProVideo","AVProVideo","跨平台播放器插件","AVProVideo","Scripts/Components/MediaPlayer.cs"),
        new DefineSymbol("USE_PostProcessingV2", "PostProcessingV2", "后期处理特效V2", "PostProcessing", "Package/PostProcessingBehaviour.cs"),//2018以后的新版特效包，通过PackageManager下载 
        new DefineSymbol("USE_UltimateReplayV2", "Ultimate Replay 2.0", "场景录制与回放V2", "Ultimate Replay 2.0", "Scripts/ReplayBehaviour.cs"),

        //特效类
        new DefineSymbol("USE_Excelsior", "Excelsior", "ScifiUI插件", "Excelsior", "CSFHI/Scripts/InterfaceAnimManager.cs"),//ToDelete
        new DefineSymbol("USE_HighlightingSystem", "HighlightingSystem", "HighlightingSystem插件", "HighlightingSystem", "Scripts/HighlightingRenderer.cs"),//边缘高亮
        new DefineSymbol("USE_PostProcessing", "PostProcessing", "后期处理特效", "PostProcessing", "Runtime/PostProcessingBehaviour.cs"),//2018以前的旧版特效包
        new DefineSymbol("USE_PostProcessingV2", "PostProcessingV2", "后期处理特效V2", "PostProcessing", "Package/PostProcessingBehaviour.cs"),//2018以后的新版特效包，通过PackageManager下载 
        new DefineSymbol("USE_CameraPlay", "CameraPlay", "相机特效", "Camera Play", "Scripts/CameraPlay.cs"),
        new DefineSymbol("USE_UIEffect", "UIEffect", "UI变换", "UIEffect", "Scripts/UIEffect.cs"),//针对UI平面的特效

        //网络类
        new DefineSymbol("USE_KingNetwork", "KingNetwork", "客户端/服务端插件", "KingNetwork", "Scripts/Shared/KingPoolManager.cs"),

        //数据库类
        new DefineSymbol("USE_JsonDotNet","Newtonsoft.Json",".Net Json","JsonDotNet","Assemblies/Standalone/Newtonsoft.Json.dll"),
        new DefineSymbol("USE_UnityCsvUtil","UnityCsvUtil",".Csv插件","UnityCsvUtil","CsvUtil.cs"),
        new DefineSymbol("USE_SimpleSQL", "SimpleSQL", "数据库插件", "SimpleSQL", "Plugins/SimpleSQL_Runtime.dll"),
    };
    static SODefineSymbolManager _instance;
    static SODefineSymbolManager _instance_Android;

    static string defaultName = "DefineSymbolManager";
    static string defaultName_Android = "DefineSymbolManager_Android";

    public BuildTargetGroup buildTargetGroup = BuildTargetGroup.Standalone;

    [Header("Android KeyStore签名")]
    public bool useCustomKeyStore = true;//使用自定义的签名
    public string keyaliasNameOverride = "colyu";//密钥名
    public string keystorePassOverride = "colyu123456";//密钥密码
    public string keyaliasPassOverride = "colyu123456";//Alias密码

    /// <summary>
    /// 自动刷新
    /// Bug：会出现每次更改代码就重新刷新的问题
    /// </summary>
    [Header("刷新设置")]
    public bool isAutoRefresh = false;
    public int maxSearchDepth = 2;//文件夹搜索深度，越小越省时间

    /// <summary>
    /// 当前使用的宏定义
    /// </summary>
    public List<DefineSymbol> listCacheUsedDS = new List<DefineSymbol>();

    /// <summary>
    /// 检查并更新当前的宏定义使用情况
    /// </summary>
    public void Init()
    {
        listCacheUsedDS.Clear();

        //使用当前的宏定义初始化List
        foreach (string def in PlayerSettingsDefines)
        {
            DefineSymbol usedDS = ListAvaliableDS.Find(ds => ds.name == def);

            if (usedDS != null)
            {
                listCacheUsedDS.AddOnce(usedDS);
            }
        }
    }

    UnityAction<List<DefineSymbol>> tempOnSearchComplete;
    DateTime tempDateTime = DateTime.Now;
    public void Search(UnityAction<List<DefineSymbol>> onSearchComplete = null)
    {
        listCacheUsedDS.Clear();

        //Search from Asset (查找指定深度的文件夹)
        tempDateTime = DateTime.Now;
        //List<DirectoryInfo> listDirectoryInfo = new DirectoryInfo(Application.dataPath).GetDirectories("*", SearchOption.AllDirectories).ToList();
        List<DirectoryInfo> listDirectoryInfo = PathTool.GetSubDirectories(new DirectoryInfo(Application.dataPath), maxSearchDepth);        //PS:Linq的 First 和Any有问题，弃用
        foreach (DirectoryInfo directoryInfo in listDirectoryInfo)
        {
            foreach (DefineSymbol defineSymbol in ListAvaliableDS)
            {
                if (defineSymbol.rootFolderName.IsNullOrEmpty() || defineSymbol.rootFolderName != directoryInfo.Name)
                    continue;

                if (defineSymbol.mainFileName.NotNullOrEmpty())
                {
                    if (!File.Exists(directoryInfo.FullName + "/" + defineSymbol.mainFileName))
                        continue;
                }

                listCacheUsedDS.Add(defineSymbol);
            }
        }
        Debug.Log("Search from Asset finish. " + "Directory Count:" + listDirectoryInfo.Count + "\r\n" + "Used Time: " + (DateTime.Now - tempDateTime).Milliseconds);

#if UNITY_2020_1_OR_NEWER
        //Search from PackageManager，因为更新列表需要时间，所以用委托（针对Unity2020）
        tempOnSearchComplete = onSearchComplete;
        tempDateTime = DateTime.Now;
        Debug.Log("Begin Refresh PackageManager List, Please wait……");

        //保证事件只监听一次
        PackageManagerHelper.tempActionOnComplete -= OnPackageManagerUpdate;
        PackageManagerHelper.tempActionOnComplete += OnPackageManagerUpdate;

        PackageManagerHelper.GetListAsync();
#else
        onSearchComplete.Execute(listCacheUsedDS);
#endif
    }

#if UNITY_2020_1_OR_NEWER

    void OnPackageManagerUpdate(UnityEditor.PackageManager.Requests.ListRequest Request, List<UnityEditor.PackageManager.PackageInfo> listPackageInfo)
    {
        if (Request.Status == UnityEditor.PackageManager.StatusCode.Success)
        {
            foreach (var packageInfo in listPackageInfo)
            {
                foreach (DefineSymbol defineSymbol in ListAvaliableDS)
                {
                    if (defineSymbol.packageName.NotNullOrEmpty() && defineSymbol.packageName == packageInfo.name)
                    {
                        listCacheUsedDS.Add(defineSymbol);
                    }
                }
            }
            tempOnSearchComplete.Execute(listCacheUsedDS);
            Debug.Log("Search from PackageManager finish. Used Time: " + (DateTime.Now - tempDateTime).Milliseconds);
        }
        else if (Request.Status == UnityEditor.PackageManager.StatusCode.InProgress)
        {
            //Debug.Log("正在获取PackageManager列表……");
        }
        else
        {
            Debug.LogError("获取PackageManager列表失败：\r\n" + Request.Error);
        }
    }
#endif

    /// <summary>
    /// 自动搜索文件目录下相关插件并设置宏定义
    /// </summary>
    public void SearchAndRefresh(UnityAction onRefreshComplete = null)
    {
        Search
            ((listResult) =>
            {
                PlayerSettingsDefines = GetListDefineName(listResult);
                Debug.Log("Set PlayerSettingsDefines finish. ");
                onRefreshComplete.Execute();
            });
    }


    /// <summary>
    /// 根据用户的选择，更新宏定义
    /// </summary>
    public void Refresh()
    {
        List<string> newDefines = PlayerSettingsDefines.Except(GetListDefineName(ListAvaliableDS)).ToList();//删除Define中所有已添加的自定义宏定义，避免有重复需要筛选（便于重新添加）
        newDefines.AddRange(GetListDefineName(listCacheUsedDS));//添加选中的宏定义

        PlayerSettingsDefines = newDefines;
    }


    List<string> PlayerSettingsDefines
    {
        get { return ScriptingDefineSymbols.Split(';').ToList(); }

        set
        {
            if (!PlayerSettingsDefines.IsElementEqual(value))
            {
                //设置宏定义
                ScriptingDefineSymbols = string.Join(";", value.ToArray());
                Debug.Log("更新DefineSymbols");
            }
            else
            {
                Debug.Log("DefineSymbols相同，不更新。");
            }
        }
    }


    #region Tools


    public List<string> GetListDefineName(List<DefineSymbol> listDefineSymbols)
    {
        return listDefineSymbols.ConvertAll((ds) => ds.name);
    }

    #endregion

    #region EditorMenu

#if UNITY_EDITOR

    public const string EditorToolName = EditorDefinition.TopMenuItemPrefix + "DefineSymbol/";

    //Bug：会出现多次加载的问题
    //[InitializeOnLoadMethod]
    //static void AutoRefresh()
    //{
    //    if (Instance.isAutoRefresh)
    //    {
    //        Debug.Log("SODefineSymbolManager AutoRefresh");
    //        Instance.SearchAndRefresh();
    //    }
    //}

    [MenuItem(EditorToolName + "搜索并刷新宏定义设置")]
    public static void AutoRefresh()
    {
        Debug.Log("SODefineSymbolManager AutoRefresh");
        Instance.SearchAndRefresh();
    }

    [MenuItem(EditorToolName + "选中（宏定义设置文件）")]
    public static void ShowDefineSymbolManager()
    {
        EditorTool.SelectAndHighlight(Instance);
    }

#endif

    #endregion

}


#endif