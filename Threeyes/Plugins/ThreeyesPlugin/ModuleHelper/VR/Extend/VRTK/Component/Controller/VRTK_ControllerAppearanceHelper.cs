using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Threeyes.Coroutine;
#if USE_VRTK
using VRTK;
using ControllerHand = VRTK.SDK_BaseController.ControllerHand;
using ControllerElements = VRTK.SDK_BaseController.ControllerElements;
using TooltipButtons = VRTK.VRTK_ControllerTooltips.TooltipButtons;
#endif
/// <summary>
/// 高亮及提示手柄的指定按键
/// 用途：挂在任意的位置并调用
/// 参考：VRTK_ControllerAppearance_Example
/// 可空依赖脚本：Tips
/// </summary>
public class VRTK_ControllerAppearanceHelper : VRControllerHelperBase
{
#if USE_VRTK

    //手柄元素
    [Header("Element")]
    public Color highlightColor = new Color(0, 1, 1, 0.25f);//高亮颜色
    public Color pulseColor = new Color(0, 0, 0, 0.25f);//进行脉冲变换时的另一端颜色
    public float pulseTimer = 0.75f;


    /// <summary>
    /// 需要高亮或提示的手柄元素
    /// </summary>
    public List<ControllerElements> listControllerElements = new List<ControllerElements>();


    //手柄整体
    [Header("Controller")]
    public float dimOpacity = 0.8f;//高亮元素时，手柄的透明度
    public float defaultOpacity = 1f;

    public void HighLight(bool isHighLight)
    {
        HighLightAndShowTipsFunc(isHighLight);
    }

    public void ShowTips(bool isShowTips)
    {
        HighLightAndShowTipsFunc(isShowTips: isShowTips);
    }

    private void HighLightAndShowTipsFunc(bool? isHighLight = null, bool? isShowTips = null)
    {
        foreach (var controller in GetControllers())
        {
            ControllerAppearanceData data = GetData(controller);
            if (data.NotNull())
            {
                foreach (var controllerElement in listControllerElements)
                {
                    //#高亮
                    if (isHighLight.HasValue)
                    {
                        if (isHighLight.Value)
                        {
                            data.highligher.HighlightElement(controllerElement, highlightColor, pulseTimer);
                            VRTK_ObjectAppearance.SetOpacity(VRTK_DeviceFinder.GetModelAliasController(data.events.gameObject), dimOpacity);
                            StartTweenPulse(data.highligher, controllerElement);
                        }
                        else
                        {
                            data.highligher.UnhighlightElement(controllerElement);
                            VRTK_ObjectAppearance.SetOpacity(VRTK_DeviceFinder.GetModelAliasController(data.events.gameObject), defaultOpacity);
                            StopTweenPulse();
                        }
                    }

                    //#提示
                    if (isShowTips.HasValue)
                    {
                        if (!data.tooltips)
                        {
                            Debug.LogError("请在Left/RightController下方增加ControllerTooltips物体！");
                        }
                        else
                        {
                            TooltipButtons tooltipButtons = GetButtonEnum(controllerElement);
                            if (tooltipButtons == TooltipButtons.None)
                            {
                                //没有找到对应的按钮
                            }
                            else
                            {
                                data.tooltips.ToggleTips(isShowTips.Value, tooltipButtons);
                            }
                        }

                    }
                }
            }
        }
    }

    void StartTweenPulse(VRTK_ControllerHighlighter highligher, ControllerElements controllerElements)
    {
        listCacheEnum.Add(CoroutineManager.StartCoroutineEx(IEPulse(highligher, controllerElements)));
    }
    void StopTweenPulse()
    {
        TryStopCoroutine();
    }

    protected List<Coroutine> listCacheEnum = new List<Coroutine>();
    protected virtual void TryStopCoroutine()
    {
        foreach (var cor in listCacheEnum)
        {
            if (cor.NotNull())
            {
                CoroutineManager.StopCoroutineEx(cor);
            }
        }
        listCacheEnum.Clear();
    }

    IEnumerator IEPulse(VRTK_ControllerHighlighter highligher, ControllerElements controllerElements)
    {
        Color currentPulseColor = highlightColor;

        while (true)
        {
            highligher.HighlightElement(controllerElements, currentPulseColor, pulseTimer);
            currentPulseColor = (currentPulseColor == pulseColor ? highlightColor : pulseColor);//切换下次要变换的颜色
            yield return new WaitForSeconds(pulseTimer);
        }
    }

    #region Utility

    TooltipButtons GetButtonEnum(ControllerElements controllerElements)
    {
        switch (controllerElements)
        {
            case ControllerElements.Trigger:
                return TooltipButtons.TriggerTooltip;
            case ControllerElements.GripLeft:
            case ControllerElements.GripRight:
                return TooltipButtons.GripTooltip;
            case ControllerElements.Touchpad:
                return TooltipButtons.TouchpadTooltip;
            case ControllerElements.ButtonOne:
                return TooltipButtons.ButtonOneTooltip;
            case ControllerElements.ButtonTwo:
                return TooltipButtons.ButtonTwoTooltip;
            case ControllerElements.StartMenu:
                return TooltipButtons.StartMenuTooltip;
            default:
                return TooltipButtons.None;
        }
    }

    ControllerAppearanceData GetData(object controllerRef)
    {
        VRTK_ControllerEvents controllerEvents = controllerRef as VRTK_ControllerEvents;
        if (!controllerEvents)
        {
            Debug.LogError("未找到对应手柄！");
            return null;//可能是该平台没有对应手柄
        }

        ControllerAppearanceData data = new ControllerAppearanceData();
        if (controllerRef == VRInterface.rightControllerRef)
        {
            data.controllerHand = ControllerHand.Right;
        }
        else if (controllerRef == VRInterface.leftControllerRef)
        {
            data.controllerHand = ControllerHand.Left;
        }
        data.events = controllerEvents;
        data.highligher = controllerEvents.AddComponentOnce<VRTK_ControllerHighlighter>();
        data.tooltips = controllerEvents.transform.FindFirstComponentInChild<VRTK_ControllerTooltips>();

        return data;
    }

    #endregion


    #region Define

    [System.Serializable]
    public class ControllerAppearanceData
    {
        public ControllerHand controllerHand = ControllerHand.None;//控制器的标识
        public VRTK_ControllerEvents events;
        public VRTK_ControllerHighlighter highligher;
        public VRTK_ControllerTooltips tooltips;//提示，可以为空
    }

    #endregion

#endif
}

