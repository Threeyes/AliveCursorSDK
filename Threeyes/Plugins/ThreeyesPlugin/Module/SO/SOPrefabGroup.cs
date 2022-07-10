using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 存放各种预制物
/// </summary>
[CreateAssetMenu(menuName = "SO/Prefab")]
public class SOPrefabGroup : SOGroupBase<GameObject>
{
    #region VersionUpdate

    [System.Obsolete("Use ListData Instead")]
    [Header(outdateWarningTips)]
    public List<GameObject> listPrefab = new List<GameObject>();

#if UNITY_EDITOR
    void OnValidate()
    {
#pragma warning disable CS0618
        Threeyes.Editor.EditorVersionUpdateTool.TransferList(this, ref listPrefab, ref listData);
#pragma warning restore CS0618
    }
#endif
    #endregion
}
