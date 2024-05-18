using UnityEngine;
using Threeyes.Core;
#if USE_DOTween
using DG.Tweening;
#endif
namespace Threeyes.Action
{
    [CreateAssetMenu(menuName = EditorDefinition_Action.AssetMenuPrefix_Action_Tween_Common + "Int", fileName = "TweenInt")]
#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
#endif
    public class SOAction_TweenInt : SOAction_TweenBase<ActionConfig_TweenInt, int, IValueHolder<int>>
    {
#if USE_DOTween
        protected override Tween CreateTween(ActionTweenRuntimeData<ActionConfig_TweenInt, int, IValueHolder<int>> runtimeData)
        {
            var config = runtimeData.Config;
            return DOTween.To(() => runtimeData.Receiver.CurValue, (v) => runtimeData.Receiver.CurValue = v, config.EndValue, config.Duration);
        }
#endif
    }
}
