using System.Collections;
using System.Collections.Generic;
using Threeyes.Config;
using UnityEngine;

namespace Threeyes.Steamworks
{
    [CreateAssetMenu(menuName = Steamworks_EditorDefinition.AssetMenuPrefix_Root_BuiltIn + "PostProcessing/PostProcessinglController", fileName = "PostProcessinglControllerConfig")]
    public class SOPostProcessingControllerConfig : SOConfigBase<PostProcessingController.ConfigInfo> { }
}