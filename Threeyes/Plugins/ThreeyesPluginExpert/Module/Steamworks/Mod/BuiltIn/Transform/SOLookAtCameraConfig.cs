using System.Collections;
using System.Collections.Generic;
using Threeyes.Config;
using UnityEngine;

namespace Threeyes.Steamworks
{
    [CreateAssetMenu(menuName = Steamworks_EditorDefinition.AssetMenuPrefix_Root_BuiltIn + "Transform/LookAtCamera", fileName = "LookAtCameraConfig")]
    public class SOLookAtCameraConfig : SOConfigBase<LookAtCamera.ConfigInfo> { }
}