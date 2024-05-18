using Threeyes.Core;
using UnityEngine;
#if USE_DOTween
using DG.Tweening;
#endif
namespace Threeyes.Action
{
    [CreateAssetMenu(menuName = EditorDefinition_Action.AssetMenuPrefix_Action_Tween_Common + "Float", fileName = "TweenFloat")]
#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
#endif
    public class SOAction_TweenFloat : SOAction_TweenBase<ActionConfig_TweenFloat, float, IValueHolder<float>>
    {
#if USE_DOTween
       protected override Tween CreateTween(ActionTweenRuntimeData<ActionConfig_TweenFloat, float, IValueHolder<float>> runtimeData)
        {
            var config = runtimeData.Config;
            return DOTween.To(() => runtimeData.Receiver.CurValue, (v) => runtimeData.Receiver.CurValue = v, config.EndValue, config.Duration);
        }
#endif
    }
}
