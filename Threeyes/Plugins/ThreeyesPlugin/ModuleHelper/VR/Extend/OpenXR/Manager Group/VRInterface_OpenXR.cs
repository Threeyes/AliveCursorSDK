#if USE_OpenXR
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
#if UNITY_EDITOR
using Threeyes.Editor;
#endif
using InputActionAsset = UnityEngine.InputSystem.InputActionAsset;
using System.Linq;
using System.IO;
using System;
using UnityEngine.XR.Interaction.Toolkit;

public partial class VRInterface : InstanceBase<VRInterface>
{
#region Property & Field
    public InputActionAsset InputActionAsset
    {
        get
        {
            return inputActionAsset;
        }
        set
        {
            inputActionAsset = value;
        }
    }

    //https://docs.unity3d.com/Manual/xr_input.html
    public InputDevice inputDevice_HMD;
    public InputDevice inputDevice_LeftController;
    public InputDevice inputDevice_RightController;


    [Header("OpenXR")]
    [SerializeField] private InputActionAsset inputActionAsset;//PS:在运行前会自动搜索，因为文件在Sample里，而且每次更新版本都会产生新的文件，所以让他自动获取


#endregion

#region  partial Method

    bool IsHMDTracking()
    {
        //检测头盔是否正在被追踪
        bool isTracked = false;
        bool hasData = false;
        if (inputDevice_HMD != null)
        {
            hasData = inputDevice_HMD.TryGetFeatureValue(CommonUsages.isTracked, out isTracked);
        }
        return isTracked;
    }
    static Transform GetTFCameraRigParentFunc()
    {
        if (!Instance.tfCameraRigParentCache)
            Instance.tfCameraRigParentCache = tfCameraRig.parent;
        return Instance.tfCameraRigParentCache;
    }
    static Transform GetTFCameraRigFunc()
    {
        if (!Instance.tfCameraRigCache)
            Instance.tfCameraRigCache = tfCameraEye.parent.parent;//PS:tfCameraEye 的 parent 只是Camera Offset
        return Instance.tfCameraRigCache;
    }

    static Transform GetTFCameraEyeFunc()
    {
        if (!Instance.tfCameraCache && GetVRCameraFunc())//PS:要强制调用Get Camera相关方法
            Instance.tfCameraCache = Instance.cameraCache.transform;
        return Instance.tfCameraCache;
    }

    static Camera GetVRCameraFunc()
    {
        Instance.cameraCache = Instance.tfOpenXRRoot.FindFirstComponentInChild<Camera>(false, true);
        return Instance.cameraCache;
    }
    static Component GetRightControllerRefFunc()
    {
        //PS:同时要检测手柄是否可用
        if (Instance.rightControllerRefCache.IsNull())
        {
            Instance.rightControllerRefCache = Instance.tfOpenXRRoot.FindFirstComponentInChild<XRController>(false, true, (c) => c.controllerNode == XRNode.RightHand);
        }
        return Instance.rightControllerRefCache;
    }
    static Component GetLeftControllerRefFunc()
    {
        if (Instance.leftControllerRefCache.IsNull())
        {
            Instance.leftControllerRefCache = Instance.tfOpenXRRoot.FindFirstComponentInChild<XRController>(false, true, (c) => c.controllerNode == XRNode.LeftHand);
        }
        return Instance.leftControllerRefCache;
    }

    Transform tfOpenXRRoot
    {
        get
        {
            return transform.Find("OpenXR Group");
        }
    }

#endregion

#region Unity Func

    public override void SetInstance()
    {
        base.SetInstance();

        //ForceRuntime();
    }
    [ContextMenu("OnValidate")]
    public void OnValidate()
    {
#if UNITY_EDITOR


        //自动查找并获取对应的InputActionAsset
        if (!InputActionAsset)
        {
            string assetName = "InputActions";
            var listAssets = AssetMenuEditor_AssetPath.FindAsset<InputActionAsset>(assetName);//可能有多个含该字符串的文件，需要进步筛选
            var result = listAssets.Find((asset) => asset.name == assetName);
            if (result != null)
            {
                inputActionAsset = result;
                UnityEditor.EditorUtility.SetDirty(this);// mark as dirty, so the change will be save into scene file
                Debug.Log("[自动]查找OpenXR对应的InputActionAsset文件");
            }
        }
#endif
    }

    private static void ForceRuntime()
    {
#if UNITY_STANDALONE
        //(Ref: package-OpenXRRuntimeSelector.cs)
        //PS:每次运行项目都会调用一次（https://forum.unity.com/threads/openxr-oculus-quest-2-in-editor-play-mode-only-works-with-direct3d11-api-vulkan-crashes.1143632/#post-7342439）
        //自动将Project Settings-OpenXR-Play Mode OpenXR Runtime的值设置为对应运行时（PS：临时方法， 等官方正式提供其他选项）
        //Bug:要打开该页面才会自动刷新（调用EnterPlayMode）
        //PC模式下自动激活SteamVR
        string envValue = Environment.GetEnvironmentVariable(k_SelectedRuntimeEnvKey);
        if (envValue != steamVRDetectorInst.jsonPath)
        {
            steamVRDetectorInst.PrepareRuntime();
            Environment.SetEnvironmentVariable(k_SelectedRuntimeEnvKey, steamVRDetectorInst.jsonPath);

#if UNITY_EDITOR
            EditorTool.RepaintAllViews();
#endif
            Debug.LogWarning("[自动]将Project Settings-OpenXR-Play Mode OpenXR Runtime的值设置为SteamVR");//因为每次都会打印，放在Warning不影响正常开发
        }

        steamVRDetectorInst.Activate();//强制激活(原代码是在playModeStateChanged的时候更换的)
#endif
    }

    void AwakeFunc()
    {
        //ForceRuntime();

        //监听按键的回调
        if (InputActionAsset)
            InputActionAsset.Enable();

        GetLeftControllerRefFunc();
        GetRightControllerRefFunc();

        //Debug.LogError("SelectedRuntime: " + Environment.GetEnvironmentVariable(k_SelectedRuntimeEnvKey));
        //Debug.LogError("Runtime: " + Environment.GetEnvironmentVariable(RuntimeDetector.k_RuntimeEnvKey));
    }

    //private void OnApplicationQuit()
    //{
    //    //Test
    //    UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager.StopSubsystems();
    //    UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager.DeinitializeLoader();
    //}

    void OnEnable()
    {
        //监听设备的连接事件
        InputDevices.deviceConnected += DeviceConnected;
        InputDevices.deviceDisconnected += DeviceDisConnected;

        //针对已有的设备，进行手动调用方法
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevices(devices);
        foreach (var device in devices)
            DeviceConnected(device);
    }

    void OnDisable()
    {
        InputDevices.deviceConnected -= DeviceConnected;
        InputDevices.deviceDisconnected -= DeviceDisConnected;
    }


    protected void UpdateFunc()
    {

    }

    void DeviceConnected(InputDevice device)
    {
        //#Result for HMD:
        //name：Head Tracking - OpenXR
        //characteristics：HeadMounted, TrackedDevice

        //#Result for rightHand:
        //name：HTC Vive Controller OpenXR
        //characteristics：HeldInHand, TrackedDevice, Controller, Right
        //Debug.Log("DeviceConnected + " + device.name + " + " + device.characteristics);

        if ((device.characteristics & InputDeviceCharacteristics.HeadMounted) != 0)
        {
            Instance.inputDevice_HMD = device;
            actionOnHMDLoaded.Execute();
        }
        // The Left Hand
        if ((device.characteristics & InputDeviceCharacteristics.Left) != 0)
        {
            Instance.inputDevice_LeftController = device;
            //Use device.name here to identify the current Left Handed Device
        }
        // The Right hand
        else if ((device.characteristics & InputDeviceCharacteristics.Right) != 0)
        {
            Instance.inputDevice_RightController = device;
            //Use device.Name here to identify the current Right Handed Device
        }
    }
    void DeviceDisConnected(InputDevice device)
    {

    }

#endregion

#region Viberation

    public static void ViberationFunc(Component controller, AudioClip clip)
    {
        //To Impl
    }

    /// <summary>
    /// 震动手柄(Wrong Spelling)
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="vibValue"></param>
    public static void ViberationFunc(Component controller, float vibValue, float duration = 0, float pulseInterval = 0.01f)
    {
        XRController xrController = controller as XRController;
        if (xrController)
        {
            duration = Mathf.Clamp(duration, 0, 100);//duration不能小于0

            ////Bug:该实现无效
            //xrController.SendHapticImpulse(vibValue, duration);

            //Ref from: https://gist.github.com/corycorvus/2b0788719f06fc162a8d5466ba58ac4d
            InputDevice device = xrController.inputDevice;
            HapticCapabilities capabilities;
            if (device.TryGetHapticCapabilities(out capabilities))
                if (capabilities.supportsImpulse)
                    device.SendHapticImpulse(0, vibValue, duration);
        }
    }

    public static void StopViberationFunc(Component controller)
    {
        XRController xrController = controller as XRController;
        if (xrController)
        {
            //Bug：无效
            InputDevice device = xrController.inputDevice;
            HapticCapabilities capabilities;
            if (device.TryGetHapticCapabilities(out capabilities))
                if (capabilities.supportsImpulse)
                    device.StopHaptics();
        }
    }

#endregion


#region Project Settings Defines

    const string k_SelectedRuntimeEnvKey = "XR_SELECTED_RUNTIME_JSON";
    class RuntimeDetector
    {
        public const string k_RuntimeEnvKey = "XR_RUNTIME_JSON";
        public virtual string name { get; }
        public virtual string jsonPath { get; }
        public virtual string tooltip => jsonPath;

        public virtual bool detected => File.Exists(jsonPath);

        public virtual void PrepareRuntime()
        {

        }

        public virtual void Activate()
        {
            if (detected)
            {
                Environment.SetEnvironmentVariable(k_RuntimeEnvKey, jsonPath);
            }
        }

        public virtual void Deactivate()
        {
            Environment.SetEnvironmentVariable(k_RuntimeEnvKey, "");
        }
    };
    class SteamVRDetector : RuntimeDetector
    {
        public override string name => "SteamVR";

        public override string jsonPath => @"C:\Program Files (x86)\Steam\steamapps\common\SteamVR\steamxr_win64.json";
    }

    private static SteamVRDetector steamVRDetectorInst = new SteamVRDetector();

#endregion

}
#endif
