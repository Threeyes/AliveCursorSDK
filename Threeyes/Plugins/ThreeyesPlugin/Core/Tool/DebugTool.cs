using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 常用的调试信息，运行后可用
/// </summary>
public static class DebugTool
{
    /// <summary>
    /// Editor or DebugBuild
    /// </summary>
    /// <value></value>
    public static bool IsEditorOrDebugBuild => Application.isEditor || !Application.isEditor && IsDebugBuild;

    public static bool IsRuntimeDebugBuild => !Application.isEditor && IsDebugBuild;

    /// <summary>
    /// In the Build Settings dialog there is a check box called "Development Build".
    /// </summary>
    public static bool IsDebugBuild => Debug.isDebugBuild;

    /// <summary>
    /// 检查当前是否为Debug状态（满足条件：编辑器+isDebug或
    /// </summary>
    /// <param name="isDebug"></param>
    /// <returns></returns>
    public static bool GetDebugMode(bool isDebug)
    {
        //编辑器模式/打包后的DebugBuild模式
        return (Application.isEditor || !Application.isEditor && IsDebugBuild) && isDebug;
    }
}
