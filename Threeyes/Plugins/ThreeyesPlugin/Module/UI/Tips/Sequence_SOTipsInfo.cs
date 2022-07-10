using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Threeyes.Coroutine;

/// <summary>
/// 设置一系列相同的SoTipsInfo，通过UnityEvent调用对应的UITips
/// 常用于一系列的PPT翻页教学（ToUpdate：抽取出类似UISplitPageBase的基类，名称为UIPPTBase)
/// </summary>
public class Sequence_SOTipsInfo : SequenceBase<SOTipsInfo>
{
    public bool isInitOnStart = true;//是否使用已有数据生成元素  （设置为false可保留已存在的元素）
    public SOTipsInfoEvent onSetTipsInfo;
    public SOTipsInfoGroup dataGroup;

    private void Start()
    {
        //临时方案
        if (isInitOnStart && dataGroup)
        {
            ListData = dataGroup.ListData;
        }
    }


    protected override void SetDataFunc(SOTipsInfo data, int index)
    {
        base.SetDataFunc(data, index);
        onSetTipsInfo.Invoke(data);
    }

    public float autoSetInterval = 0.3f;
    /// <summary>
    /// 根据音频的长度，自动按顺序播放
    /// </summary>
    /// <param name="isBegin"></param>
    public void BeginAutoSet(bool isBegin)
    {
        TryStopCoroutine();
        if (isBegin)
        {
            cacheEnum = CoroutineManager.StartCoroutineEx(IEAutoSet());
        }
    }
    protected Coroutine cacheEnum;
    protected virtual void TryStopCoroutine()
    {
        if (cacheEnum != null)
            CoroutineManager.StopCoroutineEx(cacheEnum);
    }

    IEnumerator IEAutoSet()
    {
        for (int i = 0; i != ListData.Count; i++)
        {
            SOTipsInfo tipsInfo = ListData[i];
            Set(i);
            float timeToWait = tipsInfo.AudioClipLength + autoSetInterval;
            yield return new WaitForSeconds(timeToWait);
        }
        onComplete.Invoke();
    }

}
