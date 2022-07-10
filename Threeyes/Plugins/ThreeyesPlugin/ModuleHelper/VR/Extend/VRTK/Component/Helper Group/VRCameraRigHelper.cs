using UnityEngine;
#if USE_DOTween
using DG.Tweening;
#endif

/// <summary>
/// 设置VRCameraRig的位置及父物体，常用于搭乘或旋转玩家对象
/// </summary>
public class VRCameraRigHelper : MonoBehaviour
{
    public Transform tfRotateTo;
#if USE_DOTween
    public Ease ease = Ease.Linear;
#endif

    /// <summary>
    /// 作为VR相机的子物体
    /// </summary>
    /// <param name="target"></param>
    public void FollowVRCamera(Transform target)
    {
        if (!VRInterface.vrCamera)
            return;

        target.SetParent(VRInterface.vrCamera.transform);
        target.localPosition = default(Vector3);
        target.localEulerAngles = default(Vector3);
    }

    public void ChangeRotation(float tweenDuration)
    {
        if (tfRotateTo)
        {
#if USE_DOTween
         VRInterface.tfCameraRig.DORotate(tfRotateTo.eulerAngles, tweenDuration, RotateMode.Fast).SetEase(ease);
#endif
        }
    }
    public void ResetRotation()
    {
        VRInterface.tfCameraRig.localEulerAngles = default(Vector3);
    }
}
