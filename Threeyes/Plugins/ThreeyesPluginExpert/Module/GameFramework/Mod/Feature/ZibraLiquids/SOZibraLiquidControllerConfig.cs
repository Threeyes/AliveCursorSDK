#if USE_ZibraLiquid
#if !UNITY_ANDROID
using System.Collections;
using System.Collections.Generic;
using Threeyes.Config;
using UnityEngine;

namespace Threeyes.GameFramework
{
    [CreateAssetMenu(menuName = GameFramework_EditorDefinition.AssetMenuPrefix_Root_Feature + "ZibraLiquid/Controller", fileName = "ZibraLiquidControllerConfig")]
    public class SOZibraLiquidControllerConfig : SOConfigBase<ZibraLiquidController.ConfigInfo> { }
}
#endif
#endif