using ControllerComponent = UnityEngine.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
#if VIU_WAVEVR
using HTC.UnityPlugin.VRModuleManagement;
#elif USE_RhinoX
using Ximmerse.RhinoX;
#endif

/// <summary>
/// 提供针对VR的各种接口,作为[VRTK_SDKManager]、[VRTK_Scripts]的父物体
/// 
/// Todo:增加一个统一通知设备更新的事件如VRTK_SDKManager.instance.AddBehaviourToToggleOnLoadedSetupChange
/// PS:该类不能使用工厂模式进行解耦，因为有些不通用的方法定义在这里（如VIU的IsHandRole）
/// </summary>
public partial class VRInterface : InstanceBase<VRInterface>
{

    #region Property

    //自定义的VR物体
    [Header("Custom")]
    public Transform tfCameraRigParentOverride;
    public Transform tfCameraRigOverride;
    public Transform tfCameraEyeOverride;
    public Camera vrCameraOverride;

    //供各插件缓存的引用
    protected Transform tfCameraRigParentCache;
    protected Transform tfCameraRigCache;
    protected Transform tfCameraCache;
    protected Camera cameraCache;
    protected Component leftControllerRefCache;
    protected Component rightControllerRefCache;

    #endregion

    #region  Partial Method

    //Rig的父类，如SteamVR中挂有VRTK_SDKSetup脚本。用于（更改位置后）回归时记录旧父对象
    public static Transform tfCameraRigParent { get { return GetTFCameraRigParentFunc(); } }

    //Camera的父类 [CameraRig]
    public static Transform tfCameraRig { get { return GetTFCameraRigFunc(); } }

    //Camera (eye)
    public static Transform tfCameraEye { get { return GetTFCameraEyeFunc(); } }

    public static Transform tfLeftController { get { var com = leftControllerRef; return com ? com.transform : null; } }
    public static Transform tfRightController { get { var com = rightControllerRef; return com ? com.transform : null; } }

    public static Camera vrCamera { get { return GetVRCameraFunc(); } }

    //Controller组件的引用
    public static Component defaultControllerRef { get { return rightControllerRef.NotNull() ? rightControllerRef : leftControllerRef; } }
    public static Component rightControllerRef
    {
        get
        {
#if USE_OpenXR|| USE_VRTK
            return GetRightControllerRefFunc();
#else
            return Instance.rightControllerRefCache;
#endif
        }
    }

    public static Component leftControllerRef
    {
        get
        {
#if USE_OpenXR|| USE_VRTK
            return GetLeftControllerRefFunc();
#else
            return Instance.leftControllerRefCache;
#endif
        }
    }
    public static List<Component> GetControllers(ControllerCheckType controllerCheckType)
    {
        List<Component> listControllerEvents = new List<Component>();
        switch (controllerCheckType)
        {
            case ControllerCheckType.Both:
                listControllerEvents.Add(leftControllerRef);
                listControllerEvents.Add(rightControllerRef);
                break;
            case ControllerCheckType.Left:
                listControllerEvents.Add(leftControllerRef); break;
            case ControllerCheckType.Right:
                listControllerEvents.Add(rightControllerRef); break;
            default:
                Debug.LogError("找不到对应的控制器！");
                return null;
        }
        return listControllerEvents;
    }

    #endregion

    #region Unity Func

    private void Awake()
    {
#if USE_OpenXR|| USE_VRTK||USE_VIU
        AwakeFunc();
#endif
    }

    void Update()
    {
#if USE_OpenXR|| USE_VRTK
        UpdateFunc();
#endif
    }

    void OnDestroy()
    {
#if  USE_VRTK||USE_VIU
        OnDestroyFunc();
#endif
    }
    #endregion

    #region Event

    UnityAction actionOnHMDLoaded;

    /// <summary>
    /// 监听VR相机连接成功后的状态(建议在Awake的时候调用）
    /// Warning：如果是静态委托，在OnDestroy前要取消监听，否则会报错
    /// </summary>
    /// <param name="action"></param>
    public static bool RegistOnHMDLoaded(UnityAction action)
    {
        //PS:有可能导入了VR插件但不使用VRInterface。因此要先检测是否为空
        if (Instance)
        {
            Instance.actionOnHMDLoaded += action;
            return true;
        }
        return false;
    }

    public static void UnRegistOnHMDUnLoaded(UnityAction action)
    {
        if (Instance)
            Instance.actionOnHMDLoaded -= action;
    }

    #endregion

    #region Camera

    /// <summary>
    /// 场景中可用的相机
    /// </summary>

    public static List<Camera> listAvaliableCam
    {
        get
        {
            List<Camera> listCam = new List<Camera>();

#if VIU_WAVEVR
            switch (VRModule.activeModule)
            {
                case VRModuleActiveEnum.Simulator:
                    listCam.Add(Camera.main);
                    break;

                case VRModuleActiveEnum.WaveVR:
                    if (WaveVR_Render.Instance)
                    {
                        listCam.Add(WaveVR_Render.Instance.lefteye.GetComponent<Camera>());
                        listCam.Add(WaveVR_Render.Instance.righteye.GetComponent<Camera>());
                    }
                    break;
            }
            //WaveVR 只有左右眼有效，上级摄像机不起作用
#elif USE_RhinoX
            if (ARCamera.Instance)
                listCam.Add(ARCamera.Instance.GetComponent<Camera>());
#elif USE_VRTK
            if (VRInterface.tfCameraEye)
            {
                listCam.Add(VRInterface.vrCamera);
            }
#else
            listCam.AddRange(Camera.allCameras.ToList());
#endif
            return listCam;
        }
    }
    public void SetFOV(float fov)
    {
#if VIU_WAVEVR
        foreach (Camera cam in listAvaliableCam)
        {
            cam.orthographic = true;//通知Camera更新
            cam.fieldOfView = fov;
            cam.orthographic = false;
        }
#endif
    }

    #endregion

    #region Parenting

    public UnityAction<Transform, bool> actOnSetCameraRigParent;//设置相机的父物体,bool代表target是否为有效（非VR根物体）对象

    public void ResetCameraRigParentAndPos()
    {
        ResetCameraRigParent();
        tfCameraRig.localEulerAngles = Vector3.zero;
        tfCameraRig.localPosition = Vector3.zero;
    }
    public void ResetCameraRigParent()
    {
        SetCameraRigParent(null);
    }

    /// <summary>
    /// 适用于头盔绑定在人头节点的情况
    /// </summary>
    /// <param name="target"></param>
    public void SetCameraRigParentAndResetPos(Transform target)
    {
        SetCameraRigParent(target);


        if (tfCameraRig && tfCameraEye)
        {
            //Align
            tfCameraRig.eulerAngles = target.eulerAngles;
            float camEyeYRot = tfCameraEye.localEulerAngles.y;
            tfCameraRig.Rotate(0, -camEyeYRot, 0, Space.Self);

            tfCameraRig.localPosition = default(Vector3);
            Vector3 eyeOffset = tfCameraRig.position - tfCameraEye.position;
            tfCameraRig.position += eyeOffset;//计算位移值
        }
    }
    /// <summary>
    /// 设置CameraRig的父对象，常用于驾驶时跟随车辆移动
    /// </summary>
    /// <param name="target">传入null时，回到原来的VR根物体</param>
    public void SetCameraRigParent(Transform target)
    {
        SetCameraRigParent(target, false);
    }
    public void SetCameraRigParent(Transform target, bool resetPosAndRot)
    {
        Transform finalTarget = null;
        bool isOtherTarget = false;
        if (target)
        {
            finalTarget = target;
            isOtherTarget = true;
        }
        else
        {
            finalTarget = tfCameraRigParent;
        }
        tfCameraRig.SetParent(finalTarget);
        if (resetPosAndRot)//重置为基于父物体的默认位置
        {
            tfCameraRig.localPosition = Vector3.zero;//重置父物体位置
            tfCameraRig.localEulerAngles = Vector3.zero;
            tfCameraEye.parent.localEulerAngles = Vector3.zero;//重置旋转
        }
        actOnSetCameraRigParent.Execute(finalTarget, isOtherTarget);
    }
    /// ToDelete:用ResetCameraRigParent代替
    public void ResetCameraHeadParent()
    {
        SetCameraHeadParent(null);
    }

    /// <summary>
    /// 设置头部的父物体，常用于第一人称视角  。
    /// ToDelete:用SetCameraRigParent代替
    /// </summary>
    /// <param name="target"></param>
    public void SetCameraHeadParent(Transform target)
    {
        //if (target)
        //{
        //    //Todo:停用CameraEye的position追踪
        //    tfCameraEye.SetParent(target, true);
        //}
        //else
        //{
        //    tfCameraEye.SetParent(tfCameraRig);
        //}
    }

    #endregion

    #region  Judging

    public static bool IsVRPlayerWholeBody(Collider other)
    {
        return IsVRPlayerHead(other) | IsVRPlayerLowerBody(other);
    }

    public static bool IsVRPlayerHead(Collider other)
    {
#if USE_VRTK||USE_VIU
        return IsVRPlayerHeadFunc(other);
#else
        Debug.LogError("未实现！");
        return false;
#endif
    }

    public static bool IsVRPlayerLowerBody(Collider other)
    {
#if USE_VRTK||USE_VIU
        return IsVRPlayerLowerBodyFunc(other);
#else
        Debug.LogError("未实现！");
        return false;
#endif
    }

    public static bool IsVRController(Collider other)
    {
#if USE_VRTK||USE_VIU
        return IsVRControllerFunc(other);
#else
        Debug.LogError("未实现！");
        return false;
#endif
    }

    public static bool IsVRRightController(Collider other)
    {
#if USE_VRTK
        return IsVRRightControllerFunc(other);
#else
        return false;
#endif
    }

    public static bool IsVRLeftController(Collider other)
    {
#if USE_VRTK
        return IsVRLeftControllerFunc(other);
#else
        return false;
#endif
    }

    #endregion

    #region Viberation

    /// <summary>
    ///  Todo:效果不好，改为自己手动监听AudioSource（可以是一个事件回调）
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="clip"></param>
    public static void Viberation(Component obj, AudioClip clip)
    {
#if USE_OpenXR|| USE_VRTK
        ViberationFunc(obj, clip);
#endif
    }

    /// <summary>
    /// 震动手柄(Wrong Spelling)
    /// </summary>
    /// <param name="controllerEvents"></param>
    /// <param name="vibValue"></param>
    public static void Viberation(Component obj, float vibValue, float duration = 0, float pulseInterval = 0.01f)
    {
#if USE_OpenXR|| USE_VRTK
        ViberationFunc(obj, vibValue, duration, pulseInterval);
#endif
    }

    public static void StopViberation(Component obj)
    {
#if USE_OpenXR|| USE_VRTK
        StopViberationFunc(obj);
#endif
    }

    #endregion

    #region Teleport

    /// <summary>
    /// 变暗，常用于失败画面
    /// </summary>
    /// <param name="duration"></param>
    public void HeadsetFade(float duration)
    {
#if USE_VRTK||USE_VIU||USE_PicoMobileSDK
        HeadsetFadeFunc(duration);
#endif
    }

    /// <summary>
    /// 取消变暗
    /// </summary>
    /// <param name="duration"></param>
    public void HeadsetReleaseFade(float duration)
    {
#if USE_VRTK||USE_VIU||USE_PicoMobileSDK
        HeadsetReleaseFadeFunc(duration);
#endif
    }


    #endregion


    #region Utility Func

    /// <summary>
    /// 获取对应的引用
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="funcList"></param>
    /// <param name="realValue"></param>
    /// <returns></returns>
    public static T GetValiableRef<T>(CustomFunc<T> funcList, ref T realValue) where T : class
    {
        if (realValue.NotNull())
            return realValue;

        //遍历委托列表的返回值，保存第一个非空的方法（正常情况下，只有一个有效监听方法）
        foreach (CustomFunc<T> cf in funcList.GetInvocationList())
        {
            T tempVal = cf();
            if (tempVal.NotNull())
            {
                realValue = tempVal;
                return realValue;
            }
        }
        Debug.LogError("找不到对应组件！");
        return null;
    }

    #endregion
}
