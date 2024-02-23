using Newtonsoft.Json;
using Threeyes.Config;
using Threeyes.Core;
using UnityEngine;

namespace Threeyes.Steamworks
{

    /// <summary>
    /// Tilt rotate around the axis
    ///
    /// </summary>
    public class TiltByTargetMovement : ConfigurableUpdateComponentBase<Transform, SOTiltByTargetMovementConfig, TiltByTargetMovement.ConfigInfo>
    {
        public Transform target;//Movement target

        [Header("Run Time")]
        public Vector3 curTargetVelocity;
        public Vector3 velocityOnMovementAxis;
        Vector3 lastTargetPosition;
        public int sign;
        public float destAngle = 0;//对应轴向的角度

        void Start()
        {
            lastTargetPosition = target.position;
        }

        ///ToUpdate:Config中的Axis应该是代表移动时的Up轴方向
        protected override void UpdateFunc()
        {
            base.UpdateFunc();

            curTargetVelocity = (target.position - lastTargetPosition) / DeltaTime;
            if (curTargetVelocity.sqrMagnitude > 0.01f)//移动中
            {
                velocityOnMovementAxis = Vector3.Project(curTargetVelocity, target.TransformDirection(Config.localDetectMovementAxis)); //获取velocity在物体局部移动轴上的分力矢量（因为光标可以任意旋转，所以使用局部坐标）

                //sign = Vector3.Dot(target.up, target.TransformDirection(Config.targetUpAxis)) > 0 ? -1 : 1;//检查目标物体朝向，确认sign(朝上为-1，朝下为1)(ToUpdate：应该根据localDetectMovementAxis决定sign)
                sign = Vector3.Dot(target.TransformDirection(Config.localDetectMovementAxis), velocityOnMovementAxis) > 0 ? 1 : -1;

                destAngle = Mathf.Clamp(destAngle + sign * velocityOnMovementAxis.magnitude * Config.increaseSpeed, -Config.maxAngle, Config.maxAngle);//计算要增加的角度
            }
            else//暂停移动
            {
                destAngle = Mathf.Lerp(destAngle, 0, Config.decreaseSpeed);//恢复原状
            }
            Comp.localRotation = Quaternion.Euler(Vector3.one.Multi(Config.localTiltAxis) * destAngle);//旋转到指定角度

            lastTargetPosition = target.position;
        }

        #region Define

        [System.Serializable]
        public class ConfigInfo : SerializableDataBase
        {
            public float increaseSpeed = 0.3f;
            public float decreaseSpeed = 0.1f;
            public float maxAngle = 20;

            public Vector3 localDetectMovementAxis = new Vector3(1, 0, 0);//which positive local axis on target gameobject to detect movemnt
            public Vector3 localTiltAxis = new Vector3(0, 1, 0);//which axis on self gameobject to tilt
        }

        #endregion
    }
}