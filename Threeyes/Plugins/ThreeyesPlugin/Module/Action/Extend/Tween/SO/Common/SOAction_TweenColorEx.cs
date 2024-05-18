using Threeyes.Core;
using UnityEngine;
#if USE_DOTween
using DG.Tweening;
#endif
namespace Threeyes.Action
{

    [CreateAssetMenu(menuName = EditorDefinition_Action.AssetMenuPrefix_Action_Tween_Common + "ColorEx", fileName = "TweenColorEx")]
#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
#endif

    public class SOAction_TweenColorEx : SOAction_TweenBase<ActionConfig_TweenColorEx, Color, IValueHolder<Color>>
    {
#if USE_DOTween
       protected override Tween CreateTween(ActionTweenRuntimeData<ActionConfig_TweenColorEx, Color, IValueHolder<Color>> runtimeData)
        {
            Tween tween = null;
            var config = runtimeData.Config;
            var endValue = config.EndValue;
            if (runtimeData.Config.Option.OnlyChangeAlpha)
            {
                tween = DOTween.ToAlpha(() => runtimeData.Receiver.CurValue, (v) => runtimeData.Receiver.CurValue = v, endValue.a, config.Duration);
            }
            else
            {
                tween = DOTween.To(() => runtimeData.Receiver.CurValue, (v) => runtimeData.Receiver.CurValue = v, endValue, config.Duration);
            }
            return tween;
        }
#endif
    }
}