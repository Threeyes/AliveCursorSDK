using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 通过Process管理并开关其他程序
/// </summary>
public class ProcessManager : InstanceBase<ProcessManager>
{
    public UnityAction<ProcessInfo, bool> actProcessState;//进程的 启动/终止 状态
    /// <summary>
    /// 当前开启的进程
    /// </summary>
    public ProcessInfo curProcessInfo;
    public bool IsProcessRunning
    {
        get
        {
            if (curProcessInfo.NotNull() && curProcessInfo.process.NotNull())
                return true;
            return false;
        }
    }

    public bool OpenApp(ProcessInfo processInfo, bool isKillPreProcess = true)
    {
        try
        {
            if (isKillPreProcess)
                TryKillCurProcess();

            curProcessInfo = processInfo;

            //processInfo.process = new Process();
            //string param = "";
            //修改打开质量的参数 https://docs.unity3d.com/Manual/CommandLineArguments.html
            //param = "-screen-quality ";
            //param += qualityLevel.ToString();
            //param += "Ultra";

            //QualitySettings.SetQualityLevel
            //print(qualityLevel.ToString());
            //new ProcessStartInfo(processInfo.appPath, "-screen-quality Beautiful") ==== QualityLevel.Beautiful
            //ProcessStartInfo processStartInfo = new ProcessStartInfo(processInfo.appPath, param);

            ProcessStartInfo processStartInfo = processInfo.processStartInfo;
            processInfo.process = Process.Start(processStartInfo);
            SetProcessStateFunc(processInfo, true);

            return true;
        }
        catch (System.Exception e)
        {
            processInfo.errMsg = "进程打开失败：\r\n" + e;
            SetProcessStateFunc(processInfo, false);
            UnityEngine.Debug.LogError(processInfo.errMsg);
            return false;
        }
    }

    public void TryKillCurProcess()
    {
        TryKillProcess(curProcessInfo);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="onKillCurProcess">关闭激活的进程</param>
    /// <param name="onBeforeKillApplication">允许关闭程序</param>
    public void TryQuit(UnityAction onKillCurProcess, UnityAction onBeforeKillApplication)
    {
        if (Instance.IsProcessRunning)//优先退出当前进程
        {
            Instance.TryKillCurProcess();
            onKillCurProcess.Execute();
        }
        else
        {
            //只有在聚焦时，才会弹出退出窗口
            if (Application.isFocused)
            {
                onBeforeKillApplication.Execute();
            }
        }
    }

    public void TryKillProcess(ProcessInfo processInfo)
    {
        if (processInfo.IsNull())
            return;

        try
        {
            if (processInfo.process != null)
            {
                if (!processInfo.process.HasExited)//还未推出
                {
                    SetProcessStateFunc(processInfo, false);
                    processInfo.process.Kill();
                }
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError(e);
        }
        finally
        {
            processInfo.process = null;
        }
    }

    private void Update()
    {
        if (curProcessInfo.IsNull())
            return;

        //PS:因为线程的原因，不能监听process.Exited事件，因此需要自己监听
        //检查App的退出事件（自行关闭、意外退出）
        if (curProcessInfo.isOpening)
        {
            if (curProcessInfo.process == null || curProcessInfo.process.HasExited)
            {
                SetProcessStateFunc(curProcessInfo, false);
            }
        }
    }

    private void OnApplicationQuit()
    {
        TryKillCurProcess();
    }

    #region Utility

    /// <summary>
    /// 
    /// </summary>
    /// <param name="processInfo"></param>
    /// <param name="isOpening">开启或结束</param>
    void SetProcessStateFunc(ProcessInfo processInfo, bool isOpening)
    {
        processInfo.SetState(isOpening);
        actProcessState.Execute(processInfo, isOpening);
    }

    #endregion


}

[System.Serializable]
public class ProcessInfo
{
    public ProcessStartInfo processStartInfo;

    //Config
    public string appPath;
    public UnityAction<bool> eventOpenApp;

    public Process process;//Runtime
    public bool isOpening = false;
    public string errMsg;

    //Data

    public ProcessInfo(string appPath, UnityAction<bool> eventOpenApp)
    {
        this.appPath = appPath;
        this.eventOpenApp = eventOpenApp;
        processStartInfo = new ProcessStartInfo(appPath);
    }

    public void SetState(bool isOpening)
    {
        this.isOpening = isOpening;
        eventOpenApp.Execute(isOpening);
    }

}

