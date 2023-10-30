using System.Collections;
using System.Collections.Generic;
using Threeyes.Config;
using UnityEngine;
namespace Threeyes.SpawnPoint
{

    [CreateAssetMenu(menuName = Threeyes_EditorDefinition.AssetMenuPrefix_Root_Module + "SpawnPointProvider", fileName = "SpawnPointProviderConfig")]
    public class SOSpawnPointProviderConfig : SOConfigBase<SpawnPointProvider.ConfigInfo> { }
}