using Threeyes.Config;
using UnityEngine;

namespace Threeyes.Steamworks
{
    [CreateAssetMenu(menuName = Steamworks_EditorDefinition.AssetMenuPrefix_Root_BuiltIn + "MediaController", fileName = "MediaControllerConfig")]
    public class SOMediaControllerConfig : SOConfigBase<MediaController.ConfigInfo> { }
}