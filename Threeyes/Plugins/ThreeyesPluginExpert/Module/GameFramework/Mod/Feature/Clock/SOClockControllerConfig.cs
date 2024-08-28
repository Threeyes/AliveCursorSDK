using UnityEngine;
using Threeyes.Config;

namespace Threeyes.GameFramework
{
    [CreateAssetMenu(menuName = GameFramework_EditorDefinition.AssetMenuPrefix_Root_Feature + "Clock/ClockController", fileName = "ClockControllerConfig")]
    public class SOClockControllerConfig : SOConfigBase<ClockController.ConfigInfo> { }
}