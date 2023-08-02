using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using Threeyes.Config;
using UnityEngine;

namespace Threeyes.Steamworks
{
    public class ObjectMovement_FollowTarget : ConfigurableComponentBase<SOObjectMovement_FollowTargetConfig, ObjectMovement_FollowTarget.ConfigInfo>,
        IObjectMovement
    {
        #region Interface
        public bool IsMoving { get { return isMoving; } }
        public float CurMoveSpeedPercent { get { return CurMoveSpeed / MaxMoveSpeed; } }
        public float MaxMoveSpeed { get { return Config.maxMoveSpeed; } }
        public float CurMoveSpeed { get { return curMoveSpeed; } }
        public float LastMoveTime { get { return lastMoveTime; } }
        #endregion

        public Transform tfPosTarget;
        public Transform tfLookTarget;

        [Header("Runtime")]
        private bool isMoving = false;
        private float curMoveSpeed = 0;
        float lastMoveTime = 0;
        Vector3 lastPos;

        private void Start()
        {
            if (!tfLookTarget)
                tfLookTarget = tfPosTarget;
            lastPos = tfPosTarget.position;
        }

        Vector3 lastForward;
        /// <summary>
        /// 真实移动时的速度，与物体尺寸有关
        /// 例子：
        /// -AC中根据尺寸来决定移动速度
        /// </summary>
        protected virtual float RuntimeMoveSpeed { get { return curMoveSpeed; } }
        protected virtual void Update()
        {
            isMoving = false;
            curMoveSpeed = 0;
            Vector3 curPos = transform.position;
            Vector3 targetPos = tfPosTarget.position;
            Vector3 targetDirection = targetPos - transform.position;
            float curDistance = Vector3.Distance(targetPos, curPos);
            if (curDistance > Config.stoppingDistance)
            {
                isMoving = true;
                curMoveSpeed = Mathf.Min(curDistance, Config.maxMoveSpeed);
                transform.position = transform.position + targetDirection.normalized * Time.deltaTime * RuntimeMoveSpeed;
                lastMoveTime = Time.time;
            }
            else
            {
                transform.position = targetPos;
            }

            Vector3 worldUp = tfLookTarget.TransformVector(Config.localUpAxis);
            Vector3 forward = tfLookTarget.position - transform.position;
            if (forward == Vector3.zero)
                forward = lastForward;
            else
                lastForward = forward;
            Quaternion targetRotation = Quaternion.LookRotation(forward, worldUp);//通过局部轴的up动态转换为世界worldUp

            transform.rotation = Config.isInstantRotate ? targetRotation : Quaternion.RotateTowards(transform.rotation, targetRotation, Config.rotateSpeed * Time.deltaTime);

            lastPos = curPos;
        }

        #region Define
        [System.Serializable]
        public class ConfigInfo : SerializableDataBase
        {
            [Min(0.01f)] public float maxMoveSpeed = 1;//Max move speed per second
            public Vector3 localUpAxis = new Vector3(0, 0, -1);//Up axis base on tfPosTarget
            public bool isInstantRotate = true;
            [DisableIf(nameof(isInstantRotate))] [AllowNesting] public float rotateSpeed = 360;//Max rotate speed per second
            public float stoppingDistance = 0.01f;
        }
        #endregion
    }
}