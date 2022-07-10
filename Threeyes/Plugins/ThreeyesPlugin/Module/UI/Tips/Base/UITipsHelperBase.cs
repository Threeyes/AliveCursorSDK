using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UITipsHelperBase<T, TElement> : ComponentHelperBase<T>
    where T : UITipsBase<TElement>
    where TElement : SOTipsInfo
{
    public virtual TElement TipsInfo
    {
        get { return tipsInfo; }
        set
        {
            tipsInfo = value;
            onSetTips.Invoke();
        }
    }
    public UnityEvent onSetTips;

    [SerializeField] protected TElement tipsInfo;
    public FloatEvent onPlayAudioFinish;


    public void ShowAndPlay()
    {
        ShowAndPlay(true);
    }
    public virtual void ShowAndPlay(bool isShow)
    {
        Show(isShow);
        if (isShow)
            SetTips(TipsInfo);//相当于用缓存的数据刷新，同时能调用音频
    }

    /// <summary>
    /// 运行时创建并修改TipsInfo
    /// </summary>
    public void CreateAndModifyTipsInfo(UnityAction<TElement> action)
    {
        TipsInfo = ScriptableObject.CreateInstance<TElement>();
        action.Invoke(TipsInfo);
    }

    public virtual void SetTipsAndShow(TElement tipsInfo)
    {
        Show();
        SetTips(tipsInfo);
    }


    public virtual void SetTips()
    {
        SetTips(TipsInfo);
    }

    public virtual void SetTips(TElement tipsInfo)
    {
        Comp.SetTips(tipsInfo);
        Comp.actionOnPlayAudioFinish += (f) => onPlayAudioFinish.Invoke(f);
    }



    public virtual void Show()
    {
        Show(true);
    }

    public virtual void Show(bool isShow)
    {
        Comp.Show(isShow);
    }

    public virtual void Hide()
    {
        Comp.Hide();
    }

}
