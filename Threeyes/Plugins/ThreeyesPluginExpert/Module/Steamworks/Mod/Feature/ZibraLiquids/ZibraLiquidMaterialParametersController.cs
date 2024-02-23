using com.zibra.liquid.DataStructures;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using Threeyes.Persistent;
using UnityEngine;
using UnityEngine.Events;
using Threeyes.Config;
using Threeyes.Core;

namespace Threeyes.Steamworks
{

    /// <summary>
    ///
    /// PS:
    /// 1. Use short name to avoid path length restriction
    /// </summary>
    public class ZibraLiquidMaterialParametersController : ConfigurableComponentBase<ZibraLiquidMaterialParameters, SOZibraLiquidMaterialParametersControllerConfig, ZibraLiquidMaterialParametersController.ConfigInfo>, IZibraLiquidController_SettingHandler
    {
        #region Unity Method
        private void Awake()
        {
            Config.actionPersistentChanged += OnPersistentChanged;
        }
        private void OnDestroy()
        {
            Config.actionPersistentChanged -= OnPersistentChanged;
        }
        #endregion

        #region  Callback
        void OnPersistentChanged(PersistentChangeState persistentChangeState)
        {
            if (persistentChangeState == PersistentChangeState.Load)
                return;
            UpdateSetting();
        }
        public void UpdateSetting()
        {
            Comp.Color = Config.color;
            Comp.ReflectionColor = Config.reflectionColor;
            Comp.EmissiveColor = Config.emissiveColor;
            Comp.Roughness = Config.roughness;
            Comp.Metalness = Config.metalness;
            Comp.ScatteringAmount = Config.scatteringAmount;
            Comp.AbsorptionAmount = Config.absorptionAmount;
            Comp.IndexOfRefraction = Config.indexOfRefraction;
            Comp.FluidSurfaceBlur = Config.fluidSurfaceBlur;
            Comp.ParticleScale = Config.particleScale;
            Comp.FoamIntensity = Config.foamIntensity;
            Comp.FoamAmount = Config.foamAmount;
            Comp.BlurRadius = Config.blurRadius;
        }
        #endregion

        #region Define
        [Serializable]
        [PersistentChanged(nameof(ConfigInfo.OnPersistentChanged))]
        public class ConfigInfo : SerializableDataBase
        {
            [JsonIgnore] public UnityAction<PersistentChangeState> actionPersistentChanged;

            [Tooltip("The color of the liquid body")] public Color color = new Color(0.3411765f, 0.92156863f, 0.85236126f, 1.0f);
            [Tooltip("The color of the liquid reflection.")] [ColorUsage(true, true)] public Color reflectionColor = new Color(1.39772f, 1.39772f, 1.39772f, 1.0f);
            [Tooltip("The emissive color of the liquid. Normally black for most liquids.")] [ColorUsage(true, true)] public Color emissiveColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);

            [Range(0.0f, 1.0f)] public float roughness = 0.04f;
            [Tooltip("The metalness of the surface")] [Range(0.0f, 1.0f)] public float metalness = 0.3f;
            [Tooltip("The amount of light being scattered by the liquid volume. Visually adds a fog to the fluid volume. Maximum value makes the liquid opaque.")] [Range(0.0f, 30.0f)] public float scatteringAmount = 0.1f;
            [Tooltip("The amount of light absorbed in the liquid volume. Visually darkens all colors except to the selected liquid color.")] [Range(0.0f, 30.0f)] public float absorptionAmount = 1.0f;//Affects opacity
            [Tooltip("The index of refraction")] [Range(1.0f, 3.0f)] public float indexOfRefraction = 1.333f;
            [Tooltip("The radius of the blur of the liquid density on the simulation grid. Controls the smoothness of the normals.")] [Range(0.01f, 4.0f)] public float fluidSurfaceBlur = 1.5f;
            [Tooltip("Particle rendering scale compared to the cell size. This parameter only works in Particle Render Mode.")] [Range(0.0f, 4.0f)] public float particleScale = 1.5f;
            [Tooltip("This parameter only works in Particle Render Mode.")] [Range(0.0f, 2.0f)] public float foamIntensity = 0.8f;
            [Tooltip("Foam appearance threshold. This parameter only works in Particle Render Mode.")] [Range(0.0f, 4.0f)] public float foamAmount = 1.0f;
            [Tooltip("Blur radius of particle normals and depth. This parameter only works in Particle Render Mode.")] [Range(0.0001f, 0.1f)] public float blurRadius = 0.0581f;

            #region Callback
            void OnPersistentChanged(PersistentChangeState persistentChangeState)
            {
                actionPersistentChanged.Execute(persistentChangeState);
            }
            #endregion
        }
        #endregion
    }
}