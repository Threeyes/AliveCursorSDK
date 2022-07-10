using DG.Tweening;
using System;
using UnityEngine;

public class Clock_HourGlass : AC_Clock
{
    [Header("Custom")]
    public Transform tfSecondPivot;//Pivot of the second
    public AC_LiquidController liquidSecondTop;
    public AC_LiquidController liquidSecondBottom;

    public GameObject sandFallingHour;
    public GameObject sandFallingMinute;
    public GameObject particleSystemSecond;

    public float maxIncreaseDelay = 1;//液体延迟增加的最大时间

    Tween tweenSecondPivotRotate;


    protected override void OnEnable()
    {
        base.OnEnable();

        //统一隐藏
        sandFallingHour.SetActive(false);
        sandFallingMinute.SetActive(false);
    }

    protected override void SetTimeIncreaseFunc(TimeType timeType, float lastValue, float curValue, FloatEvent onTimeChanged, Func<float, float> getPercent, Tween tween, Ease ease, float tweenDuration = 1, float delay = 0)
    {
        switch (timeType)
        {
            //PS:粒子系统播放完成后会自己Disable，所以只激活就行
            case TimeType.Minute:
                TempShow(sandFallingMinute);
                break;

            case TimeType.Hour:
                TempShow(sandFallingHour);
                break;
        }

		//自动计算延迟
        if (timeType == TimeType.Hour)
        {
            delay = maxIncreaseDelay * (1 - GetHourPercent(curValue));
        }
        if (timeType == TimeType.Minute)
        {
            delay = maxIncreaseDelay * (1 - GetMinutePercent(curValue));
        }

        base.SetTimeIncreaseFunc(timeType, lastValue, curValue, onTimeChanged, getPercent, tween, ease, tweenDuration, delay);
    }

    void TempShow(GameObject go)
    {
        if (go.activeInHierarchy)//避免编辑器下调试时频繁调用
            go.SetActive(false);
        go.SetActive(true);
    }

    protected override void SetTimeRollbackfunc(TimeType timeType, float lastValue, float curValue, FloatEvent onTimeChanged, Func<float, float> getPercent, Tween tween, Ease ease, float tweenDuration = 1, float delay = 0)
	{
        if (timeType != TimeType.Second)
        {
            base.SetTimeRollbackfunc(timeType, lastValue, curValue, onTimeChanged, getPercent, tween, ease, tweenDuration, delay);
            return;
        }

        //PS:为了避免翻转时上瓶液体有残留下瓶未满，需要临时将目标值设置到越界点；
        liquidSecondTop.FillAmount = -0.2f;
        liquidSecondBottom.FillAmount = 1.2f;


        particleSystemSecond.SetActive(false);//临时隐藏粒子
        tweenSecondPivotRotate = tfSecondPivot.DOLocalRotate(new Vector3(-180, 0, 0), 1f, RotateMode.LocalAxisAdd);
        tweenSecondPivotRotate.onComplete +=
        () =>
        {
            //旋转完成后回归原点(注意！动画开始前沙漏的值是59，对应的起始值就是1)
            tfSecondPivot.localEulerAngles = Vector3.zero;//手动重置
            onSecondPercentChange.Invoke(0);
            particleSystemSecond.SetActive(true);
        };
        tweenSecondPivotRotate.SetEase(ease);
    }
}
