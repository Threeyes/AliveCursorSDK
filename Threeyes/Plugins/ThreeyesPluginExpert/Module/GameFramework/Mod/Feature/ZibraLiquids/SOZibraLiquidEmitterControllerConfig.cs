#if USE_ZibraLiquid
#if !UNITY_ANDROID
using System.Collections;
using System.Collections.Generic;
using Threeyes.Config;
using UnityEngine;

namespace Threeyes.GameFramework
{
    [CreateAssetMenu(menuName = GameFramework_EditorDefinition.AssetMenuPrefix_Root_Feature + "ZibraLiquid/EmitterController", fileName = "ZibraLiquidEmitterControllerConfig")]
    public class SOZibraLiquidEmitterControllerConfig : SOConfigBase<ZibraLiquidEmitterController.ConfigInfo> { }
}
#endif
#endif