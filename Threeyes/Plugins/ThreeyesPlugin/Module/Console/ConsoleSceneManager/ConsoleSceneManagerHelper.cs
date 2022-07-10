using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// 设置控制台场景
/// </summary>
public class ConsoleSceneManagerHelper : MonoBehaviour
{
    readonly string errMsg = "没有场景配置文件！";

    #region OutDate

    public void SetAndLoadConsoleSceneConfig(SOConsoleSceneConfig sOConsoleSceneConfig)
    {
        SetConsoleSceneConfig(sOConsoleSceneConfig);
        LoadCurConsoleScene();
    }

    /// <summary>
    /// 设置当前的ConsoleScene信息
    /// </summary>
    /// <param name="sOConsoleSceneConfig"></param>
    public void SetConsoleSceneConfig(SOConsoleSceneConfig sOConsoleSceneConfig)
    {
        if (sOConsoleSceneConfig)
            SOConsoleSceneManager.Instance.CurConsoleSceneConfig = sOConsoleSceneConfig;
        else
            Debug.LogError(errMsg);
    }

    #endregion

    /// <summary>
    /// 设置需要自动加载的场景
    /// </summary>
    /// <param name="sOSceneInfo"></param>
    public void SetSceneToAutoLoad(SOSceneInfo sOSceneInfo)
    {
        if (SOConsoleSceneManager.Instance.CurConsoleSceneConfig)
            SOConsoleSceneManager.Instance.CurConsoleSceneConfig.sceneInfoToAutoLoad = sOSceneInfo;
        else
            Debug.LogError(errMsg);
    }

    /// <summary>
    /// 加载当前的ConsoleScene
    /// </summary>
    public void LoadCurConsoleScene()
    {
        if (SOConsoleSceneManager.Instance.CurConsoleSceneConfig)
        {
            var consoleSceneInfo = SOConsoleSceneManager.Instance.CurConsoleSceneConfig.consoleSceneInfo;
            if (consoleSceneInfo && !consoleSceneInfo.IsSceneActive)
                SOConsoleSceneManager.Instance.CurConsoleSceneConfig.consoleSceneInfo.LoadScene();
        }
        else
            Debug.LogError(errMsg);
    }

    /// <summary>
    /// 自动加载对应的场景
    /// </summary>
    public void LoadNextScene()
    {

        var consoleSceneConfig = SOConsoleSceneManager.Instance.CurConsoleSceneConfig;
        if (consoleSceneConfig)
        {
            Scene curLoadedScene = SceneManager.GetActiveScene();

            //排除场景信息
            if (curLoadedScene.name == consoleSceneConfig.consoleSceneInfo.buildName)
            {
                Debug.LogError("当前已经是控制台场景");
                return;
            }

            //获取下一场景信息
            SOSceneInfo curSOSceneInfo = consoleSceneConfig.sceneInfoLastLoaded;
            SOSceneInfo sOSceneInfoNext = null;
            switch (consoleSceneConfig.playbackOrderType)
            {
                case SOConsoleSceneConfig.PlaybackOrderType.Order:
                    sOSceneInfoNext = consoleSceneConfig.listSceneInfoToDisplay.GetNext(consoleSceneConfig.sceneInfoLastLoaded);
                    consoleSceneConfig.sceneInfoToAutoLoad = sOSceneInfoNext;
                    break;
                case SOConsoleSceneConfig.PlaybackOrderType.Repeat:
                    sOSceneInfoNext = curSOSceneInfo;
                    consoleSceneConfig.sceneInfoToAutoLoad = curSOSceneInfo;
                    break;
            }

            if (consoleSceneConfig.isReturnToConsoleOnFinish)
            {
                ConsoleSceneFadeManager.Fade(false);
                Invoke("LoadCurConsoleScene", 1.5f);
                //LoadCurConsoleScene();
            }
            else
            {
                if (sOSceneInfoNext)
                {
                    if (consoleSceneConfig.isAutoLoad)
                    {
                        ConsoleSceneFadeManager.Fade(false);
                        sceneInfoToLoad = sOSceneInfoNext;
                        Invoke("BeginLoadScene", 1.5f);
                    }
                    else
                    {
                        Debug.Log("当前参数isAutoLoad为false，不会自动加载！");
                    }
                }
                else
                {
                    Debug.LogError("下个场景为空！");
                }
            }
        }
        else
            Debug.LogError(errMsg);
    }

    SOSceneInfo sceneInfoToLoad;
    void BeginLoadScene()
    {
        sceneInfoToLoad.LoadScene();
    }

}
