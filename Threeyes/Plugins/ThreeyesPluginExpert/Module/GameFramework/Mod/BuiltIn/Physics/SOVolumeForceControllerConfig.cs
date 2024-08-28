using System.Collections;
using System.Collections.Generic;
using Threeyes.Config;
using UnityEngine;

namespace Threeyes.GameFramework
{
    [CreateAssetMenu(menuName = GameFramework_EditorDefinition.AssetMenuPrefix_Root_BuiltIn + "Physics/VolumeForceController", fileName = "VolumeForceControllerConfig")]
    public class SOVolumeForceControllerConfig : SOConfigBase<VolumeForceController.ConfigInfo> { }
}