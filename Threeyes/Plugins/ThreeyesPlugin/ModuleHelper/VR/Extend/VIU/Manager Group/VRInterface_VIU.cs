#if USE_VIU
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.VRModuleManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if USE_DOTween
using DG.Tweening;
#endif

public partial class VRInterface : InstanceBase<VRInterface>
{
    #region  Partial Method

    static Transform GetTFCameraRigParentFunc()
    {
        if (!Instance.tfCameraRigParentCache)
            Instance.tfCameraRigParentCache = tfCameraRig.parent;
        return Instance.tfCameraRigParentCache;
    }
    static Transform GetTFCameraRigFunc()
    {
        if (!Instance.tfCameraRigCache)
            Instance.tfCameraRigCache = tfCameraEye.parent;
        return Instance.tfCameraRigCache;
    }

    static Transform GetTFCameraEyeFunc()
    {
        if (!Instance.cameraCache)
            Instance.cameraCache = GetVRCameraFunc();
        return Instance.cameraCache ? Instance.cameraCache.transform : null;
    }
    static Camera GetVRCameraFunc()
    {
        if (!Instance.cameraCache)
            Instance.cameraCache = FindVRCamera();
        return Instance.cameraCache;
    }

    #endregion

    static Camera FindVRCamera()
    {
        foreach (var cam in Camera.allCameras)
        {
            if (!cam.enabled) { continue; }
#if UNITY_5_4_OR_NEWER
            // try find vr camera eye
            if (cam.stereoTargetEye != StereoTargetEyeMask.Both) { continue; }
#endif
            return cam;
        }
        return null;
    }

    //暂时判断
    private static bool IsVRPlayerHeadFunc(Collider other)
    {
        return other.GetComponent<VRCameraHook>();
    }
    private static bool IsVRPlayerLowerBodyFunc(Collider other)
    {
        return false;
    }

    private static bool IsVRControllerFunc(Collider other)
    {
        Rigidbody rig = other.attachedRigidbody;
        if (rig)
        {
            return rig.GetComponent<ViveColliderEventCaster>();//Controller所用的组件（在Grabber-DeviceTracker-Caster下）
        }
        return false;
    }

    #region Input

    /// <summary>
    /// 头盔按键
    /// Todo:增加各个按键的判断
    /// </summary>
    /// <returns></returns>
    public static bool GetHMDKeyEnter(ButtonEventType buttonEventType)
    {
        KeyCode keyCode = KeyCode.None;

        //Input.GetKeyDown(KeyCode.Escape) 按下头盔返回键
        //Input.GetKeyDown(KeyCode.Menu) 按下头盔菜单键
        //KeyCode.JoystickButton0 按下头盔确认键

        keyCode = KeyCode.Escape;
        switch (buttonEventType)
        {
            case ButtonEventType.Down:
                return Input.GetKeyDown(keyCode);
            case ButtonEventType.Up:
                return Input.GetKeyUp(keyCode);
        }

        return false;
    }

    #endregion

    public static bool IsHandRole(System.Type roleType)
    {
        return roleType == typeof(HandRole);
    }
    public static bool IsHandRole_Left(int roleValue)
    {
        return roleValue == (int)HandRole.LeftHand;
    }
    public static bool IsHandRole_Right(int roleValue)
    {
        return roleValue == (int)HandRole.RightHand;
    }

    class CameraInfo
    {
        public Color backgroundColor = new Color(0.1921569f, 0.3019608f, 0.4745098f, 0.01960784f);
        public int cullingMask;
    }

    CameraInfo cacheVRCamerInfo = new CameraInfo();

    static Color defaultCamerabackgroundColor = new Color(0.1921569f, 0.3019608f, 0.4745098f, 0.01960784f);


    public void HeadsetFadeFunc(float duration)
    {
        HeadsetFadeFunc(true, duration: duration);
    }
    public void HeadsetReleaseFadeFunc(float duration)
    {
        HeadsetFadeFunc(false, duration: duration);
    }
    /// <summary>
    /// 通用的变暗功能，常用于失败画面
    /// </summary>
    /// <param name="duration"></param>
    public void HeadsetFadeFunc(bool isFade, Color fadeColor = default(Color), float duration = 0)
    {
#if USE_CameraPlay
        //Bug:多次调用可能导致黑屏
        CameraPlayHelper cameraPlayHelper = this.AddComponentOnce<CameraPlayHelper>();

        if (duration > 0)
        {
            cameraPlayHelper.Fade_OnOff(isFade, duration);
            return;
        }
#endif

        //Todo:在游戏开始时，记录每个相机的cullingMask
        foreach (Camera cam in listAvaliableCam)
        {
            //加上DoTween
            cam.clearFlags = isFade ? CameraClearFlags.SolidColor : CameraClearFlags.Skybox;
            Color targetColor = isFade ? fadeColor : defaultCamerabackgroundColor;

            //if(duration<=0)
            {
                cam.backgroundColor = targetColor;
            }
            //Fade有Bug，因为相机会看到很多物体，光改背景色无效
            //else
            //{
            //    cam.DOColor(targetColor, duration);
            //}
            cam.cullingMask = isFade ? 0 : -1;//0=Nothing,-1=Everything
        }
    }


    #region Unity Func

    private void AwakeFunc()
    {
        VRModule.onNewPoses += OnNewPoses;
    }
    void OnDestroyFunc()
    {
        VRModule.onNewPoses -= OnNewPoses;
    }
    private void OnNewPoses()
    {
        if (VivePose.IsValidEx(DeviceRole.Hmd))
        {
            Instance.actionOnHMDLoaded.Execute();
        }
    }

    private void Start()
    {
        Debug.LogWarning("VIU Initing……");
        //调用其初始化代码
        Debug.LogWarning(tfCameraRigParent);
        Debug.LogWarning(tfCameraRig);
        Debug.LogWarning(tfCameraEye);
        Debug.LogWarning(vrCamera);
    }

    #endregion
}
#endif