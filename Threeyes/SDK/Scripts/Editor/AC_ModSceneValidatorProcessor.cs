#if UNITY_EDITOR
using UMod.BuildEngine;
using UMod.BuildPipeline;
using UMod.BuildPipeline.Build;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

using Threeyes.Persistent;
using Threeyes.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// 功能：检查场景设置是否正确
///
 
/// Ref:TanksModTools.ModSceneValidatorProcessor
/// </summary>
[UModBuildProcessor(".unity", -100)]
public class AC_ModSceneValidatorProcessor : BuildEngineProcessor
{
    public override void ProcessAsset(BuildContext context, BuildPipelineAsset asset)
    {
        if (!asset.FullPath.Contains(AC_SOWorkshopItemInfo.SceneName))//只处理与ItemScene名称相关
            return;

        // Load the scene into the editor
        Scene scene = EditorSceneManager.OpenScene(asset.FullPath);
        bool validScene = true;
        string errorInfo = null;
        //——AC——
        var arrayAC = scene.GetComponents<AC_AliveCursor>();
        if (arrayAC.Count() == 0 || arrayAC.Count() > 1)
        {
            validScene = false;
            errorInfo += $"-One and only one [{nameof(AC_AliveCursor)}] Component should exists in scene!\r\n";

        }

        //——ThreeyesPlugins——
        //PD:
        //1.检查重复的Key (PS: PDController会忽略无效的Key，而且PD会提醒，因此不需要检查）
        Dictionary<string, IPersistentData> dicKeyPD = new Dictionary<string, IPersistentData>();
        foreach (IPersistentData pd in scene.GetComponents<IPersistentData>(true))
        {
            if (!dicKeyPD.ContainsKey(pd.Key))
            {
                dicKeyPD[pd.Key] = pd;
            }
            else
            {
                validScene = false;
                IPersistentData pdCorrupt = dicKeyPD[pd.Key];

                errorInfo += $"-Same PD Key [{pd.Key}] in gameobjects:  {(pdCorrupt as Component)?.gameObject.name} & {(pd as Component)?.gameObject.name}!\r\n";
                break;
            }
        }

        // Check for valid scene
        if (validScene == false)
        {
            //ToAdd: 提醒对应的Mod场景名
            string sceneErrorHeader = $"Build mod scene [{EditorPathTool.AbsToUnityRelatePath(asset.FullPath)}] failed with error:\r\n";
            errorInfo = sceneErrorHeader + errorInfo;
            context.FailBuild(errorInfo);
        }
    }
}
#endif