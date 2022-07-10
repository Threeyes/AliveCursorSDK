using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 
/// </summary>
public class RectTransformHelper : ComponentHelperBase<RectTransform>
{
    /// <summary>
    /// 与屏幕的比例保持一致,适用于World Canvas等UI
    /// </summary>
    public void ChangeAspect(float aspect)
    {
        Comp.sizeDelta = new Vector2(Comp.sizeDelta.y * aspect, Comp.sizeDelta.y);
    }
}
