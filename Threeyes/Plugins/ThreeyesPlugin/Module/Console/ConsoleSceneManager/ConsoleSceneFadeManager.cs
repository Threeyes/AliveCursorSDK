using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConsoleSceneFadeManager : MonoBehaviour
{
    #region Instance
    public static ConsoleSceneFadeManager Instance;
    bool isInit = false;

    /// <summary>
    /// （通过DontDestroyOnLoad调用）
    /// </summary>
    public virtual void SetInstance()
    {
        if (!isInit)
        {
            Instance = this as ConsoleSceneFadeManager;
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            isInit = true;
        }
    }
    #endregion

    public BoolEvent onFadeInOut;

    public static void Fade(bool isFadeIn)
    {
        if (Instance)
            Instance.FadeFunc(isFadeIn);
    }

    void FadeFunc(bool isFadeIn)
    {
        onFadeInOut.Invoke(isFadeIn);
    }

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        Fade(true);
    }
}
