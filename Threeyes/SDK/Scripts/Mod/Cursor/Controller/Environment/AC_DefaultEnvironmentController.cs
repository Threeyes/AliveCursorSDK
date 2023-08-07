using Newtonsoft.Json;
using System;
using Threeyes.Persistent;
using Threeyes.Steamworks;
using UnityEngine;
using UnityEngine.Events;
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
    [Header("Others")]
    [Tooltip("Ground for receive shadows, Optional")] [SerializeField] protected GameObject goGround;

    #region Unity Method
    protected override void Awake()
    {
        base.Awake();
        Config.actionIsUseGroundChanged += OnIsUseGroundChanged;
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        Config.actionIsUseGroundChanged -= OnIsUseGroundChanged;
    }
    #endregion

    #region Config Callback
    void OnIsUseGroundChanged(PersistentChangeState persistentChangeState)
    {
    }
    protected override void UpdateSetting()
    {
        base.UpdateSetting();
        SetGround(Config.isUseGround);
    }
    public virtual void SetGround(bool isUse)
    {
        if (goGround)
            goGround.SetActive(isUse);
    }
    #endregion

    #region Define
    [Serializable]
    public class ConfigInfo : DefaultEnvironmentControllerConfigInfo
    {
        [JsonIgnore] public UnityAction<PersistentChangeState> actionIsUseGroundChanged;

        [Header("Others")]
        [PersistentValueChanged(nameof(OnPersistentValueChanged_IsUseGround))] public bool isUseGround = false;

        #region Callback
        void OnPersistentValueChanged_IsUseGround(PersistentChangeState persistentChangeState)
        {
            actionIsUseGroundChanged.Execute(persistentChangeState);
        }
        #endregion
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