#if USE_ZibraLiquid
#if !UNITY_ANDROID
using System.Collections;
using System.Collections.Generic;
using Threeyes.Config;
using UnityEngine;

namespace Threeyes.GameFramework
{
    [CreateAssetMenu(menuName = GameFramework_EditorDefinition.AssetMenuPrefix_Root_Feature + "ZibraLiquid/MaterialParametersController", fileName = "ZibraLiquidMaterialParametersControllerConfig")]
    public class SOZibraLiquidMaterialParametersControllerConfig : SOConfigBase<ZibraLiquidMaterialParametersController.ConfigInfo> { }
}
#endif
#endif