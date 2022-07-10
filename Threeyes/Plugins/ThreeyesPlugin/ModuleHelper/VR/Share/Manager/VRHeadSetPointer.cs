using System;
using System.Collections;
using System.Collections.Generic;
using Threeyes.Coroutine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// 头显的焦点
/// 半成品
/// </summary>
public class VRHeadSetPointer : MonoBehaviour
{
    #region Hover
    [Header("Hover")]
    public float hoverToClickTime = 2f;

    //Debug
    [Space]
    public bool isStartHoverCountDown = false;
    public bool isClicked = false;
    public GameObject currentTarget;
    public FloatEvent onCoundDownPercent;

    private float hoverUsedTime;
    protected EventSystem cachedEventSystem;
    protected Coroutine cacheEnum;

    public float HoverUsedTime
    {
        get
        {
            return hoverUsedTime;
        }
        set
        {
            hoverUsedTime = Mathf.Clamp(value, 0, hoverToClickTime); ;
            onCoundDownPercent.Invoke(hoverUsedTime / hoverToClickTime);
        }
    }

    protected void Awake()
    {
        if (!cachedEventSystem)
        {
            cachedEventSystem = FindObjectOfType<EventSystem>();
        }
    }

    protected void TryStopCoroutine()
    {
        if (cacheEnum != null)
        {
            CoroutineManager.StopCoroutineEx(cacheEnum);
        }
    }

    void StartHoverClickCountDown()
    {
        TryStopCoroutine();
        cacheEnum = CoroutineManager.StartCoroutineEx(IEHoverClickCountDown());
    }

    IEnumerator IEHoverClickCountDown()
    {
        isStartHoverCountDown = true;
        float startTime = Time.time;

        while (Time.time - startTime < hoverToClickTime)
        {
            HoverUsedTime = Time.time - startTime;
            yield return null;
        }
        SetPressDown();//模拟点击
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        SetPressUp();
        //Todo：重置选择
        ResetHover();
        isClicked = true;
    }

    private void SetPressUp()
    {
        throw new NotImplementedException();
    }

    private void SetPressDown()
    {
        throw new NotImplementedException();
    }

    private void ResetHover()
    {
        isStartHoverCountDown = false;
        currentTarget = null;
        HoverUsedTime = 0;
    }

    Selectable GetCurrentHoverSelectable()
    {
        //foreach (RaycastResult rayCastResult in sortedRaycastResults)
        //{
        //    var go = rayCastResult.gameObject;
        //    if (go)
        //    {
        //        var selectable = go.GetComponentInParent<Selectable>();
        //        //selectable = go.GetComponent<Selectable>();
        //        if (selectable && selectable.interactable)
        //            return selectable;
        //    }
        //}
        return null;
    }

    private void DetectHoverState()
    {
        //检测是否凝视状态
        EventSystem eventSystem = EventSystem.current;
        BaseInputModule baseInputModule = eventSystem.GetComponent<BaseInputModule>();
        if (eventSystem)
        {
            Selectable selectable = GetCurrentHoverSelectable();
            //是否可选择
            if (selectable)
            {
                GameObject goCurHover = selectable.gameObject;
                //初始化
                if (currentTarget == null)
                {
                    currentTarget = goCurHover;
                    if (!isStartHoverCountDown && !isClicked)
                    {
                        StartHoverClickCountDown();
                    }
                }
                if (goCurHover == currentTarget)
                {

                }
                else
                {
                    TryStopCoroutine();
                    ResetHover();
                    isClicked = false;
                }
            }
            else//没选中
            {
                TryStopCoroutine();
                ResetHover();
                isClicked = false;
            }
        }
    }

    #endregion

}
