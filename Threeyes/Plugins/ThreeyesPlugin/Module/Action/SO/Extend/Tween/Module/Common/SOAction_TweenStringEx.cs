using DG.Tweening;
using UnityEngine;
using Threeyes.ValueHolder;
using Threeyes.Core;

namespace Threeyes.Action
{
    [CreateAssetMenu(menuName = EditorDefinition_Action.AssetMenuPrefix_Action_Tween_Common + "StringEx", fileName = "TweenStringEx")]
#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
#endif
    public class SOAction_TweenStringEx : SOAction_TweenBase<ActionConfig_TweenStringEx, string, IValueHolder<string>>
    {
        protected override Tween CreateTween(ActionTweenRuntimeData<ActionConfig_TweenStringEx, string, IValueHolder<string>> runtimeData)
        {
            var options = runtimeData.config.Option;
            return DOTween.To(() => runtimeData.Receiver.CurValue, (v) => runtimeData.Receiver.CurValue = v, runtimeData.EndValue, runtimeData.Duration).SetOptions(options.richTextEnabled, options.scrambleMode, options.scrambleChars);//Ref:DOText
        }
    }
}
