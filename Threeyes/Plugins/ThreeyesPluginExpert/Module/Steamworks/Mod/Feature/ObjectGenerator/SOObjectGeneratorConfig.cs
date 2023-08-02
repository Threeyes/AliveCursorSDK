using System.Collections;
using System.Collections.Generic;
using Threeyes.Config;
using UnityEngine;

namespace Threeyes.Steamworks
{
    [CreateAssetMenu(menuName = Steamworks_EditorDefinition.AssetMenuPrefix_Root_Feature + "ObjectGenerator", fileName = "ObjectGeneratorConfig")]
    public class SOObjectGeneratorConfig : SOConfigBase<ObjectGenerator.ConfigInfo> { }
}