using com.zibra.liquid.DataStructures;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using Threeyes.Persistent;
using UnityEngine;
using UnityEngine.Events;
using Threeyes.Config;

using static com.zibra.liquid.DataStructures.ZibraLiquidAdvancedRenderParameters;

namespace Threeyes.Steamworks
{
    /// <summary>
    ///
    ///
    /// Warning:
    /// 1.This related Component is rather complex, normally you don't need to expose the config to user.
    /// </summary>
    public class ZibraLiquidAdvancedRenderParametersController : ConfigurableComponentBase<ZibraLiquidAdvancedRenderParameters, SOZibraLiquidAdvancedRenderParametersControllerConfig, ZibraLiquidAdvancedRenderParametersController.ConfigInfo>, IZibraLiquidController_SettingHandler
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
            Comp.RefractionQuality = Config.refractionQuality;
            Comp.RefractionBounces = Config.refractionBounces;
            Comp.UnderwaterRender = Config.underwaterRender;
            Comp.AdditionalJFAIterations = Config.additionalJFAIterations;
            Comp.VertexOptimizationIterations = Config.vertexOptimizationIterations;
            Comp.MeshOptimizationIterations = Config.meshOptimizationIterations;
            Comp.VertexOptimizationStep = Config.vertexOptimizationStep;
            Comp.MeshOptimizationStep = Config.meshOptimizationStep;
            Comp.DualContourIsoSurfaceLevel = Config.dualContourIsoSurfaceLevel;
            Comp.IsoSurfaceLevel = Config.isoSurfaceLevel;
            Comp.RayMarchIsoSurface = Config.rayMarchIsoSurface;
            Comp.RayMarchMaxSteps = Config.rayMarchMaxSteps;
            Comp.RayMarchStepSize = Config.rayMarchStepSize;
            Comp.RayMarchStepFactor = Config.rayMarchStepFactor;
        }
        #endregion

        #region Define
        [Serializable]
        [PersistentChanged(nameof(ConfigInfo.OnPersistentChanged))]
        public class ConfigInfo : SerializableDataBase
        {
            [JsonIgnore] public UnityAction<PersistentChangeState> actionPersistentChanged;

            [Tooltip("Controls the quality of the liquid depth")] public LiquidRefractionQuality refractionQuality = LiquidRefractionQuality.PerPixelRender;
            [Tooltip("Number of bounces of the refraction ray, to see the liquid behing itself you need 2 bounces")] public RayMarchingBounces refractionBounces = RayMarchingBounces.SingleBounce;
            [Tooltip("Enable underwater rendering. Disable it if you don't need it, since it's a bit slower.")] public bool underwaterRender = false;
            [Tooltip("Number of additional sphere render iterations, controls how large spheres can get, has a large impact on performance")] [Range(0, 8)] public int additionalJFAIterations = 0;

            [Tooltip("Number of iterations that move the mesh vertex to the liquid isosurface")] [Range(0, 20)] public int vertexOptimizationIterations = 5;

            [Tooltip("Number of smoothing iterations for the mesh")] [Range(0, 8)] public int meshOptimizationIterations = 2;

            [Tooltip("This parameter moves liquid mesh vertices to be closer to the actual liquid surface. It should be manually fine tuned until you get a smooth mesh.")] [Range(0.0f, 2.0f)] public float vertexOptimizationStep = 0.82f;

            [Tooltip("The strenght of the mesh smoothing per iteration")] [Range(0.0f, 1.0f)] public float meshOptimizationStep = 0.91f;

            [Tooltip("The isovalue at which the mesh vertices are generated")] [Range(0.01f, 2.0f)] public float dualContourIsoSurfaceLevel = 0.025f;

            [Tooltip("Controls the position of the fluid surface. Lower values result in thicker surface.")] [Range(0.01f, 2.0f)] public float isoSurfaceLevel = 0.36f;

            [Tooltip("The isosufrace level for the ray marching. Should be about 1-1/2 of the liquid density.")] [Range(0.0f, 5.0f)] public float rayMarchIsoSurface = 0.65f;

            [Tooltip("Maximum number of steps the ray can go, has a large effect on the performance")] [Range(4, 128)] public int rayMarchMaxSteps = 128;

            [Tooltip("Step size of the ray marching, controls accuracy, also has a large effect on performance")] [Range(0.0f, 1.0f)] public float rayMarchStepSize = 0.2f;

            [Tooltip("Varies the ray marching step size, in some cases might improve performace by slightly reducing ray marching quality")] [Range(1.0f, 10.0f)] public float rayMarchStepFactor = 4.0f;

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