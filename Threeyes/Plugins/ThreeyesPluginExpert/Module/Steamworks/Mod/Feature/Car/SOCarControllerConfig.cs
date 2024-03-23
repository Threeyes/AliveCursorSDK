using Threeyes.Config;
using UnityEngine;

namespace Threeyes.Steamworks
{
    [CreateAssetMenu(menuName = Steamworks_EditorDefinition.AssetMenuPrefix_Root_Feature + "CarController", fileName = "CarControllerConfig")]
    public class SOCarControllerConfig: SOConfigBase<CarController.ConfigInfo> { }
}