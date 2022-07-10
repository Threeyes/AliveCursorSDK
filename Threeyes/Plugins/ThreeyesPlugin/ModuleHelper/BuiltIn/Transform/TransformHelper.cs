using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if USE_DOTween
using DG.Tweening;
#endif
/// <summary>
/// 设置Transform的属性
/// 
/// 注意：
/// ——ChangeXXXAtOnce方法会停掉当前的动画并立即设置为终点值，类似于DoComplete
/// </summary>
public class TransformHelper : GameObjectBase
{
    public Transform Target { get { return target ? target : tfThis; } set { target = value; } }

    public Vector3 TargetPosition
    {
        get
        {
            return targetPosition;
        }

        set
        {
            targetPosition = value;
        }
    }

    public Vector3 TargetRotation
    {
        get
        {
            return targetRotation;
        }

        set
        {
            targetRotation = value;
        }
    }

    public Vector3 TargetScale
    {
        get
        {
            return targetScale;
        }

        set
        {
            targetScale = value;
        }
    }

    public Transform target;//需要执行变换操作的物体
    public UnityEvent onMoveFinish;
    public UnityEvent onRotateFinish;
    public UnityEvent onScaleFinish;

    [Header("Config")]
    public TweenFrom tweenFrom = TweenFrom.CurValue;
    public TweenTo tweenWith = TweenTo.TargetValue;
    public bool isLocalScale = true;
    public bool isLocalPosition = false;
#if USE_DOTween
    public Ease ease = Ease.OutExpo;
    public int loops = 0;//循环次数
    public LoopType loopType = LoopType.Yoyo;
#endif
    public float duration = 1;//默认的动画时长

    [Header("Values，用于下拉菜单快捷设置")]
    public Vector3 startPosition;
    public Vector3 targetPosition;//指定位置
    [Space]
    public Vector3 startRotation;
    public Vector3 targetRotation;//要旋转的指定角度
    [Space]
    public Vector3 startScale;
    public Vector3 targetScale = new Vector3(1, 1, 1);//最终的缩放值

    [Header("TargetPoint")]
    public Transform moveEndPoint;//需要移动到的目标
    public Transform rotateEndPoint;//需要旋转到的目标

    #region Menu

    [ContextMenu("SetToStartPosition")]
    public void SetToStartPosition()
    {
        Target.localPosition = startPosition;
    }
    [ContextMenu("SetToStartRotation")]
    public void SetToStartRotation()
    {
        Target.localEulerAngles = startRotation;
    }

    [ContextMenu("RecordStartPosition")]
    void RecordStartPosition()
    {
        startPosition = Target.localPosition;
    }
    [ContextMenu("RecordStartRotation")]
    void RecordStartRotation()
    {
        startRotation = Target.localEulerAngles;
    }

    [ContextMenu("RecordTargetPosition")]
    void RecordTargetPosition()
    {
        TargetPosition = Target.localPosition;
    }
    [ContextMenu("RecordTargetRotation")]
    void RecordTargetRotation()
    {
        TargetRotation = Target.localEulerAngles;
    }

    #endregion


#if USE_DOTween
    Tweener tweenerPos;
    Tweener tweenerRot;
    Tweener tweenerScale;
#endif

    public void ChangePositionAndRotationAtOnce()
    {
        ChangePositionAtOnce();
        ChangeRotationAtOnce();
    }

    [ContextMenu("ChangePositionAndRotation")]
    public void ChangePositionAndRotation()
    {
        ChangePositionAndRotation(duration);
    }

    public void ChangePositionAndRotation(float tweenDuration)
    {
        ChangePosition(tweenDuration);
        ChangeRotation(tweenDuration);
    }

    #region Position
    public void DoForwardPosition(bool isForward)
    {
#if USE_DOTween
        if (tweenerPos.NotNull())
            tweenerPos.Kill();

        if (isForward)
        {
            SetToStartPosition();
            ChangePosition();
        }
        else
        {
            //ToAdd
            //ChangePositionAtOnce();

        }
#endif
    }
    public void ChangePosition(Vector3 pos)
    {
        Target.position = pos;
    }

    [ContextMenu("ChangePosition")]
    public void ChangePosition()
    {
        ChangePosition(duration);
    }

    public void ChangePositon(bool isForward)
    {
        switch (tweenFrom)
        {
            case TweenFrom.StartValue:
                Target.localPosition = startPosition;
                break;
        }

#if USE_DOTween
        tweenerPos = Target.DOLocalMove(isForward ? targetPosition : startPosition, duration);
        tweenerPos.SetEase(ease).onComplete += () => onMoveFinish.Invoke();
#endif
    }

    [ContextMenu("ChangePositionAtOnce")]
    public void ChangePositionAtOnce()
    {
#if USE_DOTween
        if (tweenerPos.NotNull())
            tweenerPos.Complete();
#endif

        if (tweenWith == TweenTo.TargetPoint)
            Target.position = moveEndPoint.position;
        else
            Target.localPosition = TargetPosition;
    }

    public void ChangePosition(float tweenDuration)
    {
        if (tweenDuration <= 0)
        {
            ChangePositionAtOnce();
            onMoveFinish.Invoke();
        }
        else
        {
#if USE_DOTween
            switch (tweenFrom)
            {
                case TweenFrom.StartValue:
                    Target.localPosition = startPosition;
                    break;
            }
            tweenerPos = tweenWith == TweenTo.TargetPoint ? Target.DOMove(moveEndPoint.position, tweenDuration) : Target.DOLocalMove(TargetPosition, tweenDuration);
            tweenerPos.SetEase(ease).onComplete += () => onMoveFinish.Invoke();
            tweenerPos = SetUpTween(tweenerPos);
#endif
        }
    }

    #endregion

    #region Rotation

    [ContextMenu("ChangeRotationAtOnce")]
    public void ChangeRotationAtOnce()
    {
#if USE_DOTween
        if (tweenerRot.NotNull())
            tweenerRot.Complete();
#endif

        if (tweenWith == TweenTo.TargetPoint)
            Target.eulerAngles = rotateEndPoint.eulerAngles;
        else
            Target.localEulerAngles = TargetRotation;
    }

    public void ChangeRotationPercent(float percent)
    {
        Vector3 startRot = tweenFrom == TweenFrom.CurValue ? transform.localPosition : startPosition;
        Target.localEulerAngles = Vector3.Lerp(startRotation, TargetRotation, percent);
    }

    [ContextMenu("ChangeRotation")]
    public void ChangeRotation()
    {
        ChangeRotation(duration);
    }

    public void ChangeRotation(float tweenDuration)
    {
        if (!Target)
            Target = transform;

        if (tweenDuration <= 0)
        {
            ChangeRotationAtOnce();
            onRotateFinish.Invoke();
        }
        else
        {
#if USE_DOTween
            tweenerRot = tweenWith == TweenTo.TargetPoint ? Target.DORotate(rotateEndPoint.eulerAngles, duration) : Target.DOLocalRotate(TargetRotation, duration);
            tweenerRot.SetEase(ease).onComplete += () => onRotateFinish.Invoke();
#endif
        }
    }

    #endregion

    #region Scale

    public void ChangeScale(bool isForward)
    {
        ChangeScale(duration, isForward);
    }

    [ContextMenu("ChangeScale")]
    public void ChangeScale()
    {
        ChangeScale(duration);
    }

    public void ChangeUniformScale(float value)
    {
        Target.localScale = Vector3.one * value;
    }

    public void ChangeScale(float tweenDuration, bool isForward = true)
    {
        if (tweenDuration <= 0)
        {
            ChangeScaleAtOnce();
            onRotateFinish.Invoke();
        }
        else
        {
#if USE_DOTween
            switch (tweenFrom)
            {
                case TweenFrom.StartValue:
                    Target.localScale = isForward ? startScale : TargetScale;
                    break;
            }

            tweenerScale = Target.DOScale(isForward ? TargetScale : startScale, duration);
            tweenerScale.SetEase(ease).onComplete += () => onScaleFinish.Invoke();
#endif
        }
    }

    [ContextMenu("ChangeRotationAtOnce")]
    public void ChangeScaleAtOnce()
    {
#if USE_DOTween
        if (tweenerScale.NotNull())
            tweenerScale.Complete();
#endif

        Target.localScale = TargetScale;
    }

    #endregion

    Tweener SetUpTween(Tweener sourceTweener)
    {
        if (loops != 0 || loops != 1)
            sourceTweener.SetLoops(loops, loopType);
        return sourceTweener;
    }
    public void SetParent(Transform tfParent)
    {
        tfThis.parent = tfParent;
    }

    public void SetParentNull()
    {
        tfThis.parent = null;
    }

    #region Utility

    Vector3 GetTargetPosition()
    {
        if (tweenWith == TweenTo.TargetPoint)
            return moveEndPoint.position;
        else
            return TargetPosition;
    }


    #endregion

    /// <summary>
    /// 从哪个值开始
    /// </summary>
    [System.Serializable]
    public enum TweenFrom
    {
        CurValue,
        StartValue,
    }

    [System.Serializable]
    /// <summary>
    /// 基于哪类值作为变换
    /// </summary>
    public enum TweenTo
    {
        TargetValue,//值
        TargetPoint,//目标点
        DeltaValue,//间隔值
    }

}
