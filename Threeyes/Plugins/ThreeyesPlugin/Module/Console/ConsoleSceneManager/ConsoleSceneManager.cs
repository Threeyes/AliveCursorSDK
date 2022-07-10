using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsoleSceneManager : MonoBehaviour
{
    #region Instance
    public static ConsoleSceneManager Instance;
    bool isInit = false;
    public virtual void SetInstance()
    {
        if (!isInit)
        {
            Instance = this as ConsoleSceneManager;
            isInit = true;
        }
    }
    #endregion

}
