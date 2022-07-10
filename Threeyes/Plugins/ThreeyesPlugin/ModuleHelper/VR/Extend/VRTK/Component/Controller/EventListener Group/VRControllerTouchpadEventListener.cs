using UnityEngine;
using System;
using UnityEngine.Events;
using System.Collections.Generic;
#if USE_OpenXR
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
#elif USE_VRTK
using VRTK;
#endif
#if USE_VIU
using HTC.UnityPlugin.Vive;
#endif
#if USE_PicoMobileSDK
using Pvr_UnitySDKAPI;
#endif

/// <summary>
/// 查看触摸盘的各种事件
/// https://blog.csdn.net/qq_14942529/article/details/81517158
/// 角度说明：上为0，顺时针加角度
/// Axis：右上为正
/// Todo:（针对Neo2、3）检测TouchPad是否为摇杆，如果是那就不需要按下也能触发，而且是到达阈值后只触发一次
/// </summary>
public class VRControllerTouchpadEventListener : VRControllerEventListenerBase
{
    public BoolEvent onTouchpadTouch;//触碰
    public UnityEvent onTouchPress;//点击
    public BoolEvent onTouchPressDownUp;//点击
    public FloatEvent onXAxisChanged;
    public FloatEvent onYAxisChanged;
    public Vector2Event onAxisChanged;

    public FloatEvent onAngleChanged;//当前的角度值
    public FloatEvent onDeltaAngleChanged;//角度差值
    public float stepAngle = -1;//每步的角度
    public BoolEvent onAngleStepChanged;//步进

    //[Header("VIU 的设置")]
    public bool isActiveAxisChangedOnTouch = false;//触摸时调用轴向事件（持续状态）
    public bool isActiveAxisChangedOnPress = true;//按下时调用轴向事件

    bool isTouchpadTouchStart = false;//圆盘被触碰
    bool isTouchpadPressStart = false;//圆盘被按下
    protected override void AddListeners()
    {
#if USE_OpenXR

        listAxisControllerInfo =
        new List<ControllerInfo>()
        {
            new ControllerInfo("LeftHand/Primary2DAxis"),
            new ControllerInfo("RightHand/Primary2DAxis"),
        };
        RegistFunc(listAxisControllerInfo, true, OnAxis);

        listButtonControllerInfo =
        new List<ControllerInfo>()
        {
            new ControllerInfo( "LeftHand/Primary2DAxisTouch",onTouchpadTouch),
            new ControllerInfo("RightHand/Primary2DAxisTouch",onTouchpadTouch),
            new ControllerInfo( "LeftHand/Primary2DAxisClick",onTouchPressDownUp),
            new ControllerInfo("RightHand/Primary2DAxisClick",onTouchPressDownUp),
        };
        RegistFunc(listButtonControllerInfo, true, OnButton);

#elif  USE_VRTK
        VRInterface.leftControllerEvent.TouchpadTouchStart += DoTouchpadTouchStart;
        VRInterface.rightControllerEvent.TouchpadTouchStart += DoTouchpadTouchStart;
        VRInterface.leftControllerEvent.TouchpadTouchEnd += DoTouchpadTouchEnd;
        VRInterface.rightControllerEvent.TouchpadTouchEnd += DoTouchpadTouchEnd;

        VRInterface.leftControllerEvent.TouchpadPressed += DoTouchpadTouchClick;
        VRInterface.rightControllerEvent.TouchpadPressed += DoTouchpadTouchClick;
        VRInterface.leftControllerEvent.TouchpadReleased += DoTouchpadTouchRelease;
        VRInterface.rightControllerEvent.TouchpadReleased += DoTouchpadTouchRelease;



        VRInterface.leftControllerEvent.TouchpadAxisChanged += DoTouchpadAxisChanged;
        VRInterface.rightControllerEvent.TouchpadAxisChanged += DoTouchpadAxisChanged;
#elif USE_VIU
    //当ControllerButton.PadTouch时，GetPressDown是(非按下)轻触时触发，GetPressUp是（(非按下)轻触时触发，GetPress是接触时一直返回的状态。 
    //当ControllerButton.Pad才是监听按下的操作
    https://blog.csdn.net/yuanpan/article/details/82147312

        ViveInput.AddListenerEx(HandRole.RightHand, ControllerButton.Pad, ButtonEventType.Click, DoTouchpadTouchClick);

        ViveInput.AddListenerEx(HandRole.RightHand, ControllerButton.PadTouch, ButtonEventType.Down, DoTouchpadTouchStart);
        ViveInput.AddListenerEx(HandRole.LeftHand, ControllerButton.PadTouch, ButtonEventType.Down, DoTouchpadTouchStart);

        ViveInput.AddListenerEx(HandRole.RightHand, ControllerButton.PadTouch, ButtonEventType.Up, DoTouchpadTouchEnd);
        ViveInput.AddListenerEx(HandRole.LeftHand, ControllerButton.PadTouch, ButtonEventType.Up, DoTouchpadTouchEnd);

        ViveInput.AddListenerEx(HandRole.RightHand, ControllerButton.PadTouch, ButtonEventType.Press, DoTouchpadAxisChanged);
        ViveInput.AddListenerEx(HandRole.LeftHand, ControllerButton.PadTouch, ButtonEventType.Press, DoTouchpadAxisChanged);

        //ViveInput.AddListenerEx(HandRole.RightHand, ControllerButton.PadTouch, ButtonEventType.Press, DoTouchpadAxisChanged);
        //ViveInput.AddListenerEx(HandRole.LeftHand, ControllerButton.PadTouch, ButtonEventType.Press, DoTouchpadAxisChanged);
#endif
    }


    protected override void RemoveListeners()
    {
#if USE_OpenXR
        RegistFunc(listAxisControllerInfo, false, OnAxis);
        RegistFunc(listButtonControllerInfo, false, OnButton);

#elif  USE_VRTK
        VRInterface.leftControllerEvent.TouchpadTouchStart -= DoTouchpadTouchStart;
        VRInterface.rightControllerEvent.TouchpadTouchStart -= DoTouchpadTouchStart;
        VRInterface.leftControllerEvent.TouchpadTouchEnd -= DoTouchpadTouchEnd;
        VRInterface.rightControllerEvent.TouchpadTouchEnd -= DoTouchpadTouchEnd;

        VRInterface.leftControllerEvent.TouchpadPressed -= DoTouchpadTouchClick;
        VRInterface.rightControllerEvent.TouchpadPressed -= DoTouchpadTouchClick;
        VRInterface.leftControllerEvent.TouchpadReleased -= DoTouchpadTouchRelease;
        VRInterface.rightControllerEvent.TouchpadReleased -= DoTouchpadTouchRelease;



        //VRInterface.leftControllerEvent.TouchpadAxisChanged -= new ControllerInteractionEventHandler(DoTouchpadAxisChanged);
        //VRInterface.rightControllerEvent.TouchpadAxisChanged -= new ControllerInteractionEventHandler(DoTouchpadAxisChanged);
#elif USE_VIU
        ViveInput.RemoveListenerEx(HandRole.RightHand, ControllerButton.PadTouch, ButtonEventType.Click, DoTouchpadTouchClick);

        ViveInput.RemoveListenerEx(HandRole.RightHand, ControllerButton.PadTouch, ButtonEventType.Down, DoTouchpadTouchStart);
        ViveInput.RemoveListenerEx(HandRole.LeftHand, ControllerButton.PadTouch, ButtonEventType.Down, DoTouchpadTouchStart);
        ViveInput.RemoveListenerEx(HandRole.RightHand, ControllerButton.PadTouch, ButtonEventType.Up, DoTouchpadTouchEnd);
        ViveInput.RemoveListenerEx(HandRole.LeftHand, ControllerButton.PadTouch, ButtonEventType.Up, DoTouchpadTouchEnd);
#endif
    }

    #region Common

#if USE_OpenXR

    public List<ControllerInfo> listAxisControllerInfo = new List<ControllerInfo>();
    protected void OnAxis(CallbackContext callbackContext, ControllerInfo controllerInfo)
    {
        if (!IsValiable(callbackContext))
            return;

        Type typeToUse = callbackContext.control.valueType;
        Vector2 value = default(Vector2);
        if (typeToUse == typeof(Vector2))
        {
            value = callbackContext.action.ReadValue<Vector2>();
            if (callbackContext.started)
            {
                //Todo：初始化， 不作计算
            }
            else if (callbackContext.performed)
            {
                ////Todo：计算角度
                //Debug.Log("touchpadAxis: " + value + " " + Vector2.Angle(value, Vector2.up));
            }

            var curAction = callbackContext.action;

            bool canInvokeEvent = false;

            //找到对应的Touch类实例
            if (isActiveAxisChangedOnTouch)
            {
                ControllerInfo controllerInfoTouch = listButtonControllerInfo.Find((cI) => cI.id.Contains(curAction.actionMap.name) && cI.id.Contains("Primary2DAxisTouch"));
                if (controllerInfoTouch != null && controllerInfoTouch.isPressed)
                    canInvokeEvent = true;
            }
            if (isActiveAxisChangedOnPress)
            {
                ControllerInfo controllerInfoClick = listButtonControllerInfo.Find((cI) => cI.id.Contains(curAction.actionMap.name) && cI.id.Contains("Primary2DAxisClick"));
                if (controllerInfoClick != null)
                {
                    if (!controllerInfo.isPressed&& controllerInfoClick.isPressed)//仅在第一次点击时调用
                    {
                        canInvokeEvent = true;
                    }
                    controllerInfo.isPressed = controllerInfoClick.isPressed;
                }
            }

            //轴角度
            if (canInvokeEvent)
            {
                onXAxisChanged.Invoke(value.x);
                onYAxisChanged.Invoke(value.y);
                onAxisChanged.Invoke(value);
                //Debug.Log("canInvokeEvent" + value);
            }

            controllerInfo.phase = callbackContext.phase;
            controllerInfo.vector2Value = value;//记录角度
        }
    }

#endif



    float lastAngle;
    float lastStepAngle;
    float curAngle;


    /// <summary>
    /// Click Up 的事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>

    private void DoTouchpadTouchRelease
#if USE_VRTK
    (object sender, ControllerInteractionEventArgs e)
#else
     ()
#endif
    {
#if USE_VRTK
        //Todo
        if (!IsValiable(e))
            return;
#endif

        //VRTK有这个事件，VIU没有
        onTouchPressDownUp.Invoke(false);
        isTouchpadPressStart = false;
    }

    /// <summary>
    /// Click Down 的事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void DoTouchpadTouchClick
#if USE_VRTK
    (object sender, ControllerInteractionEventArgs e)
#elif USE_VIU
     (Type roleType, int roleValue, ControllerButton button, ButtonEventType eventType)
#else
     ()
#endif
    {
#if USE_VRTK
        //Todo
        if (!IsValiable(e))
            return;
        if (isActiveAxisChangedOnPress)
        {
            curAngle = e.touchpadAngle;
            Vector2 touchpadAxis = e.touchpadAxis;//-1 ~ 1
            SetData(touchpadAxis);
        }

#elif USE_VIU
        if (!IsValiable(roleType, roleValue, button, eventType))
            return;

        if (isActiveAxisChangedOnPress)
        {
            Debug.LogError("DoTouchpadTouchClick");
            //按下时自行调用方法。因为监听的事件方法调用太频繁SetData(ViveInput.GetPadTouchAxisEx(roleType, roleValue));
            SetData(ViveInput.GetPadTouchAxisEx(roleType, roleValue));
        }
#endif

        onTouchPress.Invoke();
        onTouchPressDownUp.Invoke(true);
        isTouchpadPressStart = true;
    }


    //Touch
    private void DoTouchpadTouchStart
#if USE_VRTK
        (object sender, ControllerInteractionEventArgs e)
#elif USE_VIU
        (Type roleType, int roleValue, ControllerButton button, ButtonEventType eventType)
#elif USE_PicoMobileSDK
    (int hand)//0左手，1右手
#else
     ()
#endif
    {

#if USE_VRTK
        if (!IsValiable(e))
            return;
        lastAngle = e.touchpadAngle;
#elif USE_VIU
        if (!IsValiable(roleType, roleValue, button, eventType))
            return;

        //lastAngle = e.touchpadAngle;//Todo
        if (isActiveAxisChangedOnTouch)
        {
            Debug.LogError("DoTouchpadTouchStart");
            //触碰时自行调用方法。因为监听的事件方法调用太频繁
            SetData(ViveInput.GetPadTouchAxisEx(roleType, roleValue));
        }
#endif

        //Debug.LogError("DoTouchpadTouch Down");
        lastStepAngle = lastAngle;
        onTouchpadTouch.Invoke(true);
        isTouchpadTouchStart = true;
    }

    private void DoTouchpadTouchEnd
#if USE_VRTK
    (object sender, ControllerInteractionEventArgs e)
#elif USE_VIU
     (Type roleType, int roleValue, ControllerButton button, ButtonEventType eventType)
#else
     ()
#endif
    {
#if USE_VRTK
        if (!IsValiable(e))
            return;
#elif USE_VIU
        if (!IsValiable(roleType, roleValue, button, eventType))
            return;
#endif

        onTouchpadTouch.Invoke(false);
        isTouchpadTouchStart = false;

#if USE_VIU
        onTouchPressDownUp.Invoke(false);
        isTouchpadPressStart = false;
#endif
    }

    //Change
    private void DoTouchpadAxisChanged
#if USE_VRTK
    (object sender, ControllerInteractionEventArgs e)
#elif USE_VIU
        (Type roleType, int roleValue, ControllerButton button, ButtonEventType eventType)
#elif USE_PicoMobileSDK
    (int hand, Vector2 curTouchpadAxis)//0左手，1右手
#else
     ()
#endif
    {
        Vector2 touchpadAxis = default(Vector2);
#if USE_VRTK
        if (!IsValiable(e))
            return;

        if (!isActiveAxisChangedOnTouch)//持续调用
            return;

        if (isActiveAxisChangedOnPress)//在Click的时候调用
        {
            return;
        }


        curAngle = e.touchpadAngle;
        touchpadAxis = e.touchpadAxis;//-1 ~ 1

#elif USE_VIU

        if (!isActiveAxisChangedOnTouch)
            return;

        //PS:按下圆盘边缘的时候，返回值为(0,0)。因此建议用户只按压中心
        touchpadAxis = ViveInput.GetPadTouchAxisEx(roleType, roleValue);

#elif USE_PicoMobileSDK

        if (!IsValiable(hand))
            return;
        //curAngle = e.touchpadAngle;
        touchpadAxis = curTouchpadAxis;

#endif

        SetData(touchpadAxis);
    }

    /// <summary>
    /// 通用的更新Axis信息
    /// </summary>
    /// <param name="touchpadAxis"></param>
    private void SetData(Vector2 touchpadAxis)
    {
        onAngleChanged.Invoke(curAngle);//当前角度

        float deltaAngle = curAngle.DeltaAngle(lastAngle);
        onDeltaAngleChanged.Invoke(deltaAngle);//增减的角度

        //步进角度
        if (stepAngle > 0)
        {
            float deltaAngleSinceLastStepChanged = curAngle.DeltaAngle(lastStepAngle);
            if (Mathf.Abs(deltaAngleSinceLastStepChanged) > stepAngle)
            {
                float sign = Mathf.Sign(deltaAngleSinceLastStepChanged);
                float deltaSetpAngle = sign * stepAngle;
                onAngleStepChanged.Invoke(sign > 0 ? true : false);//+/-
                lastStepAngle = curAngle + deltaSetpAngle;
            }
        }

        //轴角度
        onXAxisChanged.Invoke(touchpadAxis.x);
        onYAxisChanged.Invoke(touchpadAxis.y);
        onAxisChanged.Invoke(touchpadAxis);
        //Debug.LogError("touchpadAxis: " + touchpadAxis.y);
        lastAngle = curAngle;
    }

    #endregion

    #region Utility

    float CalDeltaAngle(float lastAngle, float nextAngle)
    {
        float sign = Mathf.Sign(nextAngle - lastAngle);//是否正向
        float deltaAngle = Mathf.Abs(nextAngle - lastAngle);

        //越过0度分界点 
        if (deltaAngle > 180)
        {
            deltaAngle = 360 - deltaAngle;
            sign *= -1;
        }

        return sign * deltaAngle;
    }

    #endregion

    protected override void Update()
    {
        base.Update();

#if USE_PicoMobileSDK

        var touchpadAxis0 = Controller.UPvr_GetAxis2D(0);
        if (touchpadAxis0 != Vector2.zero)
        {
            DoTouchpadAxisChanged(0, touchpadAxis0);
        }

        var touchpadAxis1 = Controller.UPvr_GetAxis2D(1);
        if (touchpadAxis1 != Vector2.zero)
        {
            DoTouchpadAxisChanged(1, touchpadAxis1);
        }

#endif

        #region Editor

        if (Application.isEditor)
        {
            SimulatorKeyToEvent(KeyCode.Alpha2, new List<BoolEvent>() { onTouchPressDownUp }, onTouchPress, 1);

            if (Input.GetMouseButtonDown(1))
            {
                onTouchpadTouch.Invoke(true);
                onTouchPress.Invoke();
            }
            if (Input.GetMouseButtonUp(1))
            {
                onTouchpadTouch.Invoke(false);
            }


        }
        #endregion
    }

}
