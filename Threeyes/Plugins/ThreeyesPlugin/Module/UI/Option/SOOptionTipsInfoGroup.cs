using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 题库（保存一组题目）
/// </summary>
[CreateAssetMenu(menuName = "SO/TipsInfo/OptionTipsInfoGroup")]
public class SOOptionTipsInfoGroup : SOGroupBase<SOOptionTipsInfo>
{
    [System.Obsolete("Use ListData Instead")]
    [Header(outdateWarningTips)]
    public List<SOOptionTipsInfo> listOptionTipsInfo = new List<SOOptionTipsInfo>();//题目组

    #region VersionUpdate
#if UNITY_EDITOR
    void OnValidate()
    {
#pragma warning disable CS0618
        Threeyes.Editor.EditorVersionUpdateTool.TransferList(this, ref listOptionTipsInfo, ref listData);
#pragma warning restore CS0618
    }
#endif
    #endregion
}
