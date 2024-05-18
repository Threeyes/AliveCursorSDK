using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System;
using Threeyes.Core;
#if USE_DOTween
using DG.Tweening;
#endif
/// <summary>
/// 规范：
/// 1.根据TParam编写子类：
///     如果一个组件有多个类型，那就进行拆分（如Material）
///     如果一个组件只有一个类型， 那就用枚举进行区分，方便切换（如Transform）

namespace Threeyes.Action
{
    /// <summary>
    /// Action for tween
    /// 
    /// PS:
    /// -取名为Tween而不是DoTween是为了兼容后续可能会增加的Tween插件库，因此名称要尽量通用
    /// </summary>
    /// <typeparam name="TActionConfig_Tween"></typeparam>
    /// <typeparam name="TValue">Key param</typeparam>
    /// <typeparam name="TActionReceiver">Component that Action interact with</typeparam>
    public abstract class SOAction_TweenBase<TActionConfig_Tween, TValue, TActionReceiver> : SOActionWithConfigBase<ActionTweenRuntimeData<TActionConfig_Tween, TValue, TActionReceiver>, TActionConfig_Tween, TValue, TActionReceiver>
        where TActionConfig_Tween : ActionConfig_TweenBase<TValue>
    {
        protected override void TryStopRunningProgress(ObjectID objectID)
        {
#if USE_DOTween
        TryKillTween(objectID); //Try kill pre Tween from Enter/Exit (不管其是哪种状态，只要是该SOAction就统一暂停)（如果想获取其他信息，可进行强转）
#endif
            base.TryStopRunningProgress(objectID);
        }
        protected override void EnterExitFunc(ActionTweenRuntimeData<TActionConfig_Tween, TValue, TActionReceiver> runtimeData, bool isEnter)
        {
#if USE_DOTween
            Tween tween = CreateTween(runtimeData);
            if (tween != null)
                runtimeData.tween = SetUpTween(tween, runtimeData);//Setup Tween
#endif
        }

#if USE_DOTween
        /// <summary>
        /// Create tween using receiver and config
        /// </summary>
        /// <param name="runtimeData"></param>
        /// 
        /// <returns>null if something wrong</returns>
        protected abstract Tween CreateTween(ActionTweenRuntimeData<TActionConfig_Tween, TValue, TActionReceiver> runtimeData);

        protected virtual Tween SetUpTween(Tween tween, ActionTweenRuntimeData<TActionConfig_Tween, TValue, TActionReceiver> runtimeData)
        {
            TActionConfig_Tween config = runtimeData.Config;

            tween
            //.SetTarget(setTarget)//每个Tween的Target都不一样，由子类的CreateTween自行实现
            .SetDelay(config.Delay)
            .SetLoops(config.Loops, config.LoopType)
            .SetId(config.Id)
            .SetUpdate(config.IsIndependentUpdate)
            .SetAutoKill(config.AutoKill);

            if (config.IsFrom)
                ((Tweener)tween).From(config.IsRelative);
            else
                tween.SetRelative(config.IsRelative);


            switch (config.EaseType)
            {
                case Ease.INTERNAL_Custom:
                    tween.SetEase(config.CustomEaseCurve); break;
                default:
                    tween.SetEase(config.EaseType); break;
            }

            //为该Tween增加唯一运行时标记(ToUpdate:改为RuntimeData的ID)
            string tweenID = "";
            if (tween.target != null)
                tweenID += tween.target + " ";
            tweenID += runtimeData.actionState;
            tween.SetId(tweenID);

            tween.onComplete += () =>
            {
                runtimeData.Progress = StateProgress.Complete;//标记为完成
            };

            return tween;
        }

#region Cache Tween
        protected override void RemoveRuntimeData(ObjectID objectID)
        {
            TryKillTween(objectID);
            base.RemoveRuntimeData(objectID);
        }

        /// <summary>
        /// 删除当前Tween
        /// </summary>
        /// <param name="target"></param>
        protected void TryKillTween(ObjectID objectID)
        {
            var runtimeData = GetRuntimeData(objectID);
            if (runtimeData is ActionTweenRuntimeData<TActionConfig_Tween, TValue, TActionReceiver> instRuntimeData)
            {
                Tween tween = instRuntimeData.tween;
                if (tween != null)
                {
                    tween.Kill(instRuntimeData.Config.IsCompleteOnKill);
                }
            }
        }

        protected override void OnManualDestroy(ObjectID objectID)
        {
            TryKillTween(objectID);//提前销毁Tween
        }
#endregion
#endif
    }


    public class ActionTweenRuntimeData<TActionConfig, TValue, TActionReceiver> : ActionRuntimeData<TActionConfig, TValue, TActionReceiver>
        where TActionConfig : ActionConfig_TweenBase<TValue>
    {
#if USE_DOTween

        //#Runtime
        public Tween tween;
#endif
    }
}
