using UnityEngine;
namespace Threeyes.Steamworks
{
    /// <summary>
    /// Transform using percent
    /// 
    /// 用途：
    /// -时钟：按传入的百分比旋转时针
    /// </summary>
    public class TransformByProgress : MonoBehaviour
    {
        #region Property & Field
        public Transform Target { get { return target ? target : transform; } set { target = value; } }
        public Transform target;//需要执行变换操作的物体

        [SerializeField] bool isLocalSpace = true;
        [SerializeField] Vector3 startRotation;
        [SerializeField] Vector3 targetRotation;//要旋转的指定角度
        #endregion

        #region Rotation
        public void SetRotationPercent(float percent)
        {
            if (isLocalSpace)
                Target.localEulerAngles = Vector3.Lerp(startRotation, targetRotation, percent);
            else
                Target.eulerAngles = Vector3.Lerp(startRotation, targetRotation, percent);
        }
        #endregion
    }
}