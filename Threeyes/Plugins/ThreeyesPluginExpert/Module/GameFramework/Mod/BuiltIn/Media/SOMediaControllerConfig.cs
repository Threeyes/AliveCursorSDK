using Threeyes.Config;
using UnityEngine;

namespace Threeyes.GameFramework
{
    [CreateAssetMenu(menuName = GameFramework_EditorDefinition.AssetMenuPrefix_Root_BuiltIn + "MediaController", fileName = "MediaControllerConfig")]
    public class SOMediaControllerConfig : SOConfigBase<MediaController.ConfigInfo> { }
}