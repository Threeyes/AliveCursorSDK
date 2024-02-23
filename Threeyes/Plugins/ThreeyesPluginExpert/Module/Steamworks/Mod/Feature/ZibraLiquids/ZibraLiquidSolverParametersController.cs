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
    public class ZibraLiquidSolverParametersController : ConfigurableComponentBase<ZibraLiquidSolverParameters, SOZibraLiquidSolverParametersControllerConfig, ZibraLiquidSolverParametersController.ConfigInfo>, IZibraLiquidController_SettingHandler
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
            Comp.Gravity = Config.gravity;
            Comp.FluidStiffness = Config.fluidStiffness;
            Comp.ParticleDensity = Config.particleDensity;
            Comp.MaximumVelocity = Config.maximumVelocity;
            Comp.Viscosity = Config.viscosity;
        }
        #endregion

        #region Define
        [Serializable]
        [PersistentChanged(nameof(ConfigInfo.OnPersistentChanged))]
        public class ConfigInfo : SerializableDataBase
        {
            [JsonIgnore] public UnityAction<PersistentChangeState> actionPersistentChanged;

            public Vector3 gravity = new Vector3(0.0f, -9.81f, 0.0f);
            [Tooltip("The stiffness of the liquid.")] [Min(0.0f)] public float fluidStiffness = 0.1f;
            [Tooltip("Resting density of particles. measured in particles/cell. This option directly affects volume of each particle. Higher values correspond to less volume, but higher quality simulation.")] [Range(0.1f, 10.0f)] public float particleDensity = 2.0f;
            [Tooltip("The velocity limit of the particles")] [Range(0.0f, 10.0f)] public float maximumVelocity = 3.0f;
            [Tooltip("how hard it is to change the shape of theliquid")] [Range(0.0f, 1.0f)] public float viscosity = 0.392f;

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