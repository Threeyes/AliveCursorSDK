using Threeyes.Core;
using UnityEngine;
#if USE_DOTween
using DG.Tweening;
#endif

namespace Threeyes.Action
{
    [CreateAssetMenu(menuName = EditorDefinition_Action.AssetMenuPrefix_Action_Tween_Common + "Vector2", fileName = "TweenVector2")]
#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
#endif
    public class SOAction_TweenVector2 : SOAction_TweenBase<ActionConfig_TweenVector2, Vector2, IValueHolder<Vector2>>
    {
 #if USE_DOTween
       protected override Tween CreateTween(ActionTweenRuntimeData<ActionConfig_TweenVector2, Vector2, IValueHolder<Vector2>> runtimeData)
        {
            //ToUpdate:增加类似Vector3TweenType的枚举，传入值是Vector3
            var config = runtimeData.Config;
            return DOTween.To(() => runtimeData.Receiver.CurValue, (v) => runtimeData.Receiver.CurValue = v, config.EndValue, config.Duration);
        }
#endif
    }
}
