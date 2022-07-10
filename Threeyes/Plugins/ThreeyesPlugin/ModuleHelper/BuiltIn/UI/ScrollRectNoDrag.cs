using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// 用该脚本替换ScrollRect组件，可禁用ScrollRect的拖拽功能，适用于以按键换页的形式
/// </summary>
public class ScrollRectNoDrag : ScrollRect
{
    public override void OnBeginDrag(PointerEventData eventData) { }
    public override void OnDrag(PointerEventData eventData) { }
    public override void OnEndDrag(PointerEventData eventData) { }
}
