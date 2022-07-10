using UnityEngine;

#if USE_PostProcessing
using UnityEngine.PostProcessing;
#elif USE_PostProcessingV2
using UnityEngine.Rendering.PostProcessing;
#endif

#if USE_VIU
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.VRModuleManagement;
#endif

/// <summary>
/// 管理全局PostProcessingProfile文件，常用于：
/// 一体机初始化相机中的PostProcessing相关组件
/// 
/// 
/// PS(PostProcessingV2):
/// --将Layer和Volume挂在PostProcessingProfileManager单例上
/// </summary>
public class PostProcessingProfileManager : InstanceBase<PostProcessingProfileManager>
{
    public LayerMask volumeLayer;

#if USE_PostProcessing
    public PostProcessingProfile profile;//当前引用的Profile
#elif USE_PostProcessingV2

#endif

#if USE_VIU
    protected virtual void Awake()
    {
        VRModule.onNewPoses += OnNewPoses;
    }

    private void OnDestroy()
    {
        VRModule.onNewPoses -= OnNewPoses;
    }

    private void OnNewPoses()
    {
        if (VivePose.IsValidEx(DeviceRole.Hmd))
        {
            InitComRef();

            VRModule.onNewPoses -= OnNewPoses;
        }
    }

    private void InitComRef()
    {

        //为所有相机挂上必要的组件
        foreach (Camera cam in CameraFXHelperBase.listAvaliableCam)
        {
#if USE_PostProcessing
            var comp = cam.gameObject.AddComponentOnce<PostProcessingBehaviour>();
            if (profile)
                comp.profile = profile;
#elif USE_PostProcessingV2
            var ppl = cam.AddComponentOnce<PostProcessLayer>();
            ppl.volumeLayer = volumeLayer;
            ppl.volumeTrigger = cam.transform;
#endif
        }
    }
#endif

}

