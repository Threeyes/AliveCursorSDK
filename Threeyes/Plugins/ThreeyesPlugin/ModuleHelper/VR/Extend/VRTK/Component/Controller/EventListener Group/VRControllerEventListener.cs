using UnityEngine.Events;
using UnityEngine;
using System;
using System.Collections.Generic;
#if  USE_VRTK
using VRTK;
#elif USE_VIU
using HTC.UnityPlugin.Vive;
#elif USE_PicoMobileSDK
using Pvr_UnitySDKAPI;
#endif

/// <summary>
/// 监听特定Controller常用按键的事件
/// ——Trigger按键
/// ——Menu  按键
/// </summary>
public class VRControllerEventListener : VRControllerEventListenerBase
{
    public UnityEvent onTriggerPress;//按下Trigger键
    public FloatEvent onTriggerPercent;//按下Trigger键的百分比

    public BoolEvent onTriggerPressDownUp;//按下及抬起
    public UnityEvent onMenuPress;//按下Menu键
    public UnityEvent onGripPress;//按下Grip键

#if USE_PicoMobileSDK
    public UnityEvent OnAPress;
    public UnityEvent OnBPress;
    public UnityEvent OnXPress;
    public UnityEvent OnYPress;
#endif

    protected override void AddListeners()
    {
#if USE_OpenXR

        //实现1：InputAction
        listButtonControllerInfo =
        new List<ControllerInfo>()
        {
            new ControllerInfo( "LeftHand/Trigger",onDownPercent:onTriggerPercent),
            new ControllerInfo( "RightHand/Trigger",onDownPercent:onTriggerPercent),
            new ControllerInfo( "LeftHand/TriggerPress",onTriggerPressDownUp,onTriggerPress),
            new ControllerInfo("RightHand/TriggerPress",onTriggerPressDownUp,onTriggerPress),
   
            //PS:Menu和Grip是按钮，没有百分比
            new ControllerInfo("LeftHand/Menu",onDown:onMenuPress),
            new ControllerInfo("RightHand/Menu",onDown:onMenuPress),
            new ControllerInfo("LeftHand/GripPress",onDown:onGripPress),
            new ControllerInfo("RightHand/GripPress",onDown:onGripPress)
        };
        RegistFunc(listButtonControllerInfo, true, OnButton);

#elif USE_VRTK
        VRInterface.leftControllerEvent.TriggerPressed += (s, e) => DoTrigger(s, e, true);
        VRInterface.rightControllerEvent.TriggerPressed += (s, e) => DoTrigger(s, e, true);
        VRInterface.leftControllerEvent.TriggerReleased += (s, e) => DoTrigger(s, e, false);
        VRInterface.rightControllerEvent.TriggerReleased += (s, e) => DoTrigger(s, e, false);

        VRInterface.leftControllerEvent.ButtonTwoPressed += DoMenuPressed;
        VRInterface.rightControllerEvent.ButtonTwoPressed += DoMenuPressed;
        VRInterface.leftControllerEvent.GripPressed += DoGripPressed;
        VRInterface.rightControllerEvent.GripPressed += DoGripPressed;


#elif USE_VIU
        //不监听Press，因为按下的时候执行次数太频繁。Click代表在特定时间间隔内按下按键并抬起
        ViveInput.AddListenerEx(HandRole.RightHand, ControllerButton.Trigger, ButtonEventType.Click, DoTriggerPressed);
        ViveInput.AddListenerEx(HandRole.LeftHand, ControllerButton.Trigger, ButtonEventType.Click, DoTriggerPressed);
        ViveInput.AddListenerEx(HandRole.RightHand, ControllerButton.Trigger, ButtonEventType.Down, DoTrigger);
        ViveInput.AddListenerEx(HandRole.LeftHand, ControllerButton.Trigger, ButtonEventType.Down, DoTrigger);
        ViveInput.AddListenerEx(HandRole.RightHand, ControllerButton.Trigger, ButtonEventType.Up, DoTrigger);
        ViveInput.AddListenerEx(HandRole.LeftHand, ControllerButton.Trigger, ButtonEventType.Up, DoTrigger);


        ViveInput.AddListenerEx(HandRole.LeftHand, ControllerButton.Menu, ButtonEventType.Click, DoMenuPressed);
        ViveInput.AddListenerEx(HandRole.RightHand, ControllerButton.Menu, ButtonEventType.Click, DoMenuPressed);
#endif
    }

    protected override void RemoveListeners()
    {
#if USE_OpenXR
        RegistFunc(listButtonControllerInfo, false, OnButton);
#elif USE_VRTK

        VRInterface.leftControllerEvent.TriggerPressed -= (s, e) => DoTrigger(s, e, true);
        VRInterface.rightControllerEvent.TriggerPressed -= (s, e) => DoTrigger(s, e, true);
        VRInterface.leftControllerEvent.TriggerReleased -= (s, e) => DoTrigger(s, e, false);
        VRInterface.rightControllerEvent.TriggerReleased -= (s, e) => DoTrigger(s, e, false);

        VRInterface.leftControllerEvent.ButtonTwoPressed -= DoMenuPressed;
        VRInterface.rightControllerEvent.ButtonTwoPressed -= DoMenuPressed;
        VRInterface.leftControllerEvent.GripPressed -= DoGripPressed;
        VRInterface.rightControllerEvent.GripPressed -= DoGripPressed;

#elif USE_VIU
        ViveInput.RemoveListenerEx(HandRole.RightHand, ControllerButton.Trigger, ButtonEventType.Click, DoTriggerPressed);
        ViveInput.RemoveListenerEx(HandRole.LeftHand, ControllerButton.Trigger, ButtonEventType.Click, DoTriggerPressed);
        ViveInput.RemoveListenerEx(HandRole.RightHand, ControllerButton.Trigger, ButtonEventType.Down, DoTrigger);
        ViveInput.RemoveListenerEx(HandRole.LeftHand, ControllerButton.Trigger, ButtonEventType.Down, DoTrigger);
        ViveInput.RemoveListenerEx(HandRole.RightHand, ControllerButton.Trigger, ButtonEventType.Up, DoTrigger);
        ViveInput.RemoveListenerEx(HandRole.LeftHand, ControllerButton.Trigger, ButtonEventType.Up, DoTrigger);

        ViveInput.RemoveListenerEx(HandRole.LeftHand, ControllerButton.Menu, ButtonEventType.Click, DoMenuPressed);
        ViveInput.RemoveListenerEx(HandRole.RightHand, ControllerButton.Menu, ButtonEventType.Click, DoMenuPressed);
#endif
    }

    #region 按下的事件（ToDO：整合到按下抬起中）


    private void DoMenuPressed
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

#elif USE_VIU
        if (!IsValiable(roleType, roleValue, button, eventType))
            return;

#elif USE_PicoMobileSDK
        if (!IsValiable(hand))
            return;
#endif

        onMenuPress.Invoke();
    }

    private void DoGripPressed
#if USE_VRTK
   (object sender, ControllerInteractionEventArgs e)
#elif USE_PicoMobileSDK
    (int hand)//0左手，1右手
#else
     ()
#endif

    {
#if USE_VRTK
        if (!IsValiable(e))
            return;

#elif USE_PicoMobileSDK
        if (!IsValiable(hand))
            return;
#endif

        onGripPress.Invoke();
    }


    /// <summary>
    /// 按下
    /// </summary>
    /// <param name="hand"></param>
    private void DoTriggerPressed
#if USE_VIU
    (Type roleType, int roleValue, ControllerButton button, ButtonEventType eventType)
#elif USE_PicoMobileSDK
    (int hand)//0左手，1右手
#else
     ()
#endif
    {
#if USE_VIU
        if (!IsValiable(roleType, roleValue, button, eventType))
            return;

#elif USE_PicoMobileSDK
        if (!IsValiable(hand))
            return;
#endif

        onTriggerPress.Invoke();
    }

    /// <summary>
    /// 按下/抬起
    /// </summary>
    /// <param name="hand"></param>
    /// <param name="isDown"></param>
    private void DoTrigger
#if USE_VRTK
   (object sender, ControllerInteractionEventArgs e, bool isDown)
#elif USE_VIU
   (Type roleType, int roleValue, ControllerButton button, ButtonEventType eventType)
#elif USE_PicoMobileSDK
    (int hand, bool isDown)//0左手，1右手
#else
     ()
#endif
    {
#if USE_VRTK
        if (!IsValiable(e))
            return;

        if (isDown)
        {
            onTriggerPress.Invoke();
        }
        onTriggerPressDownUp.Invoke(isDown);

#elif  USE_VIU
        if (!IsValiable(roleType, roleValue, button, eventType))
            return;

        switch (eventType)
        {
            case ButtonEventType.Down:
                onTriggerPressDownUp.Invoke(true); break;
            case ButtonEventType.Up:
                onTriggerPressDownUp.Invoke(false); break;
        }
#elif USE_PicoMobileSDK
        if (!IsValiable(hand))
            return;
        onTriggerPressDownUp.Invoke(isDown);
#endif
    }

    #endregion



    #region Editor
    public float pressThreshold = 1f;
    protected override void Update()
    {
        base.Update();
        if (Application.isEditor)
        {
            SimulatorKeyToEvent(KeyCode.M, null, onMenuPress);

            if (controllerCheckType == ControllerCheckType.Left)//左手柄
            {
                //Trigger
                SimulatorKeyToEvent(KeyCode.Alpha1, new List<BoolEvent>() { onTriggerPressDownUp }, onTriggerPress);
                SimulatorKeyToEvent(KeyCode.Keypad1, new List<BoolEvent>() { onTriggerPressDownUp }, onTriggerPress);

                SimulatorKeyToEvent(KeyCode.Alpha3, null, onTriggerPress);
                SimulatorKeyToEvent(KeyCode.Keypad3, null, onTriggerPress);

            }
            if (controllerCheckType == ControllerCheckType.Right)//右手柄
            {
                SimulatorKeyToEvent(KeyCode.Alpha2, new List<BoolEvent>() { onTriggerPressDownUp }, onTriggerPress);
                SimulatorKeyToEvent(KeyCode.Keypad2, new List<BoolEvent>() { onTriggerPressDownUp }, onTriggerPress);

                SimulatorKeyToEvent(KeyCode.Alpha4, null, onTriggerPress);
                SimulatorKeyToEvent(KeyCode.Keypad4, null, onTriggerPress);
            }

        }


#if USE_PicoMobileSDK

        for (int i = 0; i != 2; i++)
        {
            if (Controller.UPvr_GetKeyDown(i, Pvr_KeyCode.TRIGGER))
            {
                DoTriggerPressed(i);
                DoTrigger(i, true);
            }
            if (Controller.UPvr_GetKeyUp(i, Pvr_KeyCode.TRIGGER))
            {
                DoTriggerPressed(i);
                DoTrigger(i, false);
            }
        }

        if (Controller.UPvr_GetKeyDown(0, Pvr_KeyCode.APP))
            DoMenuPressed(0);
        if (Controller.UPvr_GetKeyDown(1, Pvr_KeyCode.APP))
            DoMenuPressed(1);

        if (Controller.UPvr_GetKeyDown(0, Pvr_KeyCode.Left))
            DoGripPressed(0);
        if (Controller.UPvr_GetKeyDown(1, Pvr_KeyCode.Right))
            DoGripPressed(1);

        if (Controller.UPvr_GetKeyDown(0, Pvr_KeyCode.X))
            OnXPress.Invoke();
        if (Controller.UPvr_GetKeyDown(0, Pvr_KeyCode.Y))
            OnYPress.Invoke();
        if (Controller.UPvr_GetKeyDown(1, Pvr_KeyCode.A) || Input.GetKeyDown(KeyCode.O))
            OnAPress.Invoke();
        if (Controller.UPvr_GetKeyDown(1, Pvr_KeyCode.B))
            OnBPress.Invoke();

#endif

    }

    #endregion
}

