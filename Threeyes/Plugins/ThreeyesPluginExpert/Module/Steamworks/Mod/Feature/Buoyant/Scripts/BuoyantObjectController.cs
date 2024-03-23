using System;
using UnityEngine;
using Newtonsoft.Json;
namespace Threeyes.Steamworks
{
    /// <summary>
    /// 漂浮类物体
    /// 
    /// PS:
    /// -与Rigidbody同级
    /// 
    /// Todo：
    /// -【Wasted：不能】研究替换为：https://github.com/dbrizov/NaughtyWaterBuoyancy
    /// +从Container的Shader实例读取属性
    /// -考虑指定模型的缩放，影响objectDepth
    /// -被拖拽时，临时禁用（或者通过条件判断）
    /// -【V2】增加配置项
    /// 
    /// Ref:
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class BuoyantObjectController : ConfigurableComponentBase<BuoyantObjectController, SOBuoyantObjectControllerConfig, BuoyantObjectController.ConfigInfo, BuoyantObjectController.PropertyBag>
    {
        //——BuoyantContainer(Stylized Water) Settings——
        [Header("Water")]//【Optional】：进入所在的水域时，获取其配置信息
        [SerializeField] public float waterHeight = 0.0f;//对应OceanSurface物体的世界高度

        [Header("Waves (Shader)")]//【Optional】：进入所在的水域时，获取其配置信息
        public float steepness;
        public float wavelength;
        public float speed;
        public Vector4 directions = new Vector4(0, 0.5f, 1, 0.2f);

        [Header("Physics Settings")]
        public Rigidbody m_Rigidbody;
        public Transform[] effectors;

        //Runtime
        Vector3[] effectorProjections;

        private void Start()
        {
            effectorProjections = new Vector3[effectors.Length];
            for (var i = 0; i < effectors.Length; i++)
                effectorProjections[i] = effectors[i].position;

        }
        private void OnEnable()
        {
            m_Rigidbody.useGravity = false;//激活时禁用重力
        }

        private void OnDisable()
        {
            m_Rigidbody.useGravity = true;//禁用时恢复重力
        }

        private void FixedUpdate()
        {
            var effectorAmount = effectors.Length;

            for (var i = 0; i < effectorAmount; i++)
            {
                var effectorPosition = effectors[i].position;

                effectorProjections[i] = effectorPosition;
                effectorProjections[i].y = waterHeight + GerstnerWaveDisplacement.GetWaveDisplacement(effectorPosition, steepness, wavelength, speed, directions).y;

                // gravity
                m_Rigidbody.AddForceAtPosition(Physics.gravity / effectorAmount, effectorPosition, ForceMode.Acceleration);

                var waveHeight = effectorProjections[i].y;
                var effectorHeight = effectorPosition.y;

                if (!(effectorHeight < waveHeight)) continue; // submerged（忽略水上）

                var submersion = Mathf.Clamp01(waveHeight - effectorHeight) / Config.objectDepth;
                var buoyancy = Mathf.Abs(Physics.gravity.y) * submersion * Config.strength;

                // buoyancy
                m_Rigidbody.AddForceAtPosition(Vector3.up * buoyancy, effectorPosition, ForceMode.Acceleration);

                // drag
                m_Rigidbody.AddForce(-m_Rigidbody.velocity * (Config.velocityDrag * Time.fixedDeltaTime), ForceMode.VelocityChange);

                // torque
                m_Rigidbody.AddTorque(-m_Rigidbody.angularVelocity * (Config.angularDrag * Time.fixedDeltaTime), ForceMode.Impulse);
            }
        }

        private readonly Color red = new(0.92f, 0.25f, 0.2f);
        private readonly Color green = new(0.2f, 0.92f, 0.51f);
        private readonly Color blue = new(0.2f, 0.67f, 0.92f);
        private readonly Color orange = new(0.97f, 0.79f, 0.26f);
        private void OnDrawGizmos()
        {
            if (effectors == null) return;

            for (var i = 0; i < effectors.Length; i++)
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

        #region IModHandler
        public override void UpdateSetting()
        {
        }
        #endregion

        #region Define
        [Serializable]
        public class ConfigInfo : SerializableComponentConfigInfoBase
        {
            [Header("Buoyancy")]
            [Range(0.01f, 5)] public float strength = 1f;
            [Tooltip("Object's depth in water")] [Range(0.01f, 5)] public float objectDepth = 1f;//Object's depth in water(Warning：物体尺寸不能小于该数值，否则会出现抖动)
            public float velocityDrag = 1f;
            public float angularDrag = 0.5f;

            [JsonConstructor]
            public ConfigInfo() { }
        }
        public class PropertyBag : ConfigurableComponentPropertyBagBase<BuoyantObjectController, ConfigInfo> { }
        #endregion
    }
}