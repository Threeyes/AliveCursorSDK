using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
#if USE_OpenXR
using static UnityEngine.InputSystem.InputAction;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
#elif USE_VRTK
using VRTK;
#endif
#if USE_VIU
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.VRModuleManagement;
#endif
#if USE_PicoMobileSDK
using Pvr_UnitySDKAPI;
#endif
/// <summary>
/// 管理控制器
/// </summary>
public abstract class VRControllerHelperBase : MonoBehaviour
{
    public bool activeOnHide = false;//在隐藏时仍可用
    public ControllerCheckType controllerCheckType = ControllerCheckType.Both;

#if USE_PicoMobileSDK
    public ControllerVariety controllerVariety;
#endif


    private void Awake()
    {
        AddListeners();
    }
    private void OnDestroy()
    {
        RemoveListeners();//避免该Component被删除后依然会被委托调用
    }

    protected virtual void AddListeners() { }

    protected virtual void RemoveListeners() { }

    #region Utility

    /// <summary>
    ///  检查传入的Controller是否符合规格
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    protected virtual bool IsValiable
#if USE_OpenXR
(CallbackContext callbackContext)
#elif  USE_VRTK
  (ControllerInteractionEventArgs e)
#elif USE_VIU
(System.Type roleType, int roleValue, ControllerButton button, ButtonEventType eventType)
#elif USE_PicoMobileSDK
(int hand)//0左手，1右手
#else
()
#endif
    {
        try
        {
            if (!this)
                return false;
            if (gameObject.IsNull())
                return false;
            //该物体隐藏时不调用，适用于只有激活才调用
            if (!activeOnHide && !gameObject.activeInHierarchy)
                return false;

            bool isMatch = false;

#if USE_OpenXR
            InputControl control = callbackContext.control;
            if (control == null)
                return false;

            var actionMap = callbackContext.action.actionMap;//RightHand or LeftHand
            switch (controllerCheckType)
            {
                case ControllerCheckType.Both:
                    isMatch = true; break;
                case ControllerCheckType.Left:
                    isMatch = actionMap.name.Contains("Left"); break;
                case ControllerCheckType.Right:
                    isMatch = actionMap.name.Contains("Right"); break;
                case ControllerCheckType.ParentController:
                    XRController xRController = transform.GetComponentInParent<XRController>();
                    if (xRController)
                    {
                        if ((xRController.controllerNode == UnityEngine.XR.XRNode.LeftHand) && callbackContext.action.actionMap.name.Contains("Left") ||
                            (xRController.controllerNode == UnityEngine.XR.XRNode.RightHand) && callbackContext.action.actionMap.name.Contains("Right"))
                            isMatch = true;
                    }
                    break;
            }

#elif USE_VRTK
            object sender = e.controllerReference;
            if (sender == null)
                return false;
            VRTK_ControllerReference controllerReference = sender as VRTK_ControllerReference;
            VRTK_ControllerEvents controllerEvents = controllerReference.scriptAlias.GetComponent<VRTK_ControllerEvents>();
            if (controllerEvents)
            {
                switch (controllerCheckType)
                {
                    case ControllerCheckType.Both:
                        isMatch = true; break;
                    case ControllerCheckType.Left:
                        isMatch = controllerEvents == VRInterface.leftControllerEvent; break;
                    case ControllerCheckType.Right:
                        isMatch = controllerEvents == VRInterface.rightControllerEvent; break;

                    //ToTest(from VRControllerVibrationHelper)
                    case ControllerCheckType.TouchingThisObject:
                        VRTK_InteractableObject interactableObject = GetComponentInParent<VRTK_InteractableObject>();
                        if (interactableObject)
                        {
                            foreach (var go in interactableObject.GetTouchingObjects())
                            {
                                VRTK_ControllerEvents controllerEventTouching = go.GetComponent<VRTK_ControllerEvents>();
                                if (controllerEventTouching)
                                {
                                    if (controllerEvents == controllerEventTouching)
                                    {
                                        isMatch = true;
                                    }
                                }
                            }
                        }
                        break;
                    case ControllerCheckType.ParentController:
                        if (transform.IsChildOf(controllerEvents.transform))
                            isMatch = true;
                        break;
                }
            }
#elif USE_VIU
            if (!VRInterface.IsHandRole(roleType))//判断是否为手部模型
                return false;

            switch (controllerCheckType)
            {
                case ControllerCheckType.Both:
                    isMatch = true; break;
                case ControllerCheckType.Left:
                    isMatch = VRInterface.IsHandRole_Left(roleValue); break;
                case ControllerCheckType.Right:
                    isMatch = VRInterface.IsHandRole_Right(roleValue); break;
            }

#elif USE_PicoMobileSDK
            switch (controllerCheckType)
            {
                case ControllerCheckType.Both:
                    isMatch = true;
                    break;
                case ControllerCheckType.Left:
                    isMatch = hand == 0; break;
                case ControllerCheckType.Right:
                    isMatch = hand == 1; break;
                case ControllerCheckType.ParentController:
                    Pvr_ControllerModuleInit pvr_ControllerModuleInit = transform.FindFirstComponentInParent<Pvr_ControllerModuleInit>();
                    isMatch = pvr_ControllerModuleInit.NotNull() && pvr_ControllerModuleInit.Variety == controllerVariety;
                    break;
            }
#endif
            return isMatch;
        }

        catch (System.Exception exc)
        {
            Debug.LogError(exc);
            return false;
        }
    }

    //#
    /// <summary>
    /// 获取对应的手柄脚本引用（为了跨平台，只能返回Component）
    /// </summary>
    /// <returns></returns>
    public List<Component> GetControllers()
    {
        return GetControllers(controllerCheckType);
    }

    public List<Component> listControllerTest = new List<Component>();

    /// <summary>
    /// Todo:提取成static通用方法
    /// </summary>
    /// <param name="allowedController"></param>
    /// <returns></returns>
    protected virtual List<Component> GetControllers(ControllerCheckType allowedController)
    {
        List<Component> listController = new List<Component>();
        switch (allowedController)
        {
            case ControllerCheckType.Both:
                listController.Add(VRInterface.leftControllerRef);
                listController.Add(VRInterface.rightControllerRef);
                break;
            case ControllerCheckType.Left:
                listController.Add(VRInterface.leftControllerRef); break;
            case ControllerCheckType.Right:
                listController.Add(VRInterface.rightControllerRef); break;

#if USE_OpenXR
            case ControllerCheckType.TouchingThisObject:
                //To Impl
                break;
            case ControllerCheckType.ParentController:
                var controller = transform.FindFirstComponentInParent<XRController>(false);
                if (controller != null)
                {
                    listController.Add(controller);
                }
                break;
#elif USE_VRTK
            case ControllerCheckType.TouchingThisObject:
                VRTK_InteractableObject interactableObject = GetComponentInParent<VRTK_InteractableObject>();
                if (interactableObject)
                {
                    foreach (var go in interactableObject.GetTouchingObjects())
                    {
                        VRTK_ControllerEvents controllerEvents = go.GetComponent<VRTK_ControllerEvents>();
                        if (controllerEvents)
                            listController.Add(controllerEvents);
                    }
                }
                break;
            case ControllerCheckType.ParentController:
                listController.Add(GetComponentInParent<VRTK_ControllerEvents>()); break;
#endif
        }
        listControllerTest = listController;
        return listController;
    }



    #endregion
}
