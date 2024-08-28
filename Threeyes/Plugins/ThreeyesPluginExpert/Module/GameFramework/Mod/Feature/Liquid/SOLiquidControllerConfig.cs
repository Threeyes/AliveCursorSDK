using System.Collections;
using System.Collections.Generic;
using Threeyes.Config;
using UnityEngine;

namespace Threeyes.GameFramework
{
    [CreateAssetMenu(menuName = GameFramework_EditorDefinition.AssetMenuPrefix_Root_Feature + "LiquidController", fileName = "LiquidControllerConfig")]
    public class SOLiquidControllerConfig : SOConfigBase<LiquidController.ConfigInfo> { }
}