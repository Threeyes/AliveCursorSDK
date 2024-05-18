using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Threeyes.BuiltIn
{
    /// <summary>
    /// Warning：
    /// -PointerDown的时候，可能就因为onValueChanged导致值被更新，所以不适用于缓存旧值（可以参考UIField_NumberBase，在首次onValueChanged时就缓存其值）
    /// </summary>
    public class Slider_Ex : Slider
        , IPointerUpHandler
    {
        /// <summary>
        /// 取消拖拽（抬起）
        /// </summary>
        public UnityEvent onEndDrag;

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            onEndDrag.Invoke();
        }
    }
}