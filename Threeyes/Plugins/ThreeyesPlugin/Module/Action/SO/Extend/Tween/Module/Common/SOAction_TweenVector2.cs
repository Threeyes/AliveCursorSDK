using DG.Tweening;
using Threeyes.Core;
using UnityEngine;

namespace Threeyes.Action
{
    [CreateAssetMenu(menuName = EditorDefinition_Action.AssetMenuPrefix_Action_Tween_Common + "Vector2", fileName = "TweenVector2")]
#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
#endif
    public class SOAction_TweenVector2 : SOAction_TweenBase<ActionConfig_TweenVector2, Vector2, IValueHolder<Vector2>>
    {
        protected override Tween CreateTween(ActionTweenRuntimeData<ActionConfig_TweenVector2, Vector2, IValueHolder<Vector2>> runtimeData)
        {
            return DOTween.To(() => runtimeData.Receiver.CurValue, (v) => runtimeData.Receiver.CurValue = v, runtimeData.EndValue, runtimeData.Duration);
        }
    }
}
