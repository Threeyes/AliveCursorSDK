using System;
using System.Collections;
using Threeyes.Config;
using Threeyes.RuntimeEditor;
using UnityEngine;
using Newtonsoft.Json;
using Threeyes.Core;

namespace Threeyes.GameFramework
{
    /// <summary>
    /// Add Force inside volume, force may decay according to distance
    /// 为区域中的物体添加力，力度可以设置为根据距离衰减
    /// 
    /// 用途：
    /// -声波、风力、水流
    /// -仿《王国之泪》风扇机，为物体提供推力
    /// 
    /// Todo：
    /// -对TriggerEnter的物体施加力
    /// -可选更新力的方式：Update、Manual
    /// -提供可编辑的配置：最大距离、最大力度、是否根据距离衰减（后期可通过AnimationCurver设置）
    /// -可根据maxDistance，在运行时修改Trigger的Z轴（注意Box/Sphere的Trigger的Center.Z要为0.5，确保碰撞体的原点在根部）。需要增加一个对Collider的可选引用
    /// -如果被Socket吸附，则反作用力改为作用在Socket上（通过宏定义来限制，避免AC报错）（Socket物体可以继承通用接口引用，使该组件更加通用）
    /// 
    /// PS:
    /// -该组件可以挂载Rigidbody或Trigger上，OnTriggerStay都可以生效
    /// -可以适配不同的Trigger（Box/Sphere）
    /// -可以通过设置Trigger的LayerOverrides，来决定哪些物体可以交互
    /// 
    /// </summary>
    public class VolumeForceController : ConfigurableComponentBase<VolumeForceController, SOVolumeForceControllerConfig, VolumeForceController.ConfigInfo, VolumeForceController.PropertyBag>
    , IRuntimeEditorSelectEnterHandler
    , IRuntimeEditorSelectExitHandler
    {
        public Transform TfForceCenter { get { if (!tfForceCenter) return transform; return tfForceCenter; } }
        public float ForceScale { get { return forceScale; } set { forceScale = value; } }//运行时更新缩放
        float RuntimeForce { get { return Config.maxForce * forceScale; } }
        float RuntimeReactingForce { get { return Config.maxReactingForce * forceScale; } }

        [SerializeField] float forceScale = 0;//Runtime scaler for force/reactingForce（默认设置为0，可以避免在初始化/还原时意外受力而偏移）
        [SerializeField] Rigidbody rig;//Rig for this collider, which will receive the reacting force
        [SerializeField] Transform tfVolumePivot;//The point of the volume(通常为碰撞体的父物体，其位置为碰撞体的Z轴最低点)（不直接指定碰撞体类型，可以用任意形状的碰撞体）
        [SerializeField] Transform tfForceCenter;//Center Point of the force
        [SerializeField] GameObject goColShapeIndicator;//[Optional] The shapk of the collider, use this to temp display the collider area on runtime editing ConfigInfo.maxDistance. (Remember to hide this when you finish editing it!)

        #region Unity Method
        private void OnEnable()
        {
            goColShapeIndicator?.SetActive(false);
        }
        private void OnTriggerStay(Collider other)
        {
            //#1 作用力
            if (ForceScale == 0)
                return;
            Rigidbody othersRig = other.attachedRigidbody;
            if (!othersRig)//只针对刚体
                return;

            Vector3 closestPoint = other.ClosestPointOnBounds(TfForceCenter.position);//查找最近的点（ToUpdate：后续可以通过forceDirection选择目标点）
            float distance = Vector3.Distance(closestPoint, TfForceCenter.position);
            if (distance > Config.maxForceDistance)
                return;

            float curForcePercent = Config.forceDecayWithDistance ? (Config.maxForceDistance - distance) / Config.maxForceDistance : Config.maxForceDistance;
            Vector3 forceDir = Config.forceDirection == ForceDirection.Forward ? TfForceCenter.forward : (closestPoint - TfForceCenter.position).normalized;

            othersRig.AddForceAtPosition(forceDir * curForcePercent * RuntimeForce, closestPoint, ForceMode.Force);
        }
        private void FixedUpdate()
        {
            //# 持续从TfForceCenter的位置施加反作用力
            if (rig && RuntimeReactingForce > 0)
            {
                rig.AddForceAtPosition(-TfForceCenter.forward * RuntimeReactingForce, TfForceCenter.position, ForceMode.Force);
            }
        }
        #endregion

        #region IModHandler
        public override void UpdateSetting()
        {
            UpdateCollider();//实时更新碰撞体
        }

        float cacheMaxDistance;

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Has Updated</returns>
        bool UpdateCollider()
        {
            if (!tfVolumePivot)
                return false;

            if (cacheMaxDistance != Config.maxForceDistance)
            {
                //#2 缩放碰撞体的Z轴
                Vector3 localScale = tfVolumePivot.localScale;
                localScale.z = Config.maxForceDistance;//相对位置
                tfVolumePivot.localScale = localScale;

                //#当更改maxForceDistance后，临时激活对应的物体，方便用户了解大概的作用范围（使用Mesh，是为了方便呈现不同形状）
                if (Application.isPlaying && isRuntimeEditing)
                {
                    TempShowColliderMesh();
                }

                cacheMaxDistance = Config.maxForceDistance;
                return true;
            }
            return false;
        }

        protected UnityEngine.Coroutine cacheEnum;
        void TempShowColliderMesh()
        {
            if (!goColShapeIndicator)
                return;
           CoroutineManager.StartCoroutineSoloEx(IETempShowColliderMesh(),ref cacheEnum);
        }
        IEnumerator IETempShowColliderMesh()
        {
            goColShapeIndicator.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            goColShapeIndicator.SetActive(false);
        }
        #endregion

        #region Editor Method
        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            UpdateCollider();//在用户修改Config时同步更新
        }
        #endregion

        #region RuntimeEditor
        private bool isRuntimeEditing = false;//正在运行时被选中
        public void OnRuntimeEditorSelectEntered(RESelectEnterEventArgs args)
        {
            isRuntimeEditing = true;
        }
        public void OnRuntimeEditorSelectExited(RESelectExitEventArgs args)
        {
            isRuntimeEditing = false;
        }
        #endregion

        #region Define
        [Serializable]
        public class ConfigInfo : SerializableComponentConfigInfoBase
        {
            public ForceDirection forceDirection = ForceDirection.Forward;
            [Tooltip("The force will decay with distance")] public bool forceDecayWithDistance = true;
            [Tooltip("Maximum force applied to other objects")][Min(0)] public float maxForce = 5;
            [Tooltip("The maximum distance to apply force to other objects")][Min(0)] public float maxForceDistance = 2;//The farthest distance of force
            [Tooltip("The maximum reaction force applied to self")][Min(0)] public float maxReactingForce = 0;//反作用力

            [JsonConstructor]
            public ConfigInfo()
            {
            }
        }

        public class PropertyBag : ConfigurableComponentPropertyBagBase<VolumeForceController, ConfigInfo> { }
        public enum ForceDirection
        {
            FromPivot,//Apply force from center to cloest target point（适用于）
            Forward//Base on TfPivot's forward axis
        }
        #endregion
    }
}