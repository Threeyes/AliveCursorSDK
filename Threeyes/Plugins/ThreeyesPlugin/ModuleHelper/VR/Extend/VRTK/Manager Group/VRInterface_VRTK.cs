#if USE_VRTK
using UnityEngine;
using UnityEngine.Events;
using VRTK;

public partial class VRInterface : InstanceBase<VRInterface>
{
    static VRTK_SDKSetup loadedSetup { get { return VRTK_SDKManager.instance.loadedSetup; } }

    //VRTK_ControllerReference结构（其实就是VRTK_SDKSetup里定义的东西）：
    //-actual：Controller (left)
    //  -scriptAlias:LeftController  （[VRTK_Scripts]物体下的LeftController/RightController）A reference to the GameObject that contains any scripts that apply to the Left Hand Controller
    //  -model:Model（即Controller的模型，或直接自己赋值到ModelAliases-LeftController上的模型）
    public static GameObject goUISDKSetupSwitcher;//右上角的切换的显示设备的UI
    public static VRTK_ControllerEvents defaultControllerEvent { get { return rightControllerEvent ? rightControllerEvent : leftControllerEvent; } }//获取首个可用的控制器
    public static VRTK_ControllerEvents leftControllerEvent
    {
        get
        {
            if (!_leftControllerEvent)
            {
                if (VRTK_SDKManager.instance && VRTK_SDKManager.instance.scriptAliasLeftController)//避免物体被销毁
                    _leftControllerEvent = VRTK_SDKManager.instance.scriptAliasLeftController.GetComponent<VRTK_ControllerEvents>();
            }
            return _leftControllerEvent;
        }
        set
        {
            _leftControllerEvent = value;
        }
    }
    public static VRTK_ControllerEvents rightControllerEvent
    {
        get
        {
            //OnSceneLoaded时初始化
            if (!_rightControllerEvent)
            {
                if (VRTK_SDKManager.instance && VRTK_SDKManager.instance.scriptAliasRightController)
                    _rightControllerEvent = VRTK_SDKManager.instance.scriptAliasRightController.GetComponent<VRTK_ControllerEvents>();
            }
            return _rightControllerEvent;
        }
        set
        {
            _rightControllerEvent = value;
        }
    }
    private static VRTK_ControllerEvents _leftControllerEvent;
    private static VRTK_ControllerEvents _rightControllerEvent;

    [Header("RunTime Setting")]
    public bool isRunTimeDefaultShowUISDKSetupSwitcher = false;//发布后，默认显示 SDK切换UI

    [Header("Editor Setting")]
    public bool isEditorVibe = true;//编辑器中震动手柄
    public bool isEditorRBezierPointer = true;//编辑器中设置模擬器的右手为短距离的贝塞尔曲线
    public Vector2 editorBezierPointerMaxLength = new Vector2(1f, float.PositiveInfinity);//编辑器中贝塞尔曲线的最大值

    #region  partial Method

    static Transform GetTFCameraRigParentFunc()
    {
        return loadedSetup ? loadedSetup.transform : null;
    }
    static Transform GetTFCameraRigFunc()
    {
        return loadedSetup ? loadedSetup.actualBoundaries.transform : null;
    }
    static Transform GetTFCameraEyeFunc()
    {
        return loadedSetup ? loadedSetup.actualHeadset.transform : null;
    }
    static Camera GetVRCameraFunc()
    {
        return loadedSetup ? loadedSetup.actualHeadset.GetComponent<Camera>() : null;
    }
    static Component GetRightControllerRefFunc()
    {
        if (Instance.rightControllerRefCache.IsNull())
        {
            Instance.rightControllerRefCache = rightControllerEvent;
        }
        return Instance.rightControllerRefCache;
    }
    static Component GetLeftControllerRefFunc()
    {
        if (Instance.leftControllerRefCache.IsNull())
        {
            Instance.leftControllerRefCache = leftControllerEvent;
        }
        return Instance.leftControllerRefCache;
    }

    #endregion

    #region Unity Func

    private void AwakeFunc()
    {
        VRTK_SDKManager.instance.LoadedSetupChanged += OnLoadedSetupChanged;
        VRTK_SDKSetupSwitcher sDKSetupSwitcher = GetComponentInChildren<VRTK_SDKSetupSwitcher>();
        if (sDKSetupSwitcher)
            goUISDKSetupSwitcher = sDKSetupSwitcher.gameObject;

        if (!Application.isEditor)
            ShowSDKSetupSwitcher(isRunTimeDefaultShowUISDKSetupSwitcher);
    }



    protected void UpdateFunc()
    {
        //F10 ：开关切换菜单
        if (Input.GetKeyDown(KeyCode.F10))
            ToggleSDKSetupSwitcher();

#if UNITY_EDITOR
        //避免只有一个Controller的情况
        if (!leftControllerEvent || !rightControllerEvent)
            return;

        //两个手柄四个按键同时按下：重载场景
        if (leftControllerEvent.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.TriggerPress) &&
            leftControllerEvent.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.GripPress) &&
            rightControllerEvent.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.TriggerPress) &&
            rightControllerEvent.IsButtonPressed(VRTK_ControllerEvents.ButtonAlias.GripPress))
            EnvironmentManager.Instance.ReloadScene();

        //按下T,模拟按下手柄的触发键，常用于:点击开始下一步
        if (Input.GetKeyDown(KeyCode.T))
        {
            bool isPress = true;
            UnityAction<VRTK_ControllerEvents> action = ce => ce.OnTriggerPressed(ce.SetControllerEvent(ref isPress, true, 1));
            if (VRInterface.rightControllerEvent)
                VRInterface.rightControllerEvent.TryExecute(action);
            else
                VRInterface.leftControllerEvent.TryExecute(action);
        }
#endif
    }

    void OnDestroyFunc()
    {
        leftControllerRefCache = null;
        rightControllerRefCache = null;
    }


    #endregion

    #region Viberation

    public static void ViberationFunc(Component controllerEvents, AudioClip clip)
    {
        Viberation_VRTK(GetControllerReference(controllerEvents), clip);
    }

    /// <summary>
    /// 震动手柄(Wrong Spelling)
    /// </summary>
    /// <param name="controllerEvents"></param>
    /// <param name="vibValue"></param>
    public static void ViberationFunc(Component controllerEvents, float vibValue, float duration = 0, float pulseInterval = 0.01f)
    {
        if (!controllerEvents)
            return;

        Viberation_VRTK(GetControllerReference(controllerEvents), vibValue, duration, pulseInterval);
    }

    public static void StopViberationFunc(Component controllerEvents)
    {
        StopViberation_VRTK(GetControllerReference(controllerEvents));
    }

    #endregion

    #region Inner

    static void Viberation_VRTK(VRTK_ControllerReference controllerReference, float vibValue, float duration = 0, float pulseInterval = 0.01f)
    {
        if (!IsViberationValid_VRTK(controllerReference))
            return;

        StopViberation_VRTK(controllerReference);

        if (duration > 0)
            VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, vibValue, duration, pulseInterval);
        else
            VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, vibValue);
    }

    static void Viberation_VRTK(VRTK_ControllerReference controllerReference, AudioClip clip)
    {
        if (!IsViberationValid_VRTK(controllerReference))
            return;

        StopViberation_VRTK(controllerReference);
        VRTK_ControllerHaptics.TriggerHapticPulse(controllerReference, clip);
    }

    static void StopViberation_VRTK(VRTK_ControllerReference controllerReference)
    {
        if (!IsViberationValid_VRTK(controllerReference))
            return;

        VRTK_ControllerHaptics.CancelHapticPulse(controllerReference);
    }

    static bool IsViberationValid_VRTK(VRTK_ControllerReference controllerReference)
    {
        if (controllerReference.IsNull())
            return false;
        if (Application.isEditor && !Instance.isEditorVibe)//调试时避免震动
            return false;

        return true;
    }

    public static VRTK_ControllerReference GetControllerReference(Component controllerEvents)
    {
        if (!controllerEvents)
            return null;
        return VRTK_ControllerReference.GetControllerReference(controllerEvents.gameObject);
    }

    #endregion


    /// <summary>
    /// 切换手部的模型
    /// </summary>
    /// <param name="isUseCustom">true为使用自定义手模，false为使用默认</param>
    public void SwitchHandMode(bool isUseCustom)
    {
        VRHand.leftHand.SwitchHandMode(isUseCustom);
        VRHand.rightHand.SwitchHandMode(isUseCustom);
    }

    /// <summary>
    /// 切换Controller提示的显隐
    /// </summary>
    /// <param name="isShow"></param>
    public void ShowToolTips(bool isShow)
    {
        var leftToolTips = leftControllerEvent.transform.GetComponentInChildren<VRTK_ControllerTooltips>();
        var rightToolTips = rightControllerEvent.transform.GetComponentInChildren<VRTK_ControllerTooltips>();
        leftToolTips.enabled = isShow;
        rightToolTips.enabled = isShow;
    }

    #region Teleport

    /// <summary>
    /// 眨眼（编辑器调用）
    /// </summary>
    /// <param name="duration"></param>
    public void Blink(float duration)
    {
        foreach (VRTK_BasicTeleport baseTeleport in VRTK_ObjectCache.registeredTeleporters)
        {
            System.Reflection.MethodInfo loadingMethod = baseTeleport.GetType().GetMethod("Blink", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var arguments = new object[] { duration };
            loadingMethod.Invoke(baseTeleport, arguments);
        }
    }

    /// <summary>
    /// 变暗，常用于失败画面
    /// </summary>
    /// <param name="duration"></param>
    public void HeadsetFadeFunc(float duration)
    {
        Color color = Color.black;
        VRTK_SDK_Bridge.GetHeadsetSDK().HeadsetFade(color, duration, false);
    }

    /// <summary>
    /// 取消变暗
    /// </summary>
    /// <param name="duration"></param>
    public void HeadsetReleaseFadeFunc(float duration)
    {
        Color colorClear = Color.clear;//完全透明
        VRTK_SDK_Bridge.GetHeadsetSDK().HeadsetFade(colorClear, duration, false);
    }

    #endregion

    #region Judging

    private static bool IsVRPlayerHeadFunc(Collider other)
    {
        return other.name.Contains("HeadsetColliderContainer");
    }

    private static bool IsVRPlayerLowerBodyFunc(Collider other)
    {
        return other.name.Contains("BodyColliderContainer");//VRTK_PlayerObject中的下半身
    }
    private static bool IsVRControllerFunc(Collider other)
    {
        return other.transform.GetComponentInParent<VRTK_ControllerEvents>() != null;
    }

    public static bool IsVRRightControllerFunc(Collider other)
    {
        var ce = other.transform.GetComponentInParent<VRTK_ControllerEvents>();
        if (ce)
        {
            return ce.gameObject.name == "RightController";
        }
        return false;
    }
    private static bool IsVRLeftControllerFunc(Collider other)
    {
        var ce = other.transform.GetComponentInParent<VRTK_ControllerEvents>();
        if (ce)
        {
            return ce.gameObject.name == "LeftController";
        }
        return false;
    }

    #endregion

    #region Editor Setup

    /// <summary>
    /// 显隐 SDK切换UI
    /// </summary>
    void ToggleSDKSetupSwitcher()
    {
        if (goUISDKSetupSwitcher)
        {
            goUISDKSetupSwitcher.SetActive(!goUISDKSetupSwitcher.activeInHierarchy);
        }
    }
    void ShowSDKSetupSwitcher(bool isShow)
    {
        if (goUISDKSetupSwitcher)
        {
            goUISDKSetupSwitcher.SetActive(isShow);
        }
    }

    private void OnLoadedSetupChanged(VRTK_SDKManager sender, VRTK_SDKManager.LoadedSetupChangeEventArgs e)
    {
        if (!e.currentSetup && e.errorMessage.IsNullOrEmpty())//程序退出也会调用，此时不需要执行
            return;
        //DebugSetting
        if (Application.isEditor && isEditorRBezierPointer)
        {
            if (VRTK_SDKManager.instance.loadedSetup && VRTK_SDKManager.instance.loadedSetup.name.Contains("Simulator"))//模擬器
                SetBezierPointer(VRInterface.rightControllerEvent);//设置右手为曲线，方便模拟器调试
        }

        //将切换Manager默认为成功加载HMD
        Instance.actionOnHMDLoaded.Execute();
    }

    void SetBezierPointer(VRTK_ControllerEvents controllerEvents)
    {
        VRTK_BezierPointerRenderer renderer = controllerEvents.GetComponent<VRTK_BezierPointerRenderer>();
        if (renderer)
        {
            renderer.maximumLength = editorBezierPointerMaxLength; //缩短距离，方便模拟器调试
            controllerEvents.GetComponent<VRTK_Pointer>().pointerRenderer = renderer;
        }
    }

    #endregion
}
#endif
