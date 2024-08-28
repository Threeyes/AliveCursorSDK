#if USE_ZibraLiquid
#if !UNITY_ANDROID
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.GameFramework
{
    public interface IZibraLiquidController_SettingHandler
    {
        /// <summary>
        /// Update Config setting
        /// </summary>
        void UpdateSetting();
    }
}
#endif
#endif