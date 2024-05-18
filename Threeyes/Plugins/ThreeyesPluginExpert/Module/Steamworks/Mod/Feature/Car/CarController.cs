using System;
using System.Collections;
using System.Collections.Generic;
using Threeyes.Config;
using Threeyes.Core;
using Threeyes.RuntimeEditor;
using UnityEngine;
using NaughtyAttributes;
namespace Threeyes.Steamworks
{
    /// <summary>
    /// 
    /// Todo:
    /// +【可配置】移动时增加下压力，以便上墙（参考StandardAssets的CarController.AddDownForce）
    /// +拆分为通用的汽车Controller，通过各种额外的输入组件控制（如PC端、XR端）
    /// -轮子数量可以自定义，每个轮子有单独的Controller（参考Creeper），针对每个轮子的Info，配置其在Break、Motor时是否起作用，以及Steer时是否反转，从而实现原地打转等特殊功能（也可以是统一在这组件进行配置，使用List分别保存每个轮子的物体引用以及相关配置）
    /// -移动到Expert命名空间或SDKFeature中
    /// 
    /// ToAdd：
    /// +SteerAnglePerSecond：每秒增加的方向盘旋转角度
    /// +SteerAngle偏转的进度事件（归一化，方便模型方向盘同步）
    /// +增加通过listWheelAppearance一键导入配置到Config.listWheelInfo的相关字段中
    /// </summary>
    public class CarController : ConfigurableComponentBase<
       CarController, SOCarControllerConfig, CarController.ConfigInfo, CarController.PropertyBag>
    {
        public float CurrentSpeed { get { return m_Rigidbody.velocity.magnitude * 2.23693629f; } }

        [SerializeField] Rigidbody m_Rigidbody;

        //#Events
        public FloatEvent onSteerProgressChanged;//SteerAngle偏转的进度[0,1]，其中0为最小值，1为最大值（归一化，方便模型方向盘同步）
        public FloatEvent onMotorProgressChanged;//加减速的进度
        public List<WheelAppearance> listWheelAppearance = new List<WheelAppearance>();//与listWheelInfo一一对应的引用

        //#Runtime
        private float horizontalInput;
        private float verticalInput;
        private float currentSteerAngle;
        float curMotorForce;
        private float currentbreakForce;
        private bool isBreaking;
        private bool isBoosting;

        #region Public
        public void SetSteering(float steering)
        {
            horizontalInput = steering;
        }
        public void SetAccelerate(float acceleration)
        {
            verticalInput = acceleration;
        }
        public void SetBoost(bool isBoost)
        {
            isBoosting = isBoost;
        }
        public void SetBrake(bool isBreak)
        {
            isBreaking = isBreak;
        }
        #endregion

        #region Callback
        public override void UpdateSetting()
        {
            for (int i = 0; i < Config.listWheelInfo.Count; i++)
            {
                WheelInfo wheelInfo = Config.listWheelInfo[i];
                WheelAppearance wheelAppearance = listWheelAppearance[i];
                WheelCollider wheelCollider = wheelAppearance.wheelCollider;
                wheelCollider.center = wheelInfo.center;
                wheelCollider.forwardFriction = wheelInfo.forwardFriction.ToUnityObj();
                wheelCollider.sidewaysFriction = wheelInfo.sidewaysFriction.ToUnityObj();
            }
        }
        #endregion

        private void FixedUpdate()
        {
            HandleSteering();
            HandleMotor();

            ApplyBreaking();//#刹车（刹车不影响动力输出，配合前驱可以实现漂移）

            UpdateWheels();
        }

        private void HandleSteering()
        {
            //currentSteerAngle = maxSteerAngle * horizontalInput;
            if (horizontalInput == 0)//无转向输入：逐渐居中
            {
                if (currentSteerAngle != 0)
                {
                    float lastSign = Mathf.Sign(currentSteerAngle);
                    currentSteerAngle = Mathf.Clamp(Mathf.Sign(currentSteerAngle) * (Mathf.Abs(currentSteerAngle) - Config.steerAngleDecreaseSpeed * Time.deltaTime), -Config.maxSteerAngle, Config.maxSteerAngle);

                    if (Mathf.Sign(currentSteerAngle) * lastSign < 0)//偏转代表已经越过0，此时可以归0，避免在0附近抖动
                        currentSteerAngle = 0;
                }
            }
            else
            {
                currentSteerAngle = Mathf.Clamp(currentSteerAngle + Config.steerAngleIncreaseSpeed * horizontalInput * Time.deltaTime, -Config.maxSteerAngle, Config.maxSteerAngle);
            }

            onSteerProgressChanged.Invoke((currentSteerAngle + Config.maxSteerAngle) / (Config.maxSteerAngle * 2));//转换为[0,1]区间

            for (int i = 0; i < Config.listWheelInfo.Count; i++)
            {
                WheelInfo wheelInfo = Config.listWheelInfo[i];
                WheelAppearance wheelAppearance = listWheelAppearance[i];
                wheelAppearance.wheelCollider.steerAngle = currentSteerAngle * wheelInfo.steerScale;
            }
        }

        private void HandleMotor()
        {
            curMotorForce = verticalInput * Config.motorForce;
            for (int i = 0; i < Config.listWheelInfo.Count; i++)
            {
                WheelInfo wheelInfo = Config.listWheelInfo[i];
                WheelAppearance wheelAppearance = listWheelAppearance[i];
                wheelAppearance.wheelCollider.motorTorque = curMotorForce * wheelInfo.motorScale * (isBoosting ? Config.boostScale : 1);//如果加速，则乘以加速倍率
            }
            onMotorProgressChanged.Invoke((curMotorForce + Config.motorForce) / (Config.motorForce * 2));//转换为[0,1]区间


            ////#【非必须】【针对车体】Boost
            //if (isBoosting)
            //    AddBoost();

            //#【针对车体】下压力
            AddDownForce();
        }

        //private void AddBoost()
        //{
        //    //ToUpdate:应该是一个衰减的力
        //    m_Rigidbody.AddForce(transform.forward * Config.boostScale /** m_Rigidbody.velocity.magnitude*/ , ForceMode.Impulse);
        //}
        private void AddDownForce()// this is used to add more grip in relation to speed
        {
            m_Rigidbody.AddForce(-transform.up * Config.downForce * m_Rigidbody.velocity.magnitude);//与物体的矢量成正比
        }

        private void ApplyBreaking()
        {
            currentbreakForce = isBreaking ? Config.breakForce : 0f;//需要时刻更新brakeTorque，不刹车要设置为0

            ///PS:
            ///-仅对后轮进行刹车，可模拟漂移
            for (int i = 0; i < Config.listWheelInfo.Count; i++)
            {
                WheelInfo wheelInfo = Config.listWheelInfo[i];
                WheelAppearance wheelAppearance = listWheelAppearance[i];
                wheelAppearance.wheelCollider.brakeTorque = currentbreakForce * wheelInfo.breakScale;
            }

        }
        private void UpdateWheels()//将WheelCollider的位置/旋转 同步给对应的车轮模型
        {
            for (int i = 0; i < Config.listWheelInfo.Count; i++)
            {
                WheelInfo wheelInfo = Config.listWheelInfo[i];
                WheelAppearance wheelAppearance = listWheelAppearance[i];
                UpdateSingleWheel(wheelAppearance.wheelCollider, wheelAppearance.tfWheelModel);
            }
        }

        private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
        {
            Vector3 pos;
            Quaternion rot;
            wheelCollider.GetWorldPose(out pos, out rot);
            wheelTransform.rotation = rot;
            wheelTransform.position = pos;
        }

        #region Editor
#if UNITY_EDITOR
        /// <summary>
        /// Update WheelInfos using current Wheel Colliders' settings
        /// </summary>
        [ContextMenu("Save WheelInfos")]
        void SaveListWheelInfo()
        {
            for (int i = 0; i < Config.listWheelInfo.Count; i++)
            {
                WheelInfo wheelInfo = Config.listWheelInfo[i];
                WheelAppearance wheelAppearance = listWheelAppearance[i];
                wheelInfo.forwardFriction = new WheelFrictionCurveEx(wheelAppearance.wheelCollider.forwardFriction);
                wheelInfo.sidewaysFriction = new WheelFrictionCurveEx(wheelAppearance.wheelCollider.sidewaysFriction);
            }
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
        #endregion

        #region Define
        [Serializable]
        public class ConfigInfo : SerializableComponentConfigInfoBase
        {
            //#Config
            public float motorForce;
            [Range(1, 10)] public float boostScale = 2;//加速缩放
            public float breakForce;
            public float maxSteerAngle = 45;
            [Tooltip("SteerAngle increase per second")] public float steerAngleIncreaseSpeed = 120;//(非必须，如果PC能够模拟出类似效果就不需要该值，也就是horizontalInput输入不为1或-1)
            [Tooltip("SteerAngle decrease per second")]
            public float steerAngleDecreaseSpeed = 120;//设置为0可以避免自动回正
            public float downForce;//移动时增加下压力，以便上墙（参考StandardAssets的CarController.AddDownForce）
            [RuntimeEditorNonAddOrDeleteElement] public List<WheelInfo> listWheelInfo = new List<WheelInfo>();
        }
        public class PropertyBag : ConfigurableComponentPropertyBagBase<CarController, ConfigInfo> { }

        /// <summary>
        /// 存放可供配置的信息
        /// 
        /// PS：
        /// -暂时不提供suspensionSpring，因为不太影响呈现
        /// </summary>
        [Serializable]
        public class WheelInfo
        {
            [RuntimeEditorReadOnly] [Tooltip("The identification name of the wheel")] public string name;//（打包时只读）标识，方便用户知道该序号对应的轮子
            [Range(-1, 1)] public float motorScale = 1;//加减速的缩放值，-1为反向，0为不作用（可用于模拟2驱/4驱）
            [Range(-1, 1)] public float steerScale;//转向的缩放值，-1为反向，0为不转向（用于模拟前轮、后轮、四轮转向）
            [Range(0, 1)] public float breakScale;//刹车的缩放值，0为不参与刹车（后轮刹车可用于模拟漂移）

            [Tooltip("Position the center of the wheel.")] public Vector3 center;//轮子的中心，可用于调节悬挂高度

            ///// ToAdd:
            ///// +WheelCollider的forwardFriction/sidewaysFriction（另外使用bool来决定是否重载，避免用户觉得过于复杂）（PS：如果序列化异常，可以改为用前缀+float将每个类分拆成单独字段）
            [AllowNesting] public WheelFrictionCurveEx forwardFriction;
            [AllowNesting] public WheelFrictionCurveEx sidewaysFriction;
        }

        /// <summary>
        /// 作为UnityEngine.WheelFrictionCurve的中转类，用于设置滑动系数，方便漂移
        /// </summary>
        [Serializable]
        public class WheelFrictionCurveEx
        {
            public float extremumSlip;
            public float extremumValue;
            public float asymptoteSlip;
            public float asymptoteValue;
            public float stiffness;
            public WheelFrictionCurveEx()
            {
            }

            public WheelFrictionCurveEx(WheelFrictionCurve other)
            {
                extremumSlip = other.extremumSlip;
                extremumValue = other.extremumValue;
                asymptoteSlip = other.asymptoteSlip;
                asymptoteValue = other.asymptoteValue;
                stiffness = other.stiffness;
            }

            public WheelFrictionCurve ToUnityObj()
            {
                return new WheelFrictionCurve
                {
                    extremumSlip = extremumSlip,
                    extremumValue = extremumValue,
                    asymptoteSlip = asymptoteSlip,
                    asymptoteValue = asymptoteValue,
                    stiffness = stiffness
                };
            }
        }

        /// <summary>
        /// 存放Unity引用组件
        /// </summary>
        [Serializable]
        public class WheelAppearance
        {
            public WheelCollider wheelCollider;
            public Transform tfWheelModel;
        }

        #endregion
    }
}