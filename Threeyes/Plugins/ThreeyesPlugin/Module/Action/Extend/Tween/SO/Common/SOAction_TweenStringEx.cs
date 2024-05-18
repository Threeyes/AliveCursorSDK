using UnityEngine;
using Threeyes.Core;
#if USE_DOTween
using DG.Tweening;
#endif
namespace Threeyes.Action
{
    [CreateAssetMenu(menuName = EditorDefinition_Action.AssetMenuPrefix_Action_Tween_Common + "StringEx", fileName = "TweenStringEx")]
#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
#endif
    public class SOAction_TweenStringEx : SOAction_TweenBase<ActionConfig_TweenStringEx, string, IValueHolder<string>>
    {
#if USE_DOTween
       protected override Tween CreateTween(ActionTweenRuntimeData<ActionConfig_TweenStringEx, string, IValueHolder<string>> runtimeData)
        {
            var config = runtimeData.Config;
            var options = config.Option;
            return DOTween.To(() => runtimeData.Receiver.CurValue, (v) => runtimeData.Receiver.CurValue = v, config.EndValue, config.Duration).SetOptions(options.RichTextEnabled, options.ScrambleMode, options.ScrambleChars);//Ref:DOText
        }
#endif
    }
}
