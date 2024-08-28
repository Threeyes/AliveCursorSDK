using System.Collections;
using System.Collections.Generic;
using Threeyes.Config;
using UnityEngine;

namespace Threeyes.GameFramework
{
    [CreateAssetMenu(menuName = GameFramework_EditorDefinition.AssetMenuPrefix_Root_Feature + "Whiteboard", fileName = "WhiteboardConfig")]
    public class SOWhiteboardConfig : SOConfigBase<Whiteboard.ConfigInfo> { }
}