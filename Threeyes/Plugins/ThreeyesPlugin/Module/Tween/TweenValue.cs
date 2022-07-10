#if USE_DOTween
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
/// <summary>
/// Tween特定值
/// Todo：改为TweenValue<泛型>
/// </summary>
public class TweenValue : MonoBehaviour
{
    public float TargetValue { get { return targetValue; } set { targetValue = value; } }

    public float CurValue { get { return curValue; } set { curValue = value; } }

    public Slider.SliderEvent onChangeValue;
    public UnityEvent onFinish;//动画完成
    [Header("设置通用参数")]
    public bool isPlayOnAwake = false;//物体首次初始化时开始动画
    public bool isPlayOnEnable = false;//物体每次激活时自动开始动画
    public bool isKillOnDisable = true;//物体每次隐藏时自动结束动画
    public bool isUseStartValue = true;//使用开始值，可以防止重新开始动画时当前值与targetValue相近导致过渡不明显的问题
    public bool isResetOnEnable = false;//物体每次激活时重置
    public bool isResetOnKill = false;//KillTween时重置初始值，常用于Start Tween(bool)
    public bool isAutoKill = false;
    public bool isReUseTween = false;//重用Tween
    public bool isIgnoreTimeScale = false;

    public float startValue = 0.2f;
    public float targetValue = 0.8f;
    public float delay = -1;//动画的延迟值
    public Random_Float duration = new Random_Float(1f, 2f);
    [Tooltip("动画的过渡方式")]
    public EaseType easeType = EaseType.EaseEnum;
    public Ease ease = Ease.InOutSine;
    public AnimationCurve animationCurve = AnimationCurve.Constant(0, 1, 1);

    [Header("RunTime Property")]
    public bool isTweening = false;
    public float curValue;
    protected Tweener tweener;


    #region Public Func


    public void StartTween(bool isOn)
    {
        if (isOn)
            StartTween();
        else
            KillTween();
    }

    [ContextMenu("StartTween")]
    public void StartTween()
    {
        TryKillTween();

        tweener = SetUpTween(InitTweener());
        isTweening = true;
    }
    [ContextMenu("KillTween")]
    public void KillTween()
    {
        TryKillTween();
        isTweening = false;
    }


    public void DoForward(bool isForward)
    {
        var tempTweener = tweener;

        if (tempTweener == null)//创建
        {
            tempTweener = InitTweener();
        }
        tweener = SetUpTween(tempTweener);

        if (isForward)
        {
            tweener.Rewind(false);
            tweener.PlayForward();
        }
        else
        {
            tweener.Complete(false);
            tweener.PlayBackwards();
        }
    }

    public void SmoothRewind()
    {
        if (tweener != null)
            tweener.SmoothRewind();
    }

    [ContextMenu("SetToStartValue")]
    public void SetToStartValue()
    {
        KillTween();
        SetValue(startValue);
    }
    [ContextMenu("SetToTargetValue")]
    public void SetToTargetValue()
    {
        KillTween();
        SetValue(TargetValue);
    }
    #endregion

    private void Awake()
    {
        if (isPlayOnAwake)
            StartTween();

    }
    void OnEnable()
    {
        if (isResetOnEnable)
            SetValue(startValue);

        if (isPlayOnEnable)
            StartTween();
    }
    private void OnDisable()
    {
        if (isKillOnDisable)
            KillTween();
    }

    void TryKillTween()
    {
        if (tweener != null)
        {
            tweener.Kill();
            tweener = null;//加这行，否则DoForward动画可能在[IsKillOnDisable]勾选且物体禁用后不能继续用
        }
        if (isResetOnKill)
            SetValue(startValue);
    }

    /// <summary>
    /// 设置动画参数
    /// </summary>
    /// <returns></returns>
    protected virtual Tweener SetUpTween(Tweener sourceTweener)
    {
        if (isUseStartValue)
        {
            SetValue(startValue);
            //PS:不用Tweener.ChangeStartValue，而是直接修改值，是为了方便外部调用，即使Tweener已经销毁也能使用
            //tweener.ChangeStartValue(startValue);
        }

        if (!isAutoKill)
            sourceTweener.SetAutoKill(false);

        switch (easeType)
        {
            case EaseType.EaseEnum:
                sourceTweener.SetEase(ease); break;
            case EaseType.AnimationCurve:
                sourceTweener.SetEase(animationCurve); break;
        }

        if (delay > 0)
            sourceTweener.SetDelay(delay);

        if (isIgnoreTimeScale)
            sourceTweener.SetUpdate(true);

        //Reset
        sourceTweener.onComplete = null;//清空以前的监听，避免重复监听
        sourceTweener.onComplete += () => onFinish.Invoke();

        return sourceTweener;
    }

    /// <summary>
    /// 初始化动画
    /// </summary>
    /// <returns></returns>
    protected virtual Tweener InitTweener()
    {
        return DOTween.To(
           () => CurValue,
           SetValue,
           TargetValue,
           duration.ResultValue
           );
    }

    /// <summary>
    ///  设置当前需要变换的参数值
    /// </summary>
    /// <param name="value"></param>
    protected virtual void SetValue(float value)
    {
        onChangeValue.Invoke(value);
        CurValue = value;
    }

    /// <summary>
    /// 更改Target值,并且以当前值为基础，重新开始动画
    /// </summary>
    public void SetTargetAndRestartTween(float targetValue)
    {
        if (tweener.NotNull())
        {
            TargetValue = targetValue;
            tweener.ChangeEndValue(TargetValue, true).Restart();//参考：DOTween Examples/Follow.unity
        }
        else
        {
            StartTween(true);
        }
    }

    /// <summary>
    /// 动画的过渡方式
    /// </summary>
    public enum EaseType
    {
        EaseEnum,//使用Ease的各种枚举
        AnimationCurve//使用曲线
    }
}
#endif