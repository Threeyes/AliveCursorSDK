using System.Collections;
using System.Collections.Generic;
using Threeyes.Config;
using UnityEngine;

namespace Threeyes.GameFramework
{
    [CreateAssetMenu(menuName = GameFramework_EditorDefinition.AssetMenuPrefix_Root_BuiltIn + "Transform/TiltByTargetMovement", fileName = "TiltByTargetMovementConfig")]
    public class SOTiltByTargetMovementConfig : SOConfigBase<TiltByTargetMovement.ConfigInfo> { }
}