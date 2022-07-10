using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 管理所有CanvasGroupHelper，实现类似ToggleGroup的功能
/// </summary>
public class CanvasGroupGroupHelper : ComponentHelperGroupBase<CanvasGroupHelper, CanvasGroup>
{
    public UnityEvent onChangeCanvasGroup;

    public CanvasGroupHelper curCanvasGroupHelper;//当前激活的组
    public bool isEnableRepeatInvoke = false;//是否允许重复调用

    /// <summary>
    /// 当前激活的Helper（因为调试时curCanvasGroupHelper可能为空，用这个比较方便）
    /// </summary>
    public List<CanvasGroupHelper> ListActiveHelper
    {
        get
        {
            List<CanvasGroupHelper> listHelper = new List<CanvasGroupHelper>();
            ForEachChildComponent((c) =>
            {
                if (c.isShowing)
                    listHelper.Add(c);
            });
            return listHelper;
        }
    }

    //private void Reset()
    //{
    //    isRecursive = false;    
    //}

    public void SetInteractable(bool isActive)
    {
        ForEachChildComponent((c) =>
        {
            c.Comp.interactable = isActive;
        });
    }

    /// <summary>
    /// 显示指定的CanvasGroup
    /// </summary>
    /// <param name="canvasGroupHelper"></param>
    public void ShowCanvasGroup(CanvasGroupHelper canvasGroupHelper)
    {
        if (!isEnableRepeatInvoke && curCanvasGroupHelper == canvasGroupHelper)
            return;
        ForEachChildComponent((c) =>
        {
            bool isMatch = (c == canvasGroupHelper);

            if (Application.isPlaying)
            {
                c.BeginShowHide(isMatch);
            }
            else
            {
                c.ShowAtOnce(isMatch);
            }
        });

        curCanvasGroupHelper = canvasGroupHelper;
        onChangeCanvasGroup.Invoke();
    }

}