using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if USE_JsonDotNet
using Newtonsoft.Json;
#endif
/// <summary>
/// 保存相关联的一组Tips信息
/// </summary>
[CreateAssetMenu(menuName = "SO/TipsInfo/TipsInfoGroup")]
public class SOTipsInfoGroup : SOGroupBase<SOTipsInfo>
{
    [System.Obsolete("Use ListData Instead")]
    [Header(outdateWarningTips)]
    [JsonIgnore] public List<SOTipsInfo> listTipsInfo = new List<SOTipsInfo>();

    #region VersionUpdate
#if UNITY_EDITOR
    void OnValidate()
    {
#pragma warning disable CS0618
        Threeyes.Editor.EditorVersionUpdateTool.TransferList(this, ref listTipsInfo, ref listData);
#pragma warning restore CS0618
    }
#endif
    #endregion
}
