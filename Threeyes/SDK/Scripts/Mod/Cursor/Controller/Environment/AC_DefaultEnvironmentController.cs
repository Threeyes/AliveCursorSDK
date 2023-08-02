using System;
using Threeyes.Steamworks;
using UnityEngine;
/// <summary>
/// Control Environment Setting
/// 
/// PS:
/// 1.Default Environment Lighting/Reflections Sources come from Skybox, inheric this class if your want to change them
/// 2.If some new field not shown in json file, which will 
/// </summary>
[AddComponentMenu(AC_EditorDefinition.ComponentMenuPrefix_Root_Cursor_Controller + "AC_DefaultEnvironmentController")]
public class AC_DefaultEnvironmentController : DefaultEnvironmentController<AC_SODefaultEnvironmentControllerConfig, AC_DefaultEnvironmentController.ConfigInfo>
{

    #region Define
    [Serializable]
    public class ConfigInfo : DefaultEnvironmentControllerConfigInfo
    {

    }
    #endregion

    #region Editor Method
#if UNITY_EDITOR
    //——MenuItem——
    static string instName = "DefaultEnvironmentController";
    [UnityEditor.MenuItem(AC_EditorDefinition.HierarchyMenuPrefix_Root_Cursor_Controller_Environment + "Default", false)]
    public static void CreateInst()
    {
        Threeyes.Editor.EditorTool.CreateGameObjectAsChild<AC_DefaultEnvironmentController>(instName);
    }
#endif
    #endregion
}