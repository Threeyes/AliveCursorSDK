using UnityEngine;
using UnityEngine.UI;
#if USE_DOTween
using DG.Tweening;
#endif
namespace Threeyes.Action
{
    [CreateAssetMenu(menuName = EditorDefinition_Action.AssetMenuPrefix_Action_Tween_Graphic + "Color", fileName = "TweenGraphicColor")]
#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
#endif
    public class SOAction_TweenGraphicColor : SOAction_TweenGraphicBase<ActionConfig_TweenColorEx, Color>
    {
 #if USE_DOTween
       protected override Tween CreateTween(ActionTweenRuntimeData<ActionConfig_TweenColorEx, Color, Graphic> runtimeData)
        {
            Tween tween = null;
            var config = runtimeData.Config;
            var endValue = config.EndValue;
            if (config.Option.OnlyChangeAlpha)
            {
                tween = runtimeData.Receiver.DOFade(endValue.a, config.Duration);
            }
            else
            {
                tween = runtimeData.Receiver.DOColor(endValue, config.Duration);
            }
            return tween;
        }
#endif
    }
}
