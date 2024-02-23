using System.Collections;
using System.Collections.Generic;
using Threeyes.Config;
using Threeyes.Core;
using UnityEngine;
namespace Threeyes.SpawnPoint
{

    [CreateAssetMenu(menuName = EditorDefinition_Threeyes.AssetMenuPrefix_Root_Module + "SpawnPointProvider", fileName = "SpawnPointProviderConfig")]
    public class SOSpawnPointProviderConfig : SOConfigBase<SpawnPointProvider.ConfigInfo> { }
}