using System.Collections;
using System.Collections.Generic;
using Threeyes.Config;
using UnityEngine;
namespace Threeyes.GameFramework
{
    [CreateAssetMenu(menuName = GameFramework_EditorDefinition.AssetMenuPrefix_Root_Feature + "Buoyant/ObjectController", fileName = "BuoyantObjectControllerConfig")]
    public class SOBuoyantWaterWaveControllerConfig: SOConfigBase<BuoyantWaterWaveController.ConfigInfo> { }
}