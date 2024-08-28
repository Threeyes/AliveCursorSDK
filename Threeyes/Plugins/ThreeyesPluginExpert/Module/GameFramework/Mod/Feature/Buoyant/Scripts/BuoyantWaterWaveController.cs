using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.GameFramework
{
    /// <summary>
    /// Provide shader's waveinfo at runtime
    /// 
    /// ToUpdate：
    /// -将与Shader的相关实现拆分成WaveProvider，方便后续兼容不同的Shader
    /// 
    /// Require:
    /// -Shader have wave configs, such as: [Shader Graphs/Stylized Water]
    /// </summary>
    [RequireComponent(typeof(BuoyantVolumeController))]
    public class BuoyantWaterWaveController : ConfigurableComponentBase<BuoyantWaterWaveController, SOBuoyantWaterWaveControllerConfig, BuoyantWaterWaveController.ConfigInfo, BuoyantWaterWaveController.PropertyBag>
    {
        public Renderer m_Renderer;

        [Header("Runtime")]
        //Runtime
        Material material;
        bool hasInited = false;
        [SerializeField] BuoyantWaveInfo waterInfo = new BuoyantWaveInfo();

        /// <summary>
        /// Get offset on giving point
        /// </summary>
        /// <param name="effectorPosition"></param>
        /// <returns></returns>
        public Vector3 GetWaveDisplacement(Vector3 effectorPosition)
        {
            TryInit();

            if (waterInfo.IsValid)
                return GerstnerWaveDisplacement.GetWaveDisplacement(effectorPosition, waterInfo.steepness, waterInfo.waveLength, waterInfo.speed, waterInfo.directions);
            return Vector3.zero;//尚未初始化，或者参数无效
        }

        [ContextMenu("Init")]
        private void TryInit()
        {
            if (hasInited)
                return;
            if (m_Renderer)
            {
                material = m_Renderer.material;
                waterInfo = new BuoyantWaveInfo();

                //以下都是可选项，需要材质包含该属性才能初始化对应字段
                if (material.HasFloat("_Wave_Steepness"))
                    waterInfo.steepness = material.GetFloat("_Wave_Steepness");
                if (material.HasFloat("_Wave_Length"))
                    waterInfo.waveLength = material.GetFloat("_Wave_Length");
                if (material.HasFloat("_Wave_Speed"))
                    waterInfo.speed = material.GetFloat("_Wave_Speed");
                if (material.HasVector("_Wave_Directions"))
                    waterInfo.directions = material.GetVector("_Wave_Directions");
                hasInited = true;
            }
            else
            {
                Debug.LogError($"{nameof(m_Renderer)} is null!");
            }
        }

        #region Define
        [Serializable]
        public class BuoyantWaveInfo//PS:因为是材质属性，暂时不放到Config中，后期可以通过MaterialController暴露，以及改为运行时获取
        {
            public bool IsValid
            {
                get
                {
                    return steepness != 0 && waveLength != 0 && speed != 0;
                }
            }

            public float steepness;
            public float waveLength;
            public float speed;
            public Vector4 directions = new Vector4(0, 0.5f, 1, 0.2f);
        }

        [Serializable]
        public class ConfigInfo : SerializableComponentConfigInfoBase
        {
            [JsonConstructor]
            public ConfigInfo()
            {
            }
        }
        public class PropertyBag : ConfigurableComponentPropertyBagBase<BuoyantWaterWaveController, ConfigInfo> { }
        #endregion

    }
}