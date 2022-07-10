using UnityEngine;
using UnityEngine.Events;

#if USE_VRTK
using VRTK;
#elif USE_VIU
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.VRModuleManagement;
#endif

/// <summary>
/// VR系统的常见事件
/// </summary>
public class VRSystemEvent : MonoBehaviour
{
    public UnityEvent onChangeModule;//初始化完毕，头盔的位置已确定，常用于后续DP传送

    private void Awake()
    {
#if USE_VRTK
                VRTK_SDKManager.instance.LoadedSetupChanged += OnLoadedSetupChanged;
#elif USE_VIU
        VRModule.onNewPoses += OnNewPoses;
#endif
    }

#if USE_VRTK
  private void OnLoadedSetupChanged(VRTK_SDKManager sender, VRTK_SDKManager.LoadedSetupChangeEventArgs e)
    {
        onChangeModule.Invoke();
        VRTK_SDKManager.instance.LoadedSetupChanged -= OnLoadedSetupChanged;//只执行一次
    }
#elif USE_VIU
    private void OnNewPoses()
    {
        if (VivePose.IsValidEx(DeviceRole.Hmd))
        {
            onChangeModule.Invoke();
            VRModule.onNewPoses -= OnNewPoses;
        }
    }
#endif


}
