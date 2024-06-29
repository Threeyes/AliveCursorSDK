using System;
using UnityEngine;
using Threeyes.Core;
using Newtonsoft.Json;
using System.Collections.Generic;
namespace Threeyes.Steamworks
{
    /// <summary>
    /// 漂浮类物体，会同时模拟重力
    /// a
    /// PS:
    /// -与Rigidbody同级
    /// -会重载Rigidbody的重力，将重力平均分配到每个顶点（优点是不受重心影响，更加容易控制）
    /// -浮力的方向跟重力的方向相反，竖直向上，与水平面方向无关
    /// 
    /// Todo：
    /// -【重点】参考或替换为该库（https://github.com/dbrizov/NaughtyWaterBuoyancy）：
    ///     -优点：
    ///         -支持运行时更换目标水池
    ///         -不与Shader绑定（因为是直接修改Mesh）
    /// +确认运行时添加的组件，会不会影响序列化（结论：不影响序列化，影响运行时编辑）
    /// -考虑指定模型的缩放，影响 objectDepth
    /// -被拖拽时，临时禁用(【非必须】因为抓取时为Kinematic，此时不受外力影响)（还要考虑Socket的情况）
    /// -支持零重力物体及全局模式
    /// +当运行时临时添加该组件，为其添加默认的effectors(默认都是沉底，增加该组件的目的是提供浮力。如果想漂在水上们就需要提前增加该组件)
    /// -【V2】增加配置项
    /// - 【V2】添加类似AD_XRSocketInteractor的Keylock系统，适用于针对或排除特定物体。还可以用于水粒子系统触发加水等特殊功能
    /// 
    /// Ref: 
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class BuoyantObjectController : ConfigurableComponentBase<BuoyantObjectController, SOBuoyantObjectControllerConfig, BuoyantObjectController.ConfigInfo, BuoyantObjectController.PropertyBag>
    {
        [Header("Physics Settings")]
        public Rigidbody m_Rigidbody;
        public List<Transform> effectors = new List<Transform>();//Note: For square objects, they should be placed at the bottom of the object; For spherical objects, only one can be placed at the center

        //#Runtime
        bool isActive = false;
        float cacheRigidbodyDrag;//缓存的刚体原始阻力
        float cacheRigidbodyAngularDrag;//缓存的刚体原始角阻力
        public BuoyantVolumeController buoyantVolume;//当前所在的Volume
        Vector3[] effectorProjections;//保存effector的运行时目标高度(还用于Gizmo绘制)

        private void Start()
        {
            if (!isActive)//初始化完毕后，如果没有被激活就先隐藏，避免消耗性能
                enabled = false;
        }

        public void SetActive(bool isActive)
        {
            if (isActive)
            {
                TryInit();//尝试使用已有数据进行初始化
                Setup();
            }
            else//Restore setting on hide 
            {
                Restore();
            }
            this.isActive = isActive;
            this.enabled = isActive;//启用Update等
        }

        /// <summary>
        /// 基于当前信息进行程序化初始化
        /// </summary>
        [ContextMenu("ManualInit")]
        public void ManualInit()
        {
            if (!m_Rigidbody)
                m_Rigidbody = GetComponent<Rigidbody>();

            CreateEffectors();
            InitFunc();
        }


        bool hasInit = false;
        private void InitFunc()
        {
            if (hasInit)
                return;

            if (m_Rigidbody)//保存初始状态
            {
                cacheRigidbodyDrag = m_Rigidbody.drag;
                cacheRigidbodyAngularDrag = m_Rigidbody.angularDrag;
            }

            effectorProjections = new Vector3[effectors.Count];
            for (var i = 0; i < effectors.Count; i++)
                effectorProjections[i] = effectors[i].position;
            hasInit = true;
        }

        private void TryInit()
        {
            if (!m_Rigidbody)
                m_Rigidbody = GetComponent<Rigidbody>();
            InitFunc();
        }
        void Setup()
        {
            //ToAdd:缓存其他信息
        }
        void Restore()
        {
            if (m_Rigidbody)
            {
                m_Rigidbody.drag = cacheRigidbodyDrag;
                m_Rigidbody.angularDrag = cacheRigidbodyAngularDrag;
            }
        }

        void FixedUpdate()
        {
            if (!m_Rigidbody)
                return;

            if (buoyantVolume == null)
                return;

            float volumeBuoyancyStrengthScale = buoyantVolume.Config.buoyancyStrengthScale;
            ///PS:
            ///-ForceMode.Acceleration:忽略质量，可以专心计算加速度的差值
            if (Config.isAverageGravity)
            {
                m_Rigidbody.AddForce(-Physics.gravity, ForceMode.Acceleration);//先抵消其重力（Warning：之所以不直接禁用刚体的useGravity，是因为如果漂浮时被XR抓住，会被XR记录抓住前的状态（无重力），从而导致XR取消抓取时变为无重力）
            }

            int effectorAmount = effectors.Count;
            for (var i = 0; i < effectorAmount; i++)
            {
                var effectorPosition = effectors[i].position;
                Vector3 effectorForce = Vector3.zero;
                Vector3 gravityOnThisEffector = Physics.gravity / effectorAmount;//该点平均分配到的重力值（负数），不管是否在Volume都会受到重力影响

                if (Config.isAverageGravity)
                {
                    effectorForce = gravityOnThisEffector;//每个effector均分重力
                }

                effectorProjections[i] = buoyantVolume.GetClosestPointOnWaterSurface(effectorPosition);
                var waveHeight = effectorProjections[i].y;
                var effectorHeight = effectorPosition.y;
                if (effectorHeight < waveHeight) // 仅当effector在水下有效
                {
                    var submersion = Mathf.Clamp01((waveHeight - effectorHeight) / Config.objectDepth);//当前浸没的比例（基于objectDepth）

                    Vector3 buoyancyForce = Vector3.up * (Mathf.Abs(gravityOnThisEffector.y) * (submersion * Config.buoyancyStrength * volumeBuoyancyStrengthScale)); //浮力（向上，后期可向着水法线方向）
                    effectorForce += buoyancyForce;
                }

                m_Rigidbody.AddForceAtPosition(effectorForce, effectorPosition, ForceMode.Acceleration);//最终受到的力
                m_Rigidbody.drag = Config.drag;
                m_Rigidbody.angularDrag = Config.angularDrag;
            }
        }

        #region Editor
        private readonly Color red = new(0.92f, 0.25f, 0.2f);
        private readonly Color green = new(0.2f, 0.92f, 0.51f);
        private readonly Color blue = new(0.2f, 0.67f, 0.92f);
        private readonly Color orange = new(0.97f, 0.79f, 0.26f);
        private void OnDrawGizmos()
        {
            if (effectors == null) return;
            if (!enabled)
                return;

            for (var i = 0; i < effectors.Count; i++)
            {
                if (!Application.isPlaying && effectors[i] != null)
                {
                    Gizmos.color = green;
                    Gizmos.DrawSphere(effectors[i].position, 0.06f);
                }

                else
                {
                    if (effectors[i] == null) return;

                    Gizmos.color = effectors[i].position.y < effectorProjections[i].y ? red : green; // submerged

                    Gizmos.DrawSphere(effectors[i].position, 0.06f);

                    Gizmos.color = orange;
                    Gizmos.DrawSphere(effectorProjections[i], 0.06f);

                    Gizmos.color = blue;
                    Gizmos.DrawLine(effectors[i].position, effectorProjections[i]);
                }
            }
        }
        #endregion

        #region Utility
        /// <summary>
        /// 
        /// </summary>
        void CreateEffectors()
        {
            ///ToUpdate:
            ///-查找N个端点，先转为世界坐标，再转为相对于Rigidbody的局部坐标，在指定位置生成对应的Effector
            ///-计算其最大的深度，用于设置objectDepth（记录所有bounds的max和min，最后计算即可）
            List<Collider> listCollider = transform.FindComponentsInChild<Collider>(true, true).FindAll(c => c.enabled && c.gameObject.activeInHierarchy);//查找所有激活的碰撞体
            List<Bounds> listBounds = listCollider.ConvertAll(c => c.bounds); //获取所有有效Bounds。Collider.bounds: Note that this will be an empty bounding box if the collider is disabled or the game object is inactive.（Collider禁用时Bounds无效，因此上一步要排除禁用的Collider）

            //PS:以下记录的点都基于世界坐标
            Vector3 maxPoint = Vector3.zero;
            Vector3 minPoint = Vector3.zero;
            Vector3 pivotFrontMost = Vector3.zero;
            Vector3 pivotBackMost = Vector3.zero;
            Vector3 pivotLeftMost = Vector3.zero;
            Vector3 pivotRightMost = Vector3.zero;

            foreach (var bounds in listBounds)
            {
                //计算底部的四角点
                Vector3 pivotCenter = bounds.center;
                //bottomCenter.y -= bounds.extents.y;//注释原因：不取底部，否则物体容易翻转
                Vector3 pivotFront = pivotCenter + new Vector3(0, 0, bounds.extents.z);
                Vector3 pivotBack = pivotCenter - new Vector3(0, 0, bounds.extents.z);
                Vector3 pivotRight = pivotCenter + new Vector3(bounds.extents.x, 0, 0);
                Vector3 pivotLeft = pivotCenter - new Vector3(bounds.extents.x, 0, 0);

                RecordMost(ref maxPoint, bounds.max, true);
                RecordMost(ref minPoint, bounds.min, false);

                RecordMost(ref pivotFrontMost, pivotFront, true);
                RecordMost(ref pivotBackMost, pivotBack, false);
                RecordMost(ref pivotRightMost, pivotRight, true);
                RecordMost(ref pivotLeftMost, pivotLeft, false);
            }

            ///根据Bounds自动计算并添加Effector（先扫描其所有Collider并记录其Bounds，然后分别取4个极点）
            GameObject goEffectorRoot = new GameObject("Effectors");
            goEffectorRoot.SetupInstantiate(transform);//默认放置在原点

            GameObject goEffector_F = new GameObject("Effector_Front");
            GameObject goEffector_B = new GameObject("Effector_Back");
            GameObject goEffector_R = new GameObject("Effector_Right");
            GameObject goEffector_L = new GameObject("Effector_Left");
            goEffector_F.transform.position = pivotFrontMost;
            goEffector_B.transform.position = pivotBackMost;
            goEffector_R.transform.position = pivotRightMost;
            goEffector_L.transform.position = pivotLeftMost;

            goEffector_F.transform.SetParent(goEffectorRoot.transform);
            goEffector_B.transform.SetParent(goEffectorRoot.transform);
            goEffector_R.transform.SetParent(goEffectorRoot.transform);
            goEffector_L.transform.SetParent(goEffectorRoot.transform);
            effectors.AddRange(new List<Transform>() { goEffector_F.transform, goEffector_B.transform, goEffector_R.transform, goEffector_L.transform });

            Config.objectDepth = maxPoint.y - minPoint.y;//计算最大深度
            if (Config.objectDepth == 0)
                Config.objectDepth = 0.1f;//避免某些物体运行后才生成碰撞体
            Config.buoyancyStrength = 1;//默认提供与重力相等的浮力（不假设其密度，最大保证通用性）
        }

        /// <summary>
        /// 记录最大/最小值
        /// </summary>
        /// <param name="origin">用于缓存的字段</param>
        /// <param name="input"></param>
        /// <param name="isBiggerOrSmaller">应该记录更大还是更小值</param>
        /// <returns></returns>
        static void RecordMost(ref Vector3 origin, Vector3 input, bool isBiggerOrSmaller)
        {
            if (origin == Vector3.zero)//Init
                origin = input;

            if (isBiggerOrSmaller)
            {
                if (input.sqrMagnitude > origin.sqrMagnitude)
                    origin = input;
            }
            else
            {
                if (input.sqrMagnitude < origin.sqrMagnitude)
                    origin = input;
            }
        }
        #endregion

        #region Define
        [Serializable]
        public class ConfigInfo : SerializableComponentConfigInfoBase
        {
            [Header("Buoyancy")]
            [Tooltip("The depth at which the object is completely submerged")] [Range(0.01f, 5)] public float objectDepth = 1f;//Object's depth in water（可以理解为物体从水面到完全浸没的深度，用于计算对应浮力）(Warning：物体尺寸不能小于该数值，否则会出现抖动)
            [Tooltip("Buoyancy force scaling")] [Range(0.01f, 5)] public float buoyancyStrength = 1.5f;//浮力缩放值（基于重力加速度，当为1时重力与浮力相同，也就是达到平衡，类似无重力状态）
            [Tooltip("The linear drag of the object")] public float drag = 1f;
            [Tooltip("The angular drag of the object")] public float angularDrag = 0.5f;
            [Tooltip("Gravity is evenly distributed to each factor")] public bool isAverageGravity = false;//重力平均分配给每个effector，适用于船等需要保持平衡的物体（Warning：会导致增加额外的计算量）

            [JsonConstructor]
            public ConfigInfo()
            {
                buoyancyStrength = 1.5f;
            }
        }
        public class PropertyBag : ConfigurableComponentPropertyBagBase<BuoyantObjectController, ConfigInfo> { }
        #endregion
    }
}