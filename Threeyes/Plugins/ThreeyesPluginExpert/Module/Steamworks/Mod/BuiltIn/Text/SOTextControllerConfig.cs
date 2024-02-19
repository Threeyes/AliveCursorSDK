using Threeyes.Config;
using UnityEngine;

namespace Threeyes.Steamworks
{
    [CreateAssetMenu(menuName = Steamworks_EditorDefinition.AssetMenuPrefix_Root_BuiltIn + "TextController", fileName = "TextControllerConfig")]
    public class SOTextControllerConfig : SOConfigBase<TextController.ConfigInfo> { }
}