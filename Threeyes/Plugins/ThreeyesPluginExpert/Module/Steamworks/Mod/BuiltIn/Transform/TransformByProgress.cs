using UnityEngine;
namespace Threeyes.Steamworks
{
    /// <summary>
    /// Transform using percent
    /// 
    /// 用途：
    /// -音箱：按传入百分比位移
    /// -时钟：按传入百分比旋转时针
    /// 
    /// ToUpdate：
    /// -设置Leap的类型：
    ///     -Lerp
    ///     -SLerp
    ///     -自定义的曲线
    /// </summary>
    public class TransformByProgress : MonoBehaviour
    {
        #region Property & Field
        public Transform Target { get { return target ? target : transform; } set { target = value; } }
        public Transform target;//需要执行变换操作的物体

        [SerializeField] bool isLocalSpace = true;

        [Header("Position")]
        [SerializeField] Vector3 startPosition;
        [SerializeField] Vector3 targetPosition;

        [Header("Rotation")]
        [SerializeField] Vector3 startRotation;
        [SerializeField] Vector3 targetRotation;//要旋转的指定角度

        [Header("Scale")]
        [SerializeField] Vector3 startScale = Vector3.one;
        [SerializeField] Vector3 targetScale = Vector3.one;

        #endregion

        #region Rotation
        public void SetPositionPercent(float percent)
        {
            if (isLocalSpace)
                Target.localPosition = Vector3.Lerp(startPosition, targetPosition, percent);
            else
                Target.position = Vector3.Lerp(startPosition, targetPosition, percent);
        }
        public void SetRotationPercent(float percent)
        {
            if (isLocalSpace)
                Target.localEulerAngles = Vector3.Lerp(startRotation, targetRotation, percent);
            else
                Target.eulerAngles = Vector3.Lerp(startRotation, targetRotation, percent);
        }
        public void SetScalePercent(float percent)
        {
            if (isLocalSpace)
                Target.localScale = Vector3.Lerp(startScale, targetScale, percent);
            //暂无法设置全局缩放
        }
        #endregion
    }
}