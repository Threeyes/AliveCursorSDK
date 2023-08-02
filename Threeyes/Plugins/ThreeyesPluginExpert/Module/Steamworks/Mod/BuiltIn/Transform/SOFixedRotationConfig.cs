using System.Collections;
using System.Collections.Generic;
using Threeyes.Config;
using UnityEngine;

namespace Threeyes.Steamworks
{
    [CreateAssetMenu(menuName = Steamworks_EditorDefinition.AssetMenuPrefix_Root_BuiltIn + "Transform/FixedRotation", fileName = "FixedRotationConfig")]
    public class SOFixedRotationConfig : SOConfigBase<FixedRotation.ConfigInfo> { }
}