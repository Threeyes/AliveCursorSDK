using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
/// <summary>
/// 规范：
/// 1.根据TParam编写子类：
///     如果一个组件有多个类型，那就进行拆分（如Material）
///     如果一个组件只有一个类型， 那就用枚举进行区分，方便切换（如Transform）

namespace Threeyes.Action
{
    /// <summary>
    /// Action that require Receiver
    /// </summary>
    /// <typeparam name="TActionConfig_Tween"></typeparam>
    /// <typeparam name="TValue">Key param</typeparam>
    /// <typeparam name="TActionReceiver">Component that Action interact with</typeparam>
    public abstract class SOAction_TweenBase<TActionConfig_Tween, TValue, TActionReceiver> : SOActionWithConfigBase<ActionTweenRuntimeData<TActionConfig_Tween, TValue, TActionReceiver>, TActionConfig_Tween, TValue, TActionReceiver>
        where TActionConfig_Tween : ActionConfig_TweenBase<TValue>
    {
        #region Inherit

        protected override ActionRuntimeData Enter(ActionState actionState, GameObject target, object value = null, UnityAction actOnComplete = null, List<IActionModifier> listModifier = null, string id = "")
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)//非运行时禁用
                return ActionRuntimeData.None;
#endif
            TryKillTween(target); //Try kill pre Tween from Enter/Exit state
            return base.Enter(actionState, target, value, actOnComplete, listModifier, id);
        }

        protected override void EnterExitFunc(ActionTweenRuntimeData<TActionConfig_Tween, TValue, TActionReceiver> runtimeData)
        {
            Tween tween = CreateTween(runtimeData);
            if (tween != null)
            {
                runtimeData.tween = SetUpTween(tween, runtimeData);//Setup Tween
            }
        }

        /// <summary>
        /// Create tween using receiver and config
        /// </summary>
        /// <param name="runtimeData"></param>
        /// 
        /// <returns>null if something wrong</returns>
        protected abstract Tween CreateTween(ActionTweenRuntimeData<TActionConfig_Tween, TValue, TActionReceiver> runtimeData);

        protected virtual Tween SetUpTween(Tween tween, ActionTweenRuntimeData<TActionConfig_Tween, TValue, TActionReceiver> runtimeData)
        {
            TActionConfig_Tween config = runtimeData.config;

            tween
            //.SetTarget(setTarget)//每个Tween的Target都不一样
            .SetDelay(config.delay)
            .SetLoops(config.loops, config.loopType)
            .SetId(config.id)
            .SetAutoKill(config.autoKill);

            if (config.isFrom)
                ((Tweener)tween).From(config.isRelative);
            else
                tween.SetRelative(config.isRelative);


            switch (config.easeType)
            {
                case Ease.INTERNAL_Custom:
                    tween.SetEase(config.customEaseCurve); break;
                default:
                    tween.SetEase(config.easeType); break;
            }

            string tweenID = "";
            if (tween.target != null)
                tweenID += tween.target + " ";
            tweenID += runtimeData.actionState;
            tween.SetId(tweenID);//用于在运行时标记

            if (tween != null && runtimeData.actOnComplete != null)
            {
                tween.onComplete += () => runtimeData.actOnComplete();
            }

            return tween;
        }

        #endregion

        #region Cache Tween


        protected override void RemoveRuntimeData(GameObject target, string id = "")
        {
            TryKillTween(target);
            base.RemoveRuntimeData(target);
        }

        protected Tween TryGetTween(GameObject target, string id = "")
        {
            var runtimeData = GetRuntimeData(target);
            if (runtimeData != null)
                return (runtimeData as ActionTweenRuntimeData<TActionConfig_Tween, TValue, TActionReceiver>).tween;
            return null;
        }
        /// <summary>
        /// 删除当前Tween
        /// </summary>
        /// <param name="target"></param>
        protected void TryKillTween(GameObject target, string id = "")
        {
            Tween tween = TryGetTween(target, id);
            if (tween != null)
            {
                tween.Kill();
                return;
            }
        }

        protected override void OnManualDestroy(GameObject target, string id = "")
        {
            TryKillTween(target, id);//提前销毁Tween
        }
        #endregion
    }


    public class ActionTweenRuntimeData<TActionConfig, TValue, TActionReceiver> : ActionRuntimeData<TActionConfig, TValue, TActionReceiver>
        where TActionConfig : ActionConfig_TweenBase<TValue>
    {
        public Tween tween;

        /// <summary>
        /// Return modified duration
        /// </summary>
        public float Duration
        {
            get
            {
                float duration = config.duration;
                foreach (var actionModifier in GetListModifier<IActionModifier_Tween>())
                {
                    duration = duration * actionModifier.GetDurationScale(actionState);
                }
                return duration;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="actionState"></param>
        /// <returns>
        /// </returns>
        public override StateProgress GetProgress()
        {
            //检查当前的Tween状态
            if (tween != null)
            {
                if (tween.IsPlaying())//PS:即使Delay大于0，只要是启用了，IsPlaying也返回true
                    return StateProgress.Processing;
                else if (tween.IsComplete())
                    return StateProgress.Complete;
            }

            return base.GetProgress();//默认为Complete
        }
    }
}
