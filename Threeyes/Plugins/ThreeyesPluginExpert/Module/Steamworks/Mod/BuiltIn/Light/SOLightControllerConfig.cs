using Threeyes.Config;
using UnityEngine;

namespace Threeyes.Steamworks
{
    [CreateAssetMenu(menuName = Steamworks_EditorDefinition.AssetMenuPrefix_Root_BuiltIn + "LightController", fileName = "LightControllerConfig")]
    public class SOLightControllerConfig : SOConfigBase<LightController.ConfigInfo> { }
}