using Threeyes.Config;
using UnityEngine;

namespace Threeyes.GameFramework
{
    [CreateAssetMenu(menuName = GameFramework_EditorDefinition.AssetMenuPrefix_Root_BuiltIn + "Light/LightController", fileName = "LightControllerConfig")]
    public class SOLightControllerConfig : SOConfigBase<LightController.ConfigInfo> { }
}