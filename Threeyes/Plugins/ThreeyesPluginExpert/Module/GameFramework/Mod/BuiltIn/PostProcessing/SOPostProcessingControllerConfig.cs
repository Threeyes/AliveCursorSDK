using System.Collections;
using System.Collections.Generic;
using Threeyes.Config;
using UnityEngine;

namespace Threeyes.GameFramework
{
    [CreateAssetMenu(menuName = GameFramework_EditorDefinition.AssetMenuPrefix_Root_BuiltIn + "PostProcessing/PostProcessinglController", fileName = "PostProcessinglControllerConfig")]
    public class SOPostProcessingControllerConfig : SOConfigBase<PostProcessingController.ConfigInfo> { }
}