using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 保存相关联的一组场景信息
/// </summary>
[CreateAssetMenu(menuName = EditorDefinition.AssetMenuPrefix_SO_Build + "SceneInfoGroup")]
public class SOSceneInfoGroup : SOGroupBase<SOSceneInfo>
{
    #region VersionUpdate

    [System.Obsolete("Use ListData Instead")]
    [Header(outdateWarningTips)]
    public List<SOSceneInfo> listSceneInfo = new List<SOSceneInfo>();

#if UNITY_EDITOR
    void OnValidate()
    {
#pragma warning disable CS0618
        Threeyes.Editor.EditorVersionUpdateTool.TransferList(this, ref listSceneInfo, ref listData);
#pragma warning restore CS0618
    }
#endif
    #endregion
}
