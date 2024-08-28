#if USE_UnityHair
using UnityEngine;
using Threeyes.Config;

namespace Threeyes.GameFramework
{
    [CreateAssetMenu(menuName = GameFramework_EditorDefinition.AssetMenuPrefix_Root_Feature + "Hair/HairInstanceController", fileName = "HairInstanceControllerConfig")]
    public class SOHairInstanceControllerConfig : SOConfigBase<HairInstanceController.ConfigInfo> { }

}
#endif