using System.Collections;
using System.Collections.Generic;
using Threeyes.Config;
using UnityEngine;

namespace Threeyes.GameFramework
{
    [CreateAssetMenu(menuName = GameFramework_EditorDefinition.AssetMenuPrefix_Root_BuiltIn + "Transform/ObjectMovement/FollowTarget", fileName = "ObjectMovement_FollowTargetConfig")]
    public class SOObjectMovement_FollowTargetConfig : SOConfigBase<ObjectMovement_FollowTarget.ConfigInfo> { }
}