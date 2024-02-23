using System;
using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;
using Threeyes.Core;

namespace Threeyes.Steamworks
{
    /// <summary>
    /// Show clock info
    /// 
    /// ToAdd：
    /// +ConfigInfo增加时间Offset(总位移秒数)，方便用户在场景摆放多个时钟，并对应不同时区
    /// +支持运行时编辑
    /// -支持12/24小时制切换（对应HourFormat枚举）
    /// </summary>
    public class ClockController : ConfigurableComponentBase<ClockController, SOClockControllerConfig, ClockController.ConfigInfo, ClockController.PropertyBag>
    {
        //Use these event to display integer time
        public IntEvent onHourChange;
        public IntEvent onMinuteChange;
        public IntEvent onSecondChange;

        //Time progress (仅考虑其单位，不考虑小一级的单位）（因为是Tweeen完成之后更新值，所以不能与下面的Precise事件整合）
        [Header("Progress")]
        public FloatEvent onHourPercentChange;
        public FloatEvent onMinutePercentChange;
        public FloatEvent onSecondPercentChange;

        [Header("Precise progress")]        //Time precise progress (考虑本级加小一级的单位，常用于齿轮时钟等与子单位相关联的显示)
        public bool usePreciseProgress = false;//持续更新精确值
        [ShowIf(nameof(usePreciseProgress))] public FloatEvent onPreciseHourPercentChange;
        [ShowIf(nameof(usePreciseProgress))] public FloatEvent onPreciseMinutePercentChange;

        [Header("Run Time")]
        [Range(0, 11)]
        public int lastHour;
        [Range(0, 11)]
        public int curHour;

        [Range(0, 59)]
        public int lastMinute;
        [Range(0, 59)]
        public int curMinute;

        [Range(0, 59)]
        public int lastSecond;
        [Range(0, 59)]
        public int curSecond;


        [Header("Debug")]
        public bool isDebugManualUpdate = false;
        public bool TestSecondRollback { get => testSecondRollback; set => testSecondRollback = value; }//Use property for UnityEvent
        [SerializeField] private bool testSecondRollback = false;//Set to true to rollback second once (Via EP)

        DateTime CurDateTime
        {
            get
            {
                DateTime dateTimeNow = DateTime.Now;
                if (Config.offsetSeconds != 0)//如果时间位移不为零就进行计算
                {
                    dateTimeNow = new DateTime(dateTimeNow.Ticks + TimeSpan.TicksPerSecond * Config.offsetSeconds);
                }
                return dateTimeNow;
            }
        }
        bool hasInit = false;
        protected virtual void OnEnable()
        {
            //Refresh whenever it gets actived
            DateTime dateTime = CurDateTime;
            curHour = lastHour = dateTime.Hour;
            curMinute = lastMinute = dateTime.Minute;
            curSecond = lastSecond = dateTime.Second;
            onHourChange.Invoke(lastHour);
            onMinuteChange.Invoke(lastMinute);
            onSecondChange.Invoke(lastSecond);
            onHourPercentChange.Invoke(GetHourPercent(lastHour));
            onMinutePercentChange.Invoke(GetMinutePercent(lastMinute));
            onSecondPercentChange.Invoke(GetSecondPercent(lastSecond));
            hasInit = true;
        }
        protected virtual void OnDisable()
        {
            hasInit = false;
        }


        Tween tweenHour;
        Tween tweenMinute;
        Tween tweenSecond;

        bool isAutoUpdate
        {
            get
            {
#if UNITY_EDITOR
                return !isDebugManualUpdate;
#else
            return true;
#endif
            }
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (TestSecondRollback)
            {
                if (curSecond == 59)
                    DebugRollBack();
                else
                {
                    curSecond = 59;
                    Invoke("DebugRollBack", 1.2f);
                }
                TestSecondRollback = false;
            }
            if (isDebugManualUpdate)
            {
                if (lastMinute != curMinute)
                {
                    onMinuteChange.Invoke(curMinute);
                    SetTimeTween(TimeType.Minute, lastMinute, curMinute, onMinutePercentChange, GetMinutePercent, tweenMinute);
                    lastMinute = curMinute;
                }
                if (lastHour != curHour)
                {
                    onHourChange.Invoke(curHour);
                    SetTimeTween(TimeType.Hour, lastHour, curHour, onHourPercentChange, GetHourPercent, tweenHour);
                    lastHour = curHour;
                }

                if (usePreciseProgress)
                {
                    onPreciseHourPercentChange.Invoke(GetPreciseHourPercent(curHour, curMinute, curSecond));
                    onPreciseMinutePercentChange.Invoke(GetPreciseMinutePercent(curMinute, curSecond));
                }
            }
#endif

            if (!hasInit)
                return;

            DateTime dateTime = CurDateTime;

            if (isAutoUpdate)
                curSecond = dateTime.Second;

            //PS:只有下级更新后才检查上级，可以减少更新频率（缺点：如果用户手动更新Hour，可能需要重新激活物体，或者等待Minute更新后，Hour才能随后更新。但该情况很少出现）
            if (lastSecond != curSecond)
            {
                onSecondChange.Invoke(curSecond);
                SetTimeTween(TimeType.Second, lastSecond, curSecond, onSecondPercentChange, GetSecondPercent, tweenSecond);//ps:这里传入的Action会针对Tween的Second进行变换，不需要millisecond
                lastSecond = curSecond;

                if (isAutoUpdate)
                    curMinute = dateTime.Minute;
                if (lastMinute != curMinute)
                {
                    onMinuteChange.Invoke(curMinute);
                    SetTimeTween(TimeType.Minute, lastMinute, curMinute, onMinutePercentChange, GetMinutePercent, tweenMinute);
                    lastMinute = curMinute;

                    if (isAutoUpdate)
                        curHour = dateTime.Hour;
                    if (lastHour != curHour)
                    {
                        onHourChange.Invoke(curHour);
                        SetTimeTween(TimeType.Hour, lastHour, curHour, onHourPercentChange, GetHourPercent, tweenHour);
                        lastHour = curHour;
                    }
                }

                if (usePreciseProgress)
                {
                    onPreciseHourPercentChange.Invoke(GetPreciseHourPercent(curHour, curMinute, curSecond));
                    onPreciseMinutePercentChange.Invoke(GetPreciseMinutePercent(curMinute, curSecond));
                }

            }
        }

        protected virtual void SetTimeTween(TimeType timeType, float lastValue, float curValue, FloatEvent onTimePercentChanged, Func<float, float> getPercent, Tween tween)
        {
            if (curValue != 0)//Increase
            {
                SetTimeIncreaseFunc(timeType, lastValue, curValue, onTimePercentChanged, getPercent, tween, Config.increaseTweenEaseType, Config.increaseTweenDuration);
            }
            else//Rollback: From 59(s/m) or 11(h) back to 0
            {
                float targetValue = 0;
                if (!Config.rollbackToZero)
                    targetValue = lastValue + 1;//直接叠加。适用于时钟等直接进入到下一值

                SetTimeRollbackfunc(timeType, lastValue, targetValue, onTimePercentChanged, getPercent, tween, Config.rollbackTweenEaseType, Config.rollbackTweenDuration);
            }
        }

        protected virtual void SetTimeIncreaseFunc(TimeType timeType, float lastValue, float curValue, FloatEvent onTimePercentChanged, Func<float, float> getPercent, Tween tween, Ease ease, float tweenDuration = 1, float delay = 0)
        {
            SetTimeTweenFunc(timeType, lastValue, curValue, onTimePercentChanged, getPercent, tween, ease, tweenDuration, delay);
        }
        protected virtual void SetTimeRollbackfunc(TimeType timeType, float lastValue, float curValue, FloatEvent onTimePercentChanged, Func<float, float> getPercent, Tween tween, Ease ease, float tweenDuration = 1, float delay = 0)
        {
            SetTimeTweenFunc(timeType, lastValue, curValue, onTimePercentChanged, getPercent, tween, ease, tweenDuration, delay);
        }

        #region IModHandler

        public override void UpdateSetting()
        {
            //都是实时更新，暂不需要做操作
        }
        #endregion

        #region Debug

        [ContextMenu("UpdateTimeAtOnce")]
        public void UpdateTimeAtOnce()
        {
            onHourPercentChange.Invoke(GetHourPercent(CurDateTime.Hour));
            onMinutePercentChange.Invoke(GetMinutePercent(CurDateTime.Minute));
            onSecondPercentChange.Invoke(GetSecondPercent(CurDateTime.Second));
            if (usePreciseProgress)
            {
                onPreciseHourPercentChange.Invoke(GetPreciseHourPercent(CurDateTime.Hour, CurDateTime.Minute, CurDateTime.Second));
                onPreciseMinutePercentChange.Invoke(GetPreciseMinutePercent(CurDateTime.Minute, CurDateTime.Second));
            }
        }

        [ContextMenu("AddOneSecond")]
        void DebugAddOneSecond()
        {
            curSecond = (curSecond + 1) % 60;
            if (curSecond == 0)
            {
                curMinute = (curMinute + 1) % 60;
                if (curMinute == 0)
                    curHour = (curHour + 1) % 12;
            }
        }

        void DebugRollBack()
        {
            curSecond = 0;
        }
        #endregion

        #region Utility
        protected virtual void SetTimeTweenFunc(TimeType timeType, float lastValue, float curValue, FloatEvent onTimePercentChanged, Func<float, float> getPercent, Tween tween, Ease ease, float tweenDuration = 1, float delay = 0)
        {
            float tempValue = lastValue;
            tween = DOTween.To(
                () => tempValue,
                    (v) =>
                    {
                        tempValue = v;
                        onTimePercentChanged.Invoke(getPercent.Invoke(tempValue));
                    },
                   curValue, tweenDuration);
            tween.SetEase(ease).SetDelay(delay);
        }

        /// <summary>
        /// 返回12小时制的小数hour
        /// </summary>
        /// <returns></returns>
        protected virtual float GetPreciseHourPercent(float hour, float minute, float second)
        {
            return GetHourPercent(hour + GetMinutePercent(minute + GetSecondPercent(second)));
        }
        protected virtual float GetPreciseMinutePercent(float minute, float second)
        {
            return GetMinutePercent(minute + GetSecondPercent(second));
        }


        /// <summary>
        /// 返回12小时制的整数hour
        /// </summary>
        /// <param name="hour"></param>
        /// <returns></returns>
        protected virtual float GetHourPercent(float hour)
        {
            return Mathf.Repeat(hour, 12) / 12;//Turn into 12-hour clock
        }
        protected virtual float GetMinutePercent(float minute)
        {
            return minute / 60;
        }
        protected virtual float GetSecondPercent(float second)
        {
            return second / 60;
        }
        #endregion

        #region Define
        [Serializable]
        public class ConfigInfo : SerializableComponentConfigInfoBase
        {
            [Range(0, 1)] public float increaseTweenDuration = 1;
            public Ease increaseTweenEaseType = Ease.OutElastic;//Ease type when Time increase
            [Range(0, 1)] public float rollbackTweenDuration = 1;
            public Ease rollbackTweenEaseType = Ease.OutCubic;//Ease type when Time rollback to zero
            public bool rollbackToZero = true;//True:[Max-1]=>[0]; False:[Max-1]=>[Max]

            public int offsetSeconds;//TimeZone offset
        }
        public class PropertyBag : ConfigurableComponentPropertyBagBase<ClockController, ConfigInfo> { }

        public enum TimeType
        {
            Hour,
            Minute,
            Second
        }
        #endregion
    }
}