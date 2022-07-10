using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
/// <summary>
/// 存储当前的厂商信息
/// PS: 需要放在Resources根目录
/// </summary>
public class SOBuildInfoManager : SOInstacneBase<SOBuildInfoManager>
{
    #region Instance

    static string defaultName = "BuildInfoManager";
    private static SOBuildInfoManager _instance;
    public static SOBuildInfoManager Instance
    {
        get
        {
            return GetOrCreateInstance(ref _instance, defaultName);
        }
    }

    //#if UNITY_EDITOR
    //    [MenuItem(CommonEditorInfo.MenuItemPprefix + "Show BuildInfoManager")]
    //    public static void ShowBuildInfoManager()
    //    {
    //        UnityEditor.Selection.activeObject = Instance;
    //    }
    //#endif

    #endregion

    public SOCompanyConfig curCompanyConfig
    {
        get
        {
            return CurBuildInfo && CurBuildInfo.companyConfig ? CurBuildInfo.companyConfig : null;
        }
    }

    public SOBuildInfo CurBuildInfo
    {
        get
        {
            return curBuildInfo;
        }

        set
        {
            curBuildInfo = value;
#if UNITY_EDITOR
            EditorUtility.SetDirty(SOBuildInfoManager.Instance);//！需要调用该方法保存更改
#endif
        }
    }
    /// <summary>
    /// 当前的BuildInfo
    /// </summary>
    public SOBuildInfo curBuildInfo;


}
