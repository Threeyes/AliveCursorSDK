using System.Collections;
using System.Collections.Generic;
using Threeyes.Coroutine;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 根据元素的比例，更新元素的尺寸。适用于动态实例化元素
/// 注意：需要选中指定的Preferred Width/Height
/// </summary>
[RequireComponent(typeof(LayoutElement))]
public class LayoutElementHelper : UIAutoFitHelperBase<LayoutElement>
{
    public AspectRatioFitter.AspectMode aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;

    protected override void SetAspectAtOnce(float aspectHeiDevWid)
    {
        if (!Comp)//可能是因为被销毁导致无法获取
            return;
        if (aspectMode == AspectRatioFitter.AspectMode.WidthControlsHeight)
        {
            Comp.preferredHeight = RectTransform.rect.width * aspectHeiDevWid;
        }
        else if (aspectMode == AspectRatioFitter.AspectMode.HeightControlsWidth)
        {
            Comp.preferredWidth = RectTransform.rect.height * 1 / aspectHeiDevWid;
        }
    }
}
