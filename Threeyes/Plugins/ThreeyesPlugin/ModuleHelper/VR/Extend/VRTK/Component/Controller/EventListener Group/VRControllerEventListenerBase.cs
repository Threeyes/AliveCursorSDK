
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if USE_OpenXR
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR.Interaction.Toolkit;
using static UnityEngine.InputSystem.InputAction;
#endif
/// <summary>
/// 监听控制器的事件
/// </summary>
public abstract class VRControllerEventListenerBase : VRControllerHelperBase
{
    #region Defines
    
    [Header("监听按键百分比")]
    public bool isActivePercent = false;//为了性能原因，默认不监听持续的事件

#if USE_OpenXR
    protected InputActionAsset inputAction { get { return VRInterface.Instance.InputActionAsset; } }

    public List<ControllerInfo> listButtonControllerInfo = new List<ControllerInfo>();


    /// <summary>
    /// （Todo：将其他方法整合到这里）
    /// （Todo：针对按键，在Update里重置ispress并调用up事件）
    /// </summary>
    /// <param name="callbackContext"></param>
    protected virtual void OnButton(CallbackContext callbackContext, ControllerInfo controllerInfo)
    {
        if (!IsValiable(callbackContext))
            return;

        ////ps:可针对设备名进行判断：
        //string deviceName = callbackContext.control.device.name;//eg:HTCViveControllerOpenXR
        //int deviceID = callbackContext.control.device.deviceId;

        //针对类型为float的Action (Todo：新增一个方法，专门处理float）
        if (isActivePercent)
        {
            Type typeToUse = callbackContext.control.valueType;
            if (typeToUse == typeof(float))//返回(0,1],不包括0
            {
                float curPressPercent = callbackContext.action.ReadValue<float>();
                controllerInfo.onDownPercent.Execute(curPressPercent);
                //if (!controllerInfo.isPressed && curPressPercent == 1)
                //{
                //    controllerInfo.isPressed = true;
                //    controllerInfo.onDownUp.Execute(true);
                //    controllerInfo.onDown.Execute();
                //}
                //if (controllerInfo.isPressed && curPressPercent != 1)//前一帧按下，当前帧抬起
                //{
                //    controllerInfo.isPressed = false;
                //    controllerInfo.onDownUp.Execute(false);
                //    controllerInfo.onUp.Execute();
                //}
            }
        }

        ////PS:这种实现也能工作，但是wasPressedThisFrame会调用多次
        //var button = callbackContext.control as ButtonControl;
        //if (button != null )
        //{
        //    if( button.wasPressedThisFrame)
        //    {
        //        Debug.Log(callbackContext.action.name + "wasPressedThisFrame");
        //    }
        //    if(button.wasReleasedThisFrame)
        //    {
        //        Debug.Log(callbackContext.action.name + "wasReleasedThisFrame");
        //    }
        //}

        if (callbackContext.started)
        {
            controllerInfo.isPressed = true;
            controllerInfo.onDownUp.Execute(true);
            controllerInfo.onDown.Execute();
        }
        else if (callbackContext.canceled)
        {
            controllerInfo.isPressed = false;
            controllerInfo.onDownUp.Execute(false);
            controllerInfo.onUp.Execute();

            if (isActivePercent)
            {
                controllerInfo.onDownPercent.Execute(0);//重置
            }
        }

        controllerInfo.phase = callbackContext.phase;
    }

    protected bool RegistFunc(List<ControllerInfo> listControllerInfo, bool isRegist, Action<CallbackContext, ControllerInfo> callback)
    {
        if (inputAction)
        {
            foreach (var item in listControllerInfo)
            {
                if (isRegist)
                {
                    inputAction[item.id].started += (cc) => callback(cc, item);
                    inputAction[item.id].performed += (cc) => callback(cc, item);
                    inputAction[item.id].canceled += (cc) => callback(cc, item);//Button抬起

                }
                else
                {
                    inputAction[item.id].started -= (cc) => callback(cc, item);
                    inputAction[item.id].performed -= (cc) => callback(cc, item);
                    inputAction[item.id].canceled -= (cc) => callback(cc, item);
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }


    /// <summary>
    /// 控制器的按键信息(Todo:将其细分为支持Vector2值得)
    /// Todo:变成通用
    /// </summary>
    [Serializable]
    public class ControllerInfo
    {
        public string id;//mapName/actionName (eg:LeftHand/Trigger)
        public bool isPressed = false;//在前一帧是否已经按下
        public InputActionPhase phase;
        public Vector2 vector2Value;

        public BoolEvent onDownUp;
        public UnityEvent onDown;
        public UnityEvent onUp;
        public FloatEvent onDownPercent;

        public ControllerInfo(string id, BoolEvent onDownUp = null, UnityEvent onDown = null, UnityEvent onUp = null, FloatEvent onDownPercent = null)
        {
            this.id = id;
            this.onDownUp = onDownUp;
            this.onDown = onDown;
            this.onUp = onUp;
            this.onDownPercent = onDownPercent;
        }
    }

#endif

    #endregion

    #region Unity Event

    protected virtual void Update()
    {
        //Test
        //XRController xRController = VRInterface.rightControllerRef as XRController;
        //if (xRController)
        //{
        //    bool isPressed = false;
        //    xRController.inputDevice.IsPressed(InputHelpers.Button.Trigger, out isPressed);
        //    Debug.Log(xRController + " " + isPressed);
        //}
    }

    #endregion

    #region Editor

    /// <summary>
    /// 模拟键位的按下及抬起
    /// </summary>
    /// <param name="keyCode"></param>
    /// <param name="listBoolEvent"></param>
    protected void SimulatorKeyToEvent(KeyCode keyCode, List<BoolEvent> listBoolEvent = null, UnityEvent unityEvent = null, int mouseButton = -1)
    {
        if (Input.GetKeyDown(keyCode) || mouseButton > -1 && Input.GetMouseButtonDown(mouseButton))
        {
            if (listBoolEvent.NotNull())
                foreach (var be in listBoolEvent)
                    be.Invoke(true);
            if (unityEvent.NotNull())
                unityEvent.Invoke();
        }
        else if (Input.GetKeyUp(keyCode) || mouseButton > -1 && Input.GetMouseButtonUp(mouseButton))
        {
            if (listBoolEvent.NotNull())
                foreach (var be in listBoolEvent)
                    be.Invoke(false);
        }
    }

    #endregion

}