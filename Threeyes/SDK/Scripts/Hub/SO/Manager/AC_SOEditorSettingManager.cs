#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Threeyes.GameFramework;
/// <summary>
/// 【Editor】缓存用户设置
/// </summary>
public class AC_SOEditorSettingManager : SOEditorSettingManager<AC_SOEditorSettingManager, AC_SOWorkshopItemInfo>
{
    #region Property & Field
    public bool HubSimulator_ShowAssistantGizmo
    {
        get
        {
            return hubSimulator_ShowAssistantGizmo;
        }
        set
        {
            hubSimulator_ShowAssistantGizmo = value;
            EditorUtility.SetDirty(Instance);
        }
    }
    public bool HubSimulator_ShowAssistantInfo
    {
        get
        {
            return hubSimulator_ShowAssistantInfo;
        }
        set
        {
            hubSimulator_ShowAssistantInfo = value;
            EditorUtility.SetDirty(Instance);
        }
    }


    [Header("HubSimulator")]
    [SerializeField] protected bool hubSimulator_ShowAssistantGizmo = true;
    [SerializeField] protected bool hubSimulator_ShowAssistantInfo = true;

    #endregion
}
#endif
