using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 强制设置应用程序的分辨率，常用于4K屏导致应用程序重置的情况。可以在设置完成后再加载指定场景，这样避免初始化错误
/// </summary>
public class DisplayResolutionManager : SingletonBase<DisplayResolutionManager>
{
    public Vector2 desireResolution = new Vector2(1920, 1080);
    FullScreenMode fullScreenMode = FullScreenMode.FullScreenWindow;

    public UnityEvent onInitCompleted;

    private void Awake()
    {
        if (Application.isEditor)
            return;

        StartCoroutine(IEInitWindow());
    }

    IEnumerator IEInitWindow()
    {
        //如果此时显示器没打开，那么默认分辨率为1024*768，因此要等待其打开，从而电脑能够识别显示器的正确分辨率
        while (Screen.currentResolution.width < desireResolution.x || Screen.currentResolution.height < desireResolution.y)
        {
            yield return null;
        }

        //Warning:频繁以下这段方法会导致闪屏，建议切屏时不使用
        if (Screen.currentResolution.width != desireResolution.x || Screen.currentResolution.height != desireResolution.y)
        {
            //解决分辨率 小->大 后，窗口分辨率仍保持小的问题（Bug:会造成窗口闪烁，改为重新加载应用程序）
            //Unity Set FullScreen Version
            Screen.fullScreen = false;
            yield return new WaitForEndOfFrame();//等待窗口尺寸更新

            //设置为全屏（传入的分辨率值不重要）
            Screen.SetResolution((int)desireResolution.x, (int)desireResolution.y, FullScreenMode.FullScreenWindow);

            Debug.Log(string.Format("Current Resolution is:{0}*{1}, setting to target resolution{2}*{3}", Screen.currentResolution.width, Screen.currentResolution.height, desireResolution.x, desireResolution.y));

            //等待完成
            while (Screen.width != desireResolution.x)
            {
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForEndOfFrame();
            onInitCompleted.Invoke();
        }
    }
}
