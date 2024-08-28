#if USE_ZibraLiquid
#if !UNITY_ANDROID
using com.zibra.liquid.Manipulators;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using Threeyes.Persistent;
using UnityEngine;
using UnityEngine.Events;
using Threeyes.Config;

using static com.zibra.liquid.Manipulators.ZibraLiquidEmitter;
using Threeyes.Core;

namespace Threeyes.GameFramework
{
    public class ZibraLiquidEmitterController : ConfigurableComponentBase<ZibraLiquidEmitter, SOZibraLiquidEmitterControllerConfig, ZibraLiquidEmitterController.ConfigInfo>, IZibraLiquidController_SettingHandler
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
            Comp.VolumePerSimTime = Config.volumePerSimTime;
            Comp.InitialVelocity = Config.initialVelocity;
            Comp.PositionClampBehavior = Config.positionClampBehavior;
        }
        #endregion


        #region Define
        [Serializable]
        [PersistentChanged(nameof(ConfigInfo.OnPersistentChanged))]
        public class ConfigInfo : SerializableDataBase
        {
            [JsonIgnore] public UnityAction<PersistentChangeState> actionPersistentChanged;

            [Tooltip("Emitted volume per simulation time unit")] [Min(0.0f)] public float volumePerSimTime = 0.125f;
            [Tooltip("Initial velocity of newly created particles")] public Vector3 initialVelocity = new Vector3(0, 0, 0);
            [Tooltip("Controls what whether effective position of emitter will clamp to container bounds.")] public ClampBehaviorType positionClampBehavior = ClampBehaviorType.Clamp;

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
#endif
#endif