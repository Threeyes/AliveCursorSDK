using UnityEngine;
using Threeyes.Config;

namespace Threeyes.Steamworks
{
    [CreateAssetMenu(menuName = Steamworks_EditorDefinition.AssetMenuPrefix_Root_Feature + "Clock/ClockController", fileName = "ClockControllerConfig")]
    public class SOClockControllerConfig : SOConfigBase<ClockController.ConfigInfo> { }
}