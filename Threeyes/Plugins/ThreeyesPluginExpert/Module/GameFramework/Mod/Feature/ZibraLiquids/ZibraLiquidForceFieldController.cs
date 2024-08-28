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

using static com.zibra.liquid.Manipulators.ZibraLiquidForceField;
using Threeyes.Core;

namespace Threeyes.GameFramework
{
    /// <summary>
    ///
    /// Warning:
    /// 1.gameobject's cale will affect ZibraLiquidForceField (eg: scale to 0 will make liquid freeze)
    /// </summary>
    public class ZibraLiquidForceFieldController : ConfigurableComponentBase<ZibraLiquidForceField, SOZibraLiquidForceFieldControllerConfig, ZibraLiquidForceFieldController.ConfigInfo>, IZibraLiquidController_SettingHandler
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
            Comp.Shape = Config.shape;
            Comp.Strength = Config.strength;
            Comp.DistanceDecay = Config.distanceDecay;
        }
        #endregion

        #region Define
        [Serializable]
        [PersistentChanged(nameof(ConfigInfo.OnPersistentChanged))]
        public class ConfigInfo : SerializableDataBase
        {
            [JsonIgnore] public UnityAction<PersistentChangeState> actionPersistentChanged;

            public ForceFieldShape shape = ForceFieldShape.Sphere;
            [Tooltip("The strength of the force acting on the liquid")] [Range(-1.0f, 1.0f)] public float strength = 1.0f;
            [Tooltip("How fast does the force lose its strenght with distance to the center")] [Range(0.0f, 10.0f)] public float distanceDecay = 1.0f;

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