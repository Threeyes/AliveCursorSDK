using System.Collections;
using System.Collections.Generic;
using Threeyes.Config;
using UnityEngine;

namespace Threeyes.Steamworks
{
    [CreateAssetMenu(menuName = Steamworks_EditorDefinition.AssetMenuPrefix_Root_Feature + "Creeper/TransformController", fileName = "CreeperTransformControllerConfig")]
    public class SOCreeperTransformControllerConfig : SOConfigBase<CreeperTransformController.ConfigInfo> { }
}