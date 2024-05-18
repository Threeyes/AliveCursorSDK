using System.Collections;
using System.Collections.Generic;
using Threeyes.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Threeyes.BuiltIn
{
    /// <summary>
    /// Warning: !!!不能直接继承EventTrigger，否则界面会被替换成EventTrigger的界面并且Event被隐藏掉
    /// </summary>
    public class EventTriggerListener : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerClickHandler
    {
        public UnityEvent onPointerEnter;
        public UnityEvent onPointerExit;
        public BoolEvent onPointerEnterExit;

        public UnityEvent onPointerDown;
        public UnityEvent onPointerUp;

        public UnityEvent onPointerClick;
        public UnityEvent onPointerDoubleClick;

        public void OnPointerEnter(PointerEventData eventData)
        {
            onPointerEnter.Invoke();
            onPointerEnterExit.Invoke(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            onPointerExit.Invoke();
            onPointerEnterExit.Invoke(false);
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            onPointerDown.Invoke();
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            onPointerUp.Invoke();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onPointerClick.Invoke();
            if (eventData.clickCount == 2)
            {
                onPointerDoubleClick.Invoke();
            }
        }
    }
}