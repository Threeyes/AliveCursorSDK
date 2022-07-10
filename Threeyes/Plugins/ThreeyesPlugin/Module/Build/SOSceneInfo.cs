using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
#endif

/// <summary>
/// 场景信息
/// </summary>
[CreateAssetMenu(menuName = EditorDefinition.AssetMenuPrefix_SO_Build + "SceneInfo")]
public class SOSceneInfo : ScriptableObject
{
    public static UnityAction<SOSceneInfo> sceneLoaded;//加载新场景时调用
    public SceneInfoType sceneInfoType = SceneInfoType.Normal;

    public string displayName;//显示的名字
    public string buildName;//Build的名字(Scene.name)

    public Texture2D icon;//项目管理软件上的显示图标(512×512,png格式)，可空

#if UNITY_EDITOR

    public SceneAsset sceneAsset;

    public string scenePath { get { return GetScenePath(this); } }

    [ContextMenu("UseCurScene")]
    void UseCurScene()
    {
        Scene sceneCur = SceneManager.GetActiveScene();

        sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(sceneCur.path);
        buildName = sceneCur.name;
    }

    [ContextMenu("UpdateBuildName")]
    void UpdateBuildName()
    {
        if (sceneAsset)
            buildName = sceneAsset.name;
    }

    /// <summary>
    /// 在编辑器中打开该场景
    /// </summary>
    [ContextMenu("OpenSceneInEditor")]
    public void OpenSceneInEditor()
    {
        if (scenePath.NotNullOrEmpty())
        {
            EditorSceneManager.OpenScene(scenePath);
        }
        else
        {
            Debug.LogError("未找到对应的场景:" + scenePath);
        }
    }

    /// <summary>
    /// 返回BuildSetting中的对应场景路径
    /// </summary>
    /// <param name="sOSceneInfo"></param>
    /// <returns></returns>
    public static string GetScenePath(SOSceneInfo sOSceneInfo)
    {
        if (sOSceneInfo)
        {
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.path.Contains(sOSceneInfo.buildName))
                {
                    return scene.path;
                }
            }
        }
        return "";
    }

#endif

    /// <summary>
    /// 场景是否已经加载
    /// </summary>
    public bool IsSceneActive
    {
        get
        {
            return SceneManager.GetActiveScene().name == buildName;
        }
    }
    //Todo:增加一些参数，可以传到EnvironmentManager，用于初始化
    public void LoadScene()
    {
        EnvironmentManager.LoadScene(buildName);
        sceneLoaded.Execute(this);
    }

    public void LoadSceneAdditive(LoadSceneMode loadSceneMode = LoadSceneMode.Additive)
    {
        EnvironmentManager.LoadSceneAdditive(buildName, loadSceneMode);
        sceneLoaded.Execute(this);
    }

    public void LoadSceneAsync(LoadSceneMode loadSceneMode = LoadSceneMode.Additive, UnityAction<float> actOnLoadingProgress = null, UnityAction actOnLoadCompleted = null, float delayLoadNextScene = -1f, bool isWaitUntilFullyLoaded = false)
    {
        EnvironmentManager.LoadSceneAsync(buildName, loadSceneMode, actOnLoadingProgress, actOnLoadCompleted, delayLoadNextScene, isWaitUntilFullyLoaded);
        sceneLoaded.Execute(this);
    }


    public enum SceneInfoType
    {
        Normal,//加载场景
        Reload,//重载
        Quit//退出
    }
}
