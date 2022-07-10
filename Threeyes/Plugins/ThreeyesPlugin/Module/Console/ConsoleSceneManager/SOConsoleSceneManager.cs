using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SOConsoleSceneManager : SOInstacneBase<SOConsoleSceneManager>
{
    #region Instance

    static string defaultName = "ConsoleSceneManager";
    private static SOConsoleSceneManager _instance;
    public static SOConsoleSceneManager Instance
    {
        get
        {
            return GetOrCreateInstance(ref _instance, defaultName);
        }
    }

    #endregion

    public SOConsoleSceneConfig CurConsoleSceneConfig
    {
        get
        {
            if (curConsoleSceneConfig)
                return curConsoleSceneConfig;
            else
            {
                Debug.LogError("未设置SOConsoleSceneConfig！");
                return null;
            }
        }

        set
        {
            curConsoleSceneConfig = value;
        }
    }

    public SOConsoleSceneConfig curConsoleSceneConfig;
}
