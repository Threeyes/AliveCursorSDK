using UnityEngine;
#if USE_VRTK
using VRTK;
#elif USE_VIU
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.VRModuleManagement;
#endif

/// <summary>
///  在加载场景后，自动作为Camera的子物体
///  可用于物体附加、UI附加
/// 
/// </summary>
public class VRAttachToHeadOnLoad : MonoBehaviour
{
    public Vector3 distanceToCamera = new Vector3(0, 0, 0f);


    private void Awake()
    {
#if USE_VRTK
        VRTK_SDKManager.instance.LoadedSetupChanged += OnLoadedSetupChanged;//保持贴在当前的相机前
#elif USE_VIU
        VRModule.onNewPoses += OnLoadedSetupChanged;
#endif
    }

    private void OnLoadedSetupChanged
#if USE_VRTK
    (VRTK_SDKManager sender, VRTK_SDKManager.LoadedSetupChangeEventArgs e)
#else
()
#endif
    {

#if USE_VIU
        if (!VivePose.IsValidEx(DeviceRole.Hmd))
            return;

        VRModule.onNewPoses -= OnLoadedSetupChanged;
#endif

        if (VRInterface.vrCamera)
        {
            transform.SetParent(VRInterface.vrCamera.transform);
            transform.localPosition = distanceToCamera;
            transform.localRotation = default(Quaternion);
        }
    }

}
