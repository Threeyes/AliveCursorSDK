using DG.Tweening;
using UnityEngine;
using Threeyes.ValueHolder;
using Threeyes.Core;

namespace Threeyes.Action
{
    [CreateAssetMenu(menuName = EditorDefinition_Action.AssetMenuPrefix_Action_Tween_Common + "Int", fileName = "TweenInt")]
#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
#endif
    public class SOAction_TweenInt : SOAction_TweenBase<ActionConfig_TweenInt, int, IValueHolder<int>>
    {
        protected override Tween CreateTween(ActionTweenRuntimeData<ActionConfig_TweenInt, int, IValueHolder<int>> runtimeData)
        {
            return DOTween.To(() => runtimeData.Receiver.CurValue, (v) => runtimeData.Receiver.CurValue = v, runtimeData.EndValue, runtimeData.Duration);
        }
    }
}
