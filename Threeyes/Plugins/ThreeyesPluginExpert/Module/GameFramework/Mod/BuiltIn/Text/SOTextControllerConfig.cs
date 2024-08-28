using Threeyes.Config;
using UnityEngine;

namespace Threeyes.GameFramework
{
    [CreateAssetMenu(menuName = GameFramework_EditorDefinition.AssetMenuPrefix_Root_BuiltIn + "TextController", fileName = "TextControllerConfig")]
    public class SOTextControllerConfig : SOConfigBase<TextController.ConfigInfo> { }
}