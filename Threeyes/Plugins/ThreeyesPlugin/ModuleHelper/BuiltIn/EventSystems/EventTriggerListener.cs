using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
/// <summary>
/// Warning: !!!不能直接继承EventTrigger，否则界面会被替换成EventTrigger的界面并且Event被隐藏掉
/// </summary>
public class EventTriggerListener : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public UnityEvent onPointerEnter;
    public UnityEvent onPointerExit;
    public BoolEvent onPointerEnterExit;
    public UnityEvent onPointerClick;

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

    public void OnPointerClick(PointerEventData eventData)
    {
        onPointerClick.Invoke();
    }
}