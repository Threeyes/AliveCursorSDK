using System;
using System.Collections;
using Threeyes.Coroutine;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 计时器
/// </summary>
public class TimeCounter : MonoBehaviour
{
    #region Property & Field

    [Header("OutputFormat")]
    //时间格式: https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-timespan-format-strings  https://docs.microsoft.com/en-us/dotnet/standard/base-types/formatting-types
    //输出格式：https://www.cnblogs.com/arxive/p/5744823.html
    //分:秒:毫秒 = mm\:ss\:f = 00:00:0
    //分:秒 = mm\:ss = 00:00
    public string strTimeFormat = @"mm\:ss";
    public StringEvent stringEvent;//当前的时间(文字格式)


    [Header("Event")]
    //如果只需要输出秒，且不需要占位符0，可以与TextHelper联合使用，format为{0:G}
    public FloatEvent curTimeEvent;//当前已用的时间（与当前是否为倒计时有关）
    public FloatEvent curTimePercentEvent; //当前已用的时间百分比
    public FloatEvent usedTimeEvent;//已用时间
    public FloatEvent usedTimePercentEvent; //已用的时间百分比
    public FloatEvent sumCountDownTimeEvent;//总时间
    public UnityEvent onCountDownFinish;//倒计时完成
    public BoolEvent onStartStopCount;//开始/结束倒计时//ToUse


    [Header("Common Config")]
    [SerializeField] protected float deltaUpdateTime = 0.5f;//更新时间的频率
    [SerializeField] protected bool isIgnoreTimeScale = false;//是否忽略缩放

    [Header("CountDown")]
    [SerializeField] protected bool isCountDownMode = false;//倒计时模式
    [SerializeField] protected long sumCountDownTime = 10;//总时长(Second)

    [Header("Editor")]
    [SerializeField] protected float curTime;//当前时间
    [SerializeField] protected float usedTime;//已用时间

    #region Property

    /// <summary>
    /// 计时开始时间
    /// </summary>
    public DateTime StartDateTime
    {
        get
        {
            return startDateTime;
        }

        set
        {
            startDateTime = value;
        }
    }

    /// <summary>
    /// 当前时间（如果是倒计时模式，数值会减少）
    /// </summary>
    public TimeSpan CurTimeSpan
    {
        get
        {
            TimeSpan timeSpan = UsedTimeSpan;
            if (IsCountDownMode)
            {
                timeSpan = SumTimeSpan > timeSpan ? SumTimeSpan - timeSpan : TimeSpan.Zero;
            }
            return timeSpan;
        }
        set
        {
            TimeSpan timeSpan = value;
        }
    }
    public string CurTimeFormat { get { return FormatTime(CurTimeSpan); } }
    public float CurTime { get { return (float)CurTimeSpan.TotalSeconds; } }

    /// <summary>
    /// 已用时间（origin）
    /// </summary>
    public TimeSpan UsedTimeSpan { get { return _usedTimeSpan; } }
    TimeSpan _usedTimeSpan = TimeSpan.Zero;//缓存已用的时间
    public string UsedTimeFormat { get { return FormatTime(UsedTimeSpan); } }
    public float UsedTime { get { return (float)UsedTimeSpan.TotalSeconds; } }
    public float SumCountDownTimeFloat { get { return SumCountDownTime; } set { SumCountDownTime = (long)value; } }
    public long SumCountDownTime { get { return sumCountDownTime; } set { sumCountDownTime = value; sumCountDownTimeEvent.Invoke(value); } }
    public bool IsCountDownMode { get { return isCountDownMode; } set { isCountDownMode = value; } }
    TimeSpan SumTimeSpan { get { TimeSpan timeSpan = TimeToTimeSpan(SumCountDownTime); return timeSpan; } }
    public string SumTimeFormat { get { return FormatTime(SumTimeSpan); } }

    public bool IsStartCount { get { return isStartCount; } set { isStartCount = value; } }
    public bool IsIgnoreTimeScale { get { return isIgnoreTimeScale; } set { isIgnoreTimeScale = value; } }
    public float DeltaUpdateTime { get { return deltaUpdateTime; } set { deltaUpdateTime = value; } }
    #region Obsolete

    [Obsolete("Use CurTimeFormat instead", true)]
    public string CurUsedTimeFormat { get { return CurTimeFormat; } }
    [Obsolete("Use CurTime instead", true)]
    public float CurUsedTime { get { return CurTime; } }


    #endregion

    #endregion

    #region Private

    Coroutine cacheEnum;//缓存计时协程
    bool isStartCount = false;//是否正在开始计时
    float startTime;//通过Time计算，与Timescale相关
    DateTime startDateTime = new DateTime();//通过DateTime计算，与Timescale无关

    #endregion

    #endregion

    public void StartCount(bool isOn)
    {
        if (isOn)
            StartCount();
        else
            StopCount();
    }

    [ContextMenu("StartCount")]
    public void StartCount()
    {
        TryStopCoroutine();
        cacheEnum = CoroutineManager.StartCoroutineEx(IEUpdateCount());
        IsStartCount = true;
    }

    public void ResetCount()
    {
        _usedTimeSpan = TimeSpan.Zero;
        InvokeEventFunc();
    }

    [ContextMenu("StopCount")]
    public void StopCount()
    {
        TryStopCoroutine();
        IsStartCount = false;
    }

    void TryStopCoroutine()
    {
        if (cacheEnum != null)
            CoroutineManager.StopCoroutineEx(cacheEnum);
    }

    //Todo
    //public bool isFadeOutOnStop = false;//当调用Stop后将时间逐渐归零
    //IEnumerator IEFadeOutStopCount()
    //{

    //}


    IEnumerator IEUpdateCount()
    {
        //以下两个都需要记录,因为后期计算可能需要使用
        StartDateTime = DateTime.Now;
        startTime = Time.time;
        _usedTimeSpan = TimeSpan.Zero;//初始化

        if (InvokeEventAndCheckCompletionCondition())
            yield break;

        while (true)
        {
            if (IsIgnoreTimeScale)//真实时间
            {
                DateTime dtCache = DateTime.Now;
                if (DeltaUpdateTime > 0)
                    yield return new WaitForSecondsRealtime(DeltaUpdateTime);//忽略TimeScale
                else
                    yield return null;

                _usedTimeSpan += DateTime.Now - dtCache;//累加
            }
            else//Unity时间
            {
                float curTimeCache = Time.time;
                if (DeltaUpdateTime > 0)
                    yield return new WaitForSeconds(DeltaUpdateTime);
                else
                    yield return null;

                _usedTimeSpan += TimeToTimeSpan(Time.time - curTimeCache);//累加
            }

            if (InvokeEventAndCheckCompletionCondition())
                yield break;
        }
    }


    /// <summary>
    /// 检查计时是否完成
    /// </summary>
    /// <returns>是否倒计时完成</returns>
    bool InvokeEventAndCheckCompletionCondition()
    {
        InvokeEventFunc();

        if (UsedTimeSpan >= SumTimeSpan)//结束计时
        {
            onCountDownFinish.Invoke();
            return true;
        }
        return false;
    }

    /// <summary>
    /// 调用事件
    /// </summary>
    private void InvokeEventFunc()
    {
        //用于编辑器中显示
        curTime = CurTime;
        usedTime = UsedTime;

        stringEvent.Invoke(CurTimeFormat);
        curTimeEvent.Invoke(CurTime);
        usedTimeEvent.Invoke(UsedTime);

        if (SumCountDownTime > 0)
        {
            curTimePercentEvent.Invoke(CurTime / SumCountDownTime);
            usedTimePercentEvent.Invoke(UsedTime / SumCountDownTime);
        }
        else
        {
            Debug.LogError("分母小于0！");
        }
    }

    #region Utility

    /// <summary>
    /// Convert from time to timespan
    /// </summary>
    /// <param name="time">Second</param>
    /// <returns></returns>
    TimeSpan TimeToTimeSpan(float time)
    {
        return TimeSpan.FromSeconds(time);
    }

    /// <summary>
    /// 格式化时间
    /// </summary>
    /// <param name="timeSpan"></param>
    /// <returns></returns>
    string FormatTime(TimeSpan timeSpan)
    {
        DateTime dateTime = new DateTime(timeSpan.Ticks);
        return dateTime.ToString(strTimeFormat);
    }

    string FormatTime(float time)
    {
        return FormatTime(TimeToTimeSpan(time));
    }

    #endregion

}
