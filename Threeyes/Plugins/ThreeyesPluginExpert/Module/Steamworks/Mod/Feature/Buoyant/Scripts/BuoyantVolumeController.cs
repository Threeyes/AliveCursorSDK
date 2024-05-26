using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Threeyes.Core;
using Newtonsoft.Json;
using NaughtyAttributes;
using System.Linq;

namespace Threeyes.Steamworks
{
    /// <summary>
    /// A container that provides buoyancy
    /// 
    /// PS:
    /// -该物体需要包含Trigger，且水平面对应物体中心和Trigger顶部
    /// -支持
    /// 
    /// ToAdd：
    /// -可以选择是否再OnTrigger时更新，适用于可以移动的Container
    /// -可选是否自动为掉进的物体增加BuoyantObject组件
    /// -应该提炼出基类，针对通用的无参数水体，以便兼容更多材质；该类针对特殊材质
    /// -在此负责更新：
    ///     -物体在水体中时，会根据其浸入的深度，增加对应的反作用力。离开水体时力度为0，完全浸入时力度最大
    /// 
    /// Bug:
    /// -会导致附着在Socket上的物体异常抖动
    /// 
    /// Note：
    /// -需要物体有任意Trigger
    /// 
    /// ToUpdate：
    /// -通过TriggerEventBroadcaster等组件，把Trigger的事件发送到该组件，方便把该组件放到根物体以便RE编辑(参考ColliderEventReceiverBase，通过接口调用对应方法。为了性能，通过Mono组件引用）
    /// </summary>
    //[RequireComponent(typeof(Collider))]//Require trigger collider （可能会导致uMod加载报错，先注释）
    public class BuoyantVolumeController : ConfigurableComponentBase<BuoyantVolumeController, SOBuoyantVolumeControllerConfig, BuoyantVolumeController.ConfigInfo, BuoyantVolumeController.PropertyBag>
    {
        public Transform TfWaterSurface
        {
            get
            {
                if (!tfWaterSurface)
                    tfWaterSurface = transform;
                return tfWaterSurface;
            }
        }
        public float WaterHeight { get { return TfWaterSurface.position.y; } }

        [Tooltip("[Optional] Custom water surface panel")] [SerializeField] Transform tfWaterSurface;//[Optional] 自定义的水平面(可通过LiquidController在运行时控制其位置及旋转)

        //Runtime
        [Header("Runtime")]
        [SerializeField] BuoyantWaterWaveController waveController;//[Optional] extra wave info
        private void Start()
        {
            if (!waveController)
                waveController = GetComponent<BuoyantWaterWaveController>();
        }
        private void OnTriggerEnter(Collider other)
        {
            Rigidbody rigidbody = other.GetRootRigidbody();
            if (!rigidbody)
                return;
            if (rigidbody.isKinematic)//忽略Kinematic
                return;

            BuoyantObjectController buoyantObjectController = rigidbody.GetComponent<BuoyantObjectController>();
            if (!buoyantObjectController)
            {
                if (!Config.isMakeEnterObjFloatable)
                {
                    return;
                }
                else//自动添加组件
                {
                    if (rigidbody.GetComponent<NotBuoyantableObject>() != null)//跳过标记为不可漂浮的物体
                        return;

                    buoyantObjectController = rigidbody.AddComponentOnce<BuoyantObjectController>();
                    buoyantObjectController.ManualInit();//先手动初始化
                    buoyantObjectController.Config.buoyancyStrength = Config.newlyFloatableObjStrength;//Set default strength
                }
            }

            if (buoyantObjectController.buoyantVolume == null)
            {
                buoyantObjectController.buoyantVolume = this;
                buoyantObjectController.SetActive(true);//启用
            }
        }

        readonly HashSet<Collider> m_StayedColliders = new HashSet<Collider>();
        private void OnTriggerStay(Collider other)
        {
            m_StayedColliders.Add(other);
        }
        private void OnTriggerExit(Collider other)
        {
            Rigidbody rigidbody = other.attachedRigidbody;
            if (!rigidbody)
                return;
            //if (rigidbody.isKinematic)//忽略Kinematic（Warning：不能忽略，否则会因为被XR抓取时无法执行后续的禁用方法）
            //    return;

            BuoyantObjectController buoyantObjectController = rigidbody.GetComponent<BuoyantObjectController>();
            if (!buoyantObjectController)
                return;

            if (this == buoyantObjectController.buoyantVolume)//仅当离开当前正处于的Volume时才清空，避免因为意外进入其他Volume导致丢失信息
            {
                bool shouldDisableObjController = true;//是否应该禁用目标的Controller（条件为目标完全离开该Volume）

                ///ToUpdate:
                ///-如果该刚体含有多个碰撞体，应该是检查所有碰撞体都离开该Volume才禁用，否则会导致某个碰撞体出界就禁用该组件的Bug(或者改为由Controller触发)
                List<Collider> listCollider = rigidbody.GetComponentsInChildren<Collider>(false).ToList();//仅考虑激活的Collider
                if (listCollider.Count > 1)//仅考虑多个碰撞体
                {
                    foreach (var stayedCollider in m_StayedColliders)
                    {
                        if (stayedCollider == null) continue;
                        if (stayedCollider == other) continue;//忽略与当前碰撞体相同的情况

                        if (listCollider.Contains(stayedCollider))
                        {
                            shouldDisableObjController = false;
                            break;
                        }
                    }
                }

                ///ToUpdate:
                ///-可以是所有effector都在水面上时禁用，适用于液面升降的物体（非必须）
                if (shouldDisableObjController)
                {
                    buoyantObjectController.buoyantVolume = null;
                    buoyantObjectController.SetActive(false);//禁用
                }
            }

            m_StayedColliders.Remove(other);
        }

        #region Utility
        /// <summary>
        /// 获取输入点在水平面的对应点
        /// </summary>
        /// <param name="effectorPosition">effector的世界坐标</param>
        /// <returns>effector投影到水平面的世界坐标点</returns>
        public Vector3 GetClosestPointOnWaterSurface(Vector3 effectorPosition)
        {
            Vector3 effectorProjection = effectorPosition;

            effectorProjection.y = WaterHeight;//ToUpdate:应该是计算effectorPosition所在世界Y轴，与targetWaterSurface的XZ平面的焦点的Y值（方便计算倾斜液体）

            if (waveController)//当有额外的Wave信息：计算该点受波浪影响的偏移值（因为带波浪的Shader通常不会倾斜，因此不需要下方的水平面倾斜计算）
            {
                effectorProjection.y += waveController.GetWaveDisplacement(effectorPosition).y;//叠加波浪的y轴位移值
            }
            else//否则：视为简单水平面（整个平面，无波浪）
            {
                ///PS:
                //计算effectorPosition沿Y轴与水平面的交点（因为浮力与重力方向相反，所以使用世界坐标Y轴，而不是水平面法线），从而计算出距离水面的高度（主要针对倾斜水平面）
                Plane plane = new Plane(TfWaterSurface.up, TfWaterSurface.position);//Warning：WaterSurface物体可能因为LiquidController的低频晃动导致Up翻转
                Ray ray = new Ray(new Vector3(effectorPosition.x, effectorPosition.y - 1000, effectorPosition.z), Vector3.up);//起始点放在最下方，方向与重力相反，确保能与平面相交（ToUpdate：改为无端点的线）
                if (plane.Raycast(ray, out float enter))
                {
                    Vector3 hitPoint = ray.GetPoint(enter);//获取Y轴与水平面的交互
                    effectorProjection.y = hitPoint.y;
                }
                else
                    Debug.LogError("Line not intersect with Plane!");
            }

            //Debug.LogError("Test: " + GerstnerWaveDisplacement.GetWaveDisplacement(effectorPosition, steepness, wavelength, speed, directions));
            return effectorProjection;
        }
        #endregion

        #region Define
        [Serializable]
        public class ConfigInfo : SerializableComponentConfigInfoBase
        {
            [Min(0)] public float buoyancyStrengthScale = 1;//针对掉进来的物体的浮力缩放

            [Tooltip("If the rigid body that falls into the container cannot float, add corresponding components and initialize it")] public bool isMakeEnterObjFloatable = true;// 如果掉进该容器的刚体不可漂浮，则为其添加对应组件并初始化
            [Tooltip("For automatically added objects, their default buoyancy")] [ShowIf(nameof(isMakeEnterObjFloatable))] [AllowNesting] [Range(0.01f, 5)] public float newlyFloatableObjStrength = 1.5f;//针对自动添加的物体，其默认的浮力

            [JsonConstructor]
            public ConfigInfo()
            {
            }
        }
        public class PropertyBag : ConfigurableComponentPropertyBagBase<BuoyantVolumeController, ConfigInfo> { }
        #endregion
    }
}