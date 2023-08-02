using Threeyes.Config;
using UnityEngine;

namespace Threeyes.Steamworks
{
    /// <summary>
    /// Rotate base on target's Movement
    /// 
    /// </summary>
    public class RotateByTargetMovement : ConfigurableUpdateComponentBase<Transform, SORotateByTargetMovementConfig, RotateByTargetMovement.ConfigInfo>
    {
        public Transform target;

        //Runtime
        protected Vector3 lastTargetPos;
        protected Vector3 movedVectorLastFrame;

        protected override void UpdateFunc()
        {
            if (!target)
                return;

            movedVectorLastFrame = target.position - lastTargetPos;
            RotateThis();
            lastTargetPos = target.position;
        }

        protected virtual float RuntimeRotateSpeed { get { return Config.rotateSpeed; } }
        protected virtual void RotateThis()
        {
            Vector3 VectorRightAxis = Vector3.Cross(-target.forward, movedVectorLastFrame).normalized;//Get the right Axis base on current movement vector
            Comp.Rotate(VectorRightAxis, movedVectorLastFrame.magnitude * RuntimeRotateSpeed, Space.World);//绕移动方向的对应轴旋转（移动单位需要乘以缩放的倍数）
        }

        #region Define

        [System.Serializable]
        public class ConfigInfo : SerializableDataBase
        {
            public float rotateSpeed = 360f;//rotate speed when the cursor size is 1
        }

        #endregion
    }
}