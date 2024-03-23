using System.Collections;
using System.Collections.Generic;
using Threeyes.Config;
using UnityEngine;

namespace Threeyes.Steamworks
{
    [CreateAssetMenu(menuName = Steamworks_EditorDefinition.AssetMenuPrefix_Root_BuiltIn + "Rendering/MaterialController", fileName = "MaterialControllerConfig")]
    public class SOMaterialControllerConfig : SOConfigBase<MaterialController.ConfigInfo> { }
}