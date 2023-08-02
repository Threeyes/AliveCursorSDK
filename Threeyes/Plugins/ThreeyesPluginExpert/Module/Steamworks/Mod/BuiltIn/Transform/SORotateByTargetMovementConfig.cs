using System.Collections;
using System.Collections.Generic;
using Threeyes.Config;
using UnityEngine;

namespace Threeyes.Steamworks
{
    [CreateAssetMenu(menuName = Steamworks_EditorDefinition.AssetMenuPrefix_Root_BuiltIn + "Transform/RotateByTargetMovement", fileName = "RotateByTargetMovementConfig")]
    public class SORotateByTargetMovementConfig : SOConfigBase<RotateByTargetMovement.ConfigInfo> { }
}