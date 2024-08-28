using System.Collections;
using System.Collections.Generic;
using Threeyes.Config;
using UnityEngine;

namespace Threeyes.GameFramework
{
    [CreateAssetMenu(menuName = GameFramework_EditorDefinition.AssetMenuPrefix_Root_Feature + "Creeper/AudioVisualizer", fileName = "CreeperAudioVisualizerConfig")]
    public class SOCreeperAudioVisualizerConfig : SOConfigBase<CreeperAudioVisualizer.ConfigInfo> { }
}