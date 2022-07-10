using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 检测当前运行平台
/// </summary>
public class RuntimeModeHelper : MonoBehaviour
{
    public UnityEvent onRuntimeMode;
    public UnityEvent onEditorMode;

    private void Awake()
    {
        if (Application.isEditor)
            onEditorMode.Invoke();
        else
            onRuntimeMode.Invoke();
    }
}
