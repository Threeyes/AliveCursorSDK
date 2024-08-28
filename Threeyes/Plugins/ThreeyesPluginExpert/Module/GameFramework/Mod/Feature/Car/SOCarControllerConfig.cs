using Threeyes.Config;
using UnityEngine;

namespace Threeyes.GameFramework
{
    [CreateAssetMenu(menuName = GameFramework_EditorDefinition.AssetMenuPrefix_Root_Feature + "CarController", fileName = "CarControllerConfig")]
    public class SOCarControllerConfig: SOConfigBase<CarController.ConfigInfo> { }
}