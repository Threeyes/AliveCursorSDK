using DG.Tweening;
using Threeyes.Core;
using Threeyes.ValueHolder;
using UnityEngine;

namespace Threeyes.Action
{
    [CreateAssetMenu(menuName = EditorDefinition_Action.AssetMenuPrefix_Action_Tween_Common + "Float", fileName = "TweenFloat")]
#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
#endif
    public class SOAction_TweenFloat : SOAction_TweenBase<ActionConfig_TweenFloat, float, IValueHolder<float>>
    {
        protected override Tween CreateTween(ActionTweenRuntimeData<ActionConfig_TweenFloat, float, IValueHolder<float>> runtimeData)
        {
            return DOTween.To(() => runtimeData.Receiver.CurValue, (v) => runtimeData.Receiver.CurValue = v, runtimeData.EndValue, runtimeData.Duration);
        }
    }
}
