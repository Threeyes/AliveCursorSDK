using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// 控制台的场景配置
/// 作用：
/// ——通过ConsoleSceneManagerHelper调用，用于回到控制台场景并自动加载下一个场景
/// </summary>
[CreateAssetMenu(menuName = EditorDefinition.AssetMenuPrefix_SO_Build + "ConsoleSceneConfig")]
public class SOConsoleSceneConfig : ScriptableObject
{
    [Header("控制台场景信息")]
    public SOSceneInfo consoleSceneInfo;//中控台场景信息
    public SOSceneInfoGroup soSceneInfoGroup;//所有的场景信息

    [Header("自动加载")]
    public SOSceneInfo sceneInfoToAutoLoad;//待加载的场景信息，可空
    public SOSceneInfo sceneInfoLastLoaded;//已经加载的(非控制台)场景信息，可空

    [Header("按返回键回到控制台")]
    public bool isPressKeyToReturnConsole = false;//是否按下头盔的返回键自动返回Console

    //Todo:自动按顺序加载的功能，跟设备的Pref相关，跟默认SOConsoleSceneConfig设置无关
    [Header("RuntimeLoad PlayerPrefs")]
    public bool isAutoLoad = false;//是否首次进入后自动加载（同时禁用UI交互）
    public bool isReturnToConsoleOnFinish = false;//回到控制台
    public PlaybackOrderType playbackOrderType = PlaybackOrderType.Order;//场景播放的顺序

    public List<SOSceneInfo> listSceneInfoToDisplay
    {
        //除去该控制台的场景信息
        get
        {
            if (soSceneInfoGroup && consoleSceneInfo)
                return soSceneInfoGroup.ListData.FindAll((si) => si != consoleSceneInfo);

            Debug.LogError("引用为空！");
            return new List<SOSceneInfo>();
        }
    }

    /// <summary>
    /// 场景播放的顺序
    /// </summary>
    public enum PlaybackOrderType
    {
        Order = 0,//顺序播放
        Repeat = 1,//单个场景循环
    }

}
