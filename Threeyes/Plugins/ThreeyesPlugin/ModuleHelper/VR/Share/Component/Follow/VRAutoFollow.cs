using UnityEngine;
using System;
#if USE_VRTK
using VRTK;
#elif USE_VIU
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.VRModuleManagement;
#endif


/// <summary>
/// 自动跟随头盔的位置
/// </summary>
public class VRAutoFollow : MonoBehaviour
{
    public bool IsActive { get { return isActive; } set { isActive = value; } }
    public bool isActive = true;
    public Vector3 offset = new Vector3(0.35f, 0.06f, 0.68f);
    public Vector3 lookAtAllowAxis = new Vector3(1, 0, 1);//朝向时，忽略的轴向,1代表忽略（如忽略Y轴，可以写（0,1,0））
    public bool isLookAt = false;
    public float moveSpeed = 0.02f;

    public Transform tfVRCam;

    public Transform TfVRCam
    {
        get
        {
            return tfVRCam;
        }
    }

    private void Awake()
    {
        //Todo: 增加针对DontDestroyOnLoad监听更换场景事件重新监听
#if USE_VRTK
        VRTK_SDKManager.instance.LoadedSetupChanged += OnLoadedSetupChanged;//保持贴在当前的相机前
#elif USE_VIU
        VRModule.onNewPoses += OnLoadedSetupChanged;//PS:这个方法会持续追踪
#endif
    }

    private void OnDestroy()
    {
#if USE_VRTK
        VRTK_SDKManager.instance.LoadedSetupChanged -= OnLoadedSetupChanged;//保持贴在当前的相机前
#elif USE_VIU
        VRModule.onNewPoses -= OnLoadedSetupChanged;
#endif
    }

    private void OnLoadedSetupChanged
#if USE_VRTK
    (VRTK_SDKManager sender, VRTK_SDKManager.LoadedSetupChangeEventArgs e)
#else
()
#endif
    {
        tfVRCam = VRInterface.tfCameraEye;
    }

    void LateUpdate()
    {
        if (!IsActive)
            return;
        FollowFunc(moveSpeed);
    }

    [ContextMenu("MoveToDestinationAtOnce")]
    public void MoveToDestinationAtOnce()
    {
        FollowFunc(1);
    }

    private void FollowFunc(float movePercent)
    {
        if (TfVRCam)
        {
            Vector3 targetPos = TfVRCam.TransformPoint(offset);
            transform.position = Vector3.Lerp(transform.position, targetPos, movePercent);
            if (isLookAt)
            {
                Vector3 targetPoint = new Vector3(
                    lookAtAllowAxis.x > 0 ? TfVRCam.position.x : transform.position.x,
                    lookAtAllowAxis.y > 0 ? TfVRCam.position.y : transform.position.y,
                    lookAtAllowAxis.z > 0 ? TfVRCam.position.z : transform.position.z
                    );
                transform.LookAt(targetPoint);
            }
        }
    }
}