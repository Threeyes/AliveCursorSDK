using System;
using UnityEngine;
using DG.Tweening;
using Threeyes.Config;

namespace Threeyes.Steamworks
{
    /// <summary>
    /// Show clock info
    /// </summary>
    public class ClockController : ConfigurableComponentBase<SOClockControllerConfig, ClockController.ConfigInfo>
    {
        //Use these event to display number
        public IntEvent onHourChange;
        public IntEvent onMinuteChange;
        public IntEvent onSecondChange;

        //Use these event to display progress
        public FloatEvent onHourPercentChange;
        public FloatEvent onMinutePercentChange;
        public FloatEvent onSecondPercentChange;


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

        DateTime curDateTime { get { return DateTime.Now; } }
        bool hasInit = false;
        protected virtual void OnEnable()
        {
            //Refresh whenever it gets actived
            DateTime dateTime = curDateTime;
            lastHour = dateTime.Hour;
            lastMinute = dateTime.Minute;
            lastSecond = dateTime.Second;
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
            }
#endif

            if (!hasInit)
                return;

            DateTime dateTime = curDateTime;
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
            }
        }

        protected virtual void SetTimeTween(TimeType timeType, float lastValue, float curValue, FloatEvent onTimePercentChanged, Func<float, float> getPercent, Tween tween)
        {
            if (curValue != 0)
            {
                SetTimeIncreaseFunc(timeType, lastValue, curValue, onTimePercentChanged, getPercent, tween, Config.increaseTweenEaseType, Config.increaseTweenDuration);
            }
            else//From 59(s/m) or 11(h) back to 0
            {
                SetTimeRollbackfunc(timeType, lastValue, curValue, onTimePercentChanged, getPercent, tween, Config.rollbackTweenEaseType, Config.rollbackTweenDuration);
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

        #region Debug

        [ContextMenu("UpdateTimeAtOnce")]
        public void UpdateTimeAtOnce()
        {
            onHourPercentChange.Invoke(GetHourPercent(curDateTime.Hour));
            onMinutePercentChange.Invoke(GetMinutePercent(curDateTime.Minute));
            onSecondPercentChange.Invoke(GetSecondPercent(curDateTime.Second));
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
        public class ConfigInfo : SerializableDataBase
        {
            [Range(0, 1)] public float increaseTweenDuration = 1;
            public Ease increaseTweenEaseType = Ease.OutElastic;//Ease type when Time increase
            [Range(0, 1)] public float rollbackTweenDuration = 1;
            public Ease rollbackTweenEaseType = Ease.OutCubic;//Ease type when Time rollback to zero
        }
        public enum TimeType
        {
            Hour,
            Minute,
            Second
        }
        #endregion
    }
}