using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
/// <summary>
/// 管理环境设置和场景设置
/// 
/// 注意：以下设置使用Scriptableobject进行保存，加载场景时不会自动重置，建议加载或退出时重置：
///         PostProcessing
///         TimeScale
///         ……
/// </summary>
public class EnvironmentManager : InstanceBase<EnvironmentManager>
{
    #region Environment Setting

    public void SetFog(bool isOn)
    {
        //PS:如果打包后没有雾特效， Edit-Project Settings- Graphics- Shader stripping-Fog modes-Custom。 参考https://stackoverflow.com/questions/23541146/no-fog-after-build
        RenderSettings.fog = isOn;
    }
    public void SetFogDensity(float density)
    {
        RenderSettings.fogDensity = density;
    }

    public void SetTimeScale(float value)
    {
        Time.timeScale = value;
    }

    /// <summary>
    /// 用于暂停音频播放（原理是控制AudioSettings.dspTime），常与timeScale联合使用
    /// </summary>
    /// <param name="isContinue"></param>
    public void PlayPauseAudioSystem(bool isContinue)
    {
        PlayPauseAudioSystemStatic(isContinue);
    }
    public static void PlayPauseAudioSystemStatic(bool isContinue)
    {
        AudioListener.pause = isContinue ? false : true;
    }

    public void SetVolume(float percent)
    {
        SetVolumeStatic(percent);
    }
    public static void SetVolumeStatic(float percent)
    {
        AudioListener.volume = percent;
    }
    public void SetSkyBox(Material material)
    {
        RenderSettings.skybox = material;
    }

    public void SetAmbientColorPercent(float percent)
    {
        Color maxColor = Color.white;
        RenderSettings.ambientLight = maxColor * percent;
    }

    #endregion

    #region Load&Quit Scene

    public BoolEvent onSceneLoaded;//适用于切换场景前，重置当前场景作出的全局修改，如TimeScale

    private void OnEnable()
    {
        onSceneLoaded.Invoke(true);
    }

    private void OnDestroy()
    {
        onSceneLoaded.Invoke(false);
    }

    /// <summary>
    /// 退出程序
    /// </summary>
    public void Quit()
    {
        QuitStatic();
    }

    /// <summary>
    /// 关机
    /// </summary>
    public void ShutDown()
    {
        if (Application.isEditor)
        {
            QuitStatic();
            Debug.Log("Simulate System ShutDown");
        }
        else
        {
#if !UNITY_EDITOR&& UNITY_STANDALONE
            CMDUtility.RunCmd("shutdown", "-s -f -t 00");
#endif
        }
    }


    public static void QuitStatic()
    {
        if (Application.isEditor)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
        else
            Application.Quit();
    }

    public void LoadScene(SOSceneInfo sceneInfo)
    {
        if (!sceneInfo)
        {
            Debug.LogError("空引用！");
            return;
        }
        sceneInfo.LoadScene();
    }

    public void LoadSceneAdditive(SOSceneInfo sceneInfo)
    {
        sceneInfo.LoadSceneAdditive();
    }

    [ContextMenu("ReloadScene")]
    public void ReloadScene()
    {
        ReloadSceneStatic();
    }

    public static void ReloadSceneStatic()
    {
        LoadScene(SceneManager.GetActiveScene().name);
    }

    public static void LoadScene(string sceneName)
    {
        CleanUpMemory();

        if (Application.isMobilePlatform)
            SceneManager.LoadSceneAsync(sceneName);
        else
            SceneManager.LoadScene(sceneName);

    }
    public static void LoadSceneAdditive(string sceneName, LoadSceneMode sceneMode)
    {
        CleanUpMemory();
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }

    public static void LoadSceneAsync(string sceneName, LoadSceneMode sceneMode = LoadSceneMode.Single, UnityAction<float> actOnLoadingProgress = null, UnityAction actOnLoadCompleted = null, float delayLoadNextScene = -1f, bool isWaitUntilFullyLoaded = false)
    {
        Instance.StartCoroutine(IELoadSceneAsync(sceneName, sceneMode, actOnLoadingProgress, actOnLoadCompleted, delayLoadNextScene, isWaitUntilFullyLoaded));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="sceneMode"></param>
    /// <param name="actOnLoadingProgress"></param>
    /// <param name="actOnLoadCompleted"></param>
    /// <param name="delayLoadNextScene"></param>
    /// <param name="isWaitUntilFullyLoaded">等待场景完全加载后才载入(bug:)</param>
    /// <returns></returns>
    static IEnumerator IELoadSceneAsync(string sceneName, LoadSceneMode sceneMode = LoadSceneMode.Single, UnityAction<float> actOnLoadingProgress = null, UnityAction actOnLoadCompleted = null, float delayLoadNextScene = -1f, bool isWaitUntilFullyLoaded = false)
    {
        float leapProgress = 0;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, sceneMode);

        if (isWaitUntilFullyLoaded)
        {
            //Bug:Unity2018版中，allowSceneActivation会阻挡加载，https://issuetracker.unity3d.com/issues/loadsceneasync-allowsceneactivation-flag-is-ignored-in-awake
            asyncLoad.allowSceneActivation = false;//防止下一场景自动加载
        }

        // Wait until the asynchronous scene fully loads
        //Bug:
        //1.Unity最多只能加载到0.9f;
        while (!asyncLoad.isDone)
        {
            float realProgress = asyncLoad.progress < 0.9f ? asyncLoad.progress : 1;

            leapProgress = Mathf.SmoothStep(leapProgress, realProgress, 0.5f);
            if (actOnLoadingProgress != null)
                actOnLoadingProgress.Invoke(leapProgress);
            yield return null;
        }

        Debug.Log("Completed!" + asyncLoad.progress);
        yield return new WaitForEndOfFrame();
        if (actOnLoadCompleted != null)
            actOnLoadCompleted.Invoke();

        //延后加载下一个场景，适用于增加过渡效果
        if (delayLoadNextScene > 0)
            yield return new WaitForSeconds(delayLoadNextScene);

        if (isWaitUntilFullyLoaded)
        {
            asyncLoad.allowSceneActivation = true;
        }

        CleanUpMemory();
    }

    /// <summary>
    /// 清理内存
    /// </summary>
    public static void CleanUpMemory()
    {
#if UNITY_STANDALONE||UNITY_EDITOR
        Resources.UnloadUnusedAssets();//卸载不用的资源（容易导致Android黑屏！）
        //System.GC.Collect();//注意！！！可能会导致程序闪退并报“Fatal GC……”错误，通常是因为360等软件引起
#endif
    }

    private void Update()
    {
        //重新加载当前场景
        if (Input.GetKeyDown(KeyCode.F5))
            ReloadScene();
    }

    #endregion
}
