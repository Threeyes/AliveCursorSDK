#if USE_VRTK
using UnityEngine;
using VRTK;
/// <summary>
/// 设置VRTK_ObjectTooltip的size
/// </summary>
public class VRTK_ObjectTooltipHelper : ComponentHelperBase<VRTK_ObjectTooltip>
{
    public SOTipsInfo tipsInfo;

    private void Start()
    {
        UpdateText();
    }

    [ContextMenu("UpdateText")]
    void UpdateText()
    {
        Comp.containerSize = new Vector2(Comp.fontSize * (tipsInfo.tips.Length + 1), Comp.containerSize.y);//计算大小
        Comp.UpdateText(tipsInfo.tips);
    }
}
#endif