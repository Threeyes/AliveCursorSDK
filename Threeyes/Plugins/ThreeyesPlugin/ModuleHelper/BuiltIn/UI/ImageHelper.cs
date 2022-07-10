using System;
using System.Collections;
using System.Collections.Generic;
using Threeyes.Coroutine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ImageHelper : ComponentHelperBase<Image>
{
    public Color targetColor;

    public void SetColor()
    {
        Comp.color = targetColor;
    }

    public void SetSprite(Sprite sprite)
    {
        Comp.sprite = sprite;
    }

    #region Filled Type Animation

    public string demical;

    public float sumTime = 3;//总时间
    public float remainTime;//剩余时间
    public float updateFrequency = 0.02f;//更新计时器频率 比如0.02s更新一次当前时间
    float startTime;//计时使用
    TimeSpan usedTimeSpan;//计时使用
    public bool isIgnoreTimeScale;//是否忽略TimeScale
    public bool isCountDown;//是否倒计时
    //public bool isShowInDelayTime;
    private bool isStartCount;//是否开始计时
    private bool isPause;//是否暂停

    private Coroutine cacheEnum;//缓存计时协程

    public UnityEvent onCountDownFinished;//完成计时后调用
    public FloatEvent onUpdateCountDownTime;//计时器每次更新时调用

    public float RemainTime
    {
        get
        {
            return remainTime;
        }

        set
        {
            remainTime = value;
            onUpdateCountDownTime.Invoke(value);
            //InvokeEventAtSomeTime(10);
            Text text = this.GetComponentInChildren<Text>();
            if (text)
                text.text = remainTime.ToString(demical);
        }
    }
    private void Start()
    {
        //if(!isShowInDelayTime)
        //RemainTime = sumTime;
    }
    /// <summary>
    /// 获取当前Image旋转动画的值百分比
    /// </summary>
    /// <param name="curTime"></param>
    public void GetCurFillAmount()
    {
        Comp.fillAmount = (RemainTime / sumTime);
    }

    public void CountTime(bool isOn)
    {
        if (isOn)
            StartCount();
        else
            StopCount();
    }

    private void StartCount()
    {
        TryStopCoroutine();
        cacheEnum = CoroutineManager.StartCoroutineEx(IEUpdateTime(updateFrequency));
        isStartCount = true;
    }

    private void StopCount()
    {
        TryStopCoroutine();
        isStartCount = false;
        ResetRemainTime(sumTime);
        ResetFilledAmount(1f);
    }

    private void TryStopCoroutine()
    {
        if (cacheEnum != null)
            CoroutineManager.StopCoroutineEx(cacheEnum);
    }

    public void ResetRemainTime(float time)
    {
        RemainTime = time;
    }

    public void ResetFilledAmount(float curFill)
    {
        Comp.fillAmount = curFill;
    }


    IEnumerator IEUpdateTime(float updateFrequency)
    {
        //以下两个都需要记录,因为后期计算可能需要使用
        startTime = Time.time;
        usedTimeSpan = TimeSpan.Zero;//初始化

        if (InvokeEvent())
            yield break;

        while (true)
        {
            if (isIgnoreTimeScale)//真实时间
            {
                DateTime dtCache = DateTime.Now;
                yield return new WaitForSecondsRealtime(updateFrequency);//忽略TimeScale

                //RemainTime -= updateFrequency;   //PS:使用携程不能理想的认为等待0.02s  那么总时间-0.02s就是剩余时间， 携程有局限性 所以下面用的是系统真实时间。
                usedTimeSpan = DateTime.Now - dtCache;
                RemainTime -= (float)usedTimeSpan.TotalSeconds;
            }
            else
            {
                if (isPause)
                    yield return null;
                else
                {
                    float curTimeCache = Time.time;
                    yield return new WaitForSeconds(updateFrequency);//受TimeScale影响

                    //RemainTime -= updateFrequency;
                    usedTimeSpan = TimeToTimeSpan(Time.time - curTimeCache);//累加  
                    RemainTime -= (float)usedTimeSpan.TotalSeconds;
                }
            }

            if (InvokeEvent())
                yield break;
        }
    }

    /// <summary>
    /// Convert from time to timespan
    /// </summary>
    /// <param name="time">Second</param>
    /// <returns></returns>
    TimeSpan TimeToTimeSpan(float time)
    {
        return TimeSpan.FromSeconds(time);
    }

    public void PauseCount(bool isOn)
    {
        isPause = isOn;
    }

    private bool InvokeEvent()
    {
        if (RemainTime < 0)
        {
            onCountDownFinished.Invoke();
            return true;
        }
        return false;
    }
    //public UnityEvent onCountDonwInSomeTime;
    //public void InvokeEventAtSomeTime(float someTime)
    //{
    //    if (RemainTime < someTime)
    //    {
    //        onCountDonwInSomeTime.Invoke();
    //    }
    //}
    #endregion
}
