using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Threeyes.Core;

namespace Threeyes.Steamworks
{
    public class LightController : ConfigurableComponentBase<LightController, SOLightControllerConfig, LightController.ConfigInfo, LightController.PropertyBag>
    {
        #region Property & Field
        public Light _light;

        ///PS:
        ///-仅提供常见及通用的事件
        public ColorEvent onSetLightColor;
        #endregion

        #region IModHandler
        public override void UpdateSetting()
        {
            if (_light)
            {
                _light.color = Config.color;
                _light.intensity = Config.intensity;
                _light.range = Config.range;

                _light.shadows = Config.shadowType;
                _light.shadowStrength = Config.shadowStrength;
            }
            onSetLightColor.Invoke(Config.color);
        }
        #endregion

        #region Editor
#if UNITY_EDITOR
        [ContextMenu("InitUsingTargetProperties")]
        void InitUsingTargetProperties()
        {
            if (_light)
            {
                Config.color = _light.color;
                Config.intensity = _light.intensity;
                Config.range = _light.range;

                Config.shadowType = _light.shadows;
                Config.shadowStrength = _light.shadowStrength;
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif
        #endregion

        #region Define
        [Serializable]
        public class ConfigInfo : SerializableComponentConfigInfoBase
        {
            public Color color = Color.white;
            [Range(0, float.MaxValue)] public float intensity = 1;
            [Range(0, float.MaxValue)] public float range = 10;

            [Header("Shadow")]
            public LightShadows shadowType = LightShadows.None;
            [HideIf(nameof(shadowType), LightShadows.None)] [Range(0, 1)] public float shadowStrength = 1;
            ///ToAdd（其他通用的光照相关的参数）：

            public ConfigInfo()
            {
            }
        }

        public class PropertyBag : ConfigurableComponentPropertyBagBase<LightController, ConfigInfo> { }
        #endregion
    }
}