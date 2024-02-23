using DG.Tweening;
using Threeyes.Core;
using UnityEngine;

namespace Threeyes.Action
{

    [CreateAssetMenu(menuName = EditorDefinition_Action.AssetMenuPrefix_Action_Tween_Common + "ColorEx", fileName = "TweenColorEx")]
#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
#endif

    public class SOAction_TweenColorEx : SOAction_TweenBase<ActionConfig_TweenColorEx, Color, IValueHolder<Color>>
    {
        protected override Tween CreateTween(ActionTweenRuntimeData<ActionConfig_TweenColorEx, Color, IValueHolder<Color>> runtimeData)
        {
            Tween tween = null;
            var endValue = runtimeData.EndValue;
            if (runtimeData.config.Option.onlyChangeAlpha)
            {
                tween = DOTween.ToAlpha(() => runtimeData.Receiver.CurValue, (v) => runtimeData.Receiver.CurValue = v, runtimeData.EndValue.a, runtimeData.Duration);
            }
            else
            {
                tween = DOTween.To(() => runtimeData.Receiver.CurValue, (v) => runtimeData.Receiver.CurValue = v, runtimeData.EndValue, runtimeData.Duration);
            }
            return tween;
        }
    }
}