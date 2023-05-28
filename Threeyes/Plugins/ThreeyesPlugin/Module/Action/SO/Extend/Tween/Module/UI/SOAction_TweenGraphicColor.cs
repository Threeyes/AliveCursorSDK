using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Threeyes.Action
{
    [CreateAssetMenu(menuName = EditorDefinition_Action.AssetMenuPrefix_Action_Tween_Graphic + "Color", fileName = "TweenGraphicColor")]
#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
#endif
    public class SOAction_TweenGraphicColor : SOAction_TweenGraphicBase<ActionConfig_TweenColorEx, Color>
    {
        protected override Tween CreateTween(ActionTweenRuntimeData<ActionConfig_TweenColorEx, Color, Graphic> runtimeData)
        {
            Tween tween = null;
            var endValue = runtimeData.EndValue;
            if (runtimeData.config.Option.onlyChangeAlpha)
            {
                tween = runtimeData.Receiver.DOFade(endValue.a, runtimeData.Duration);
            }
            else
            {
                tween = runtimeData.Receiver.DOColor(endValue, runtimeData.Duration);
            }
            return tween;
        }
    }
}
