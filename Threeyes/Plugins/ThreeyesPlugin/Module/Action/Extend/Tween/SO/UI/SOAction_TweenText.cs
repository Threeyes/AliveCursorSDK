using UnityEngine;
using UnityEngine.UI;
#if USE_DOTween
using DG.Tweening;
#endif
namespace Threeyes.Action
{
    [CreateAssetMenu(menuName = EditorDefinition_Action.AssetMenuPrefix_Action_Tween_UI + "Text", fileName = "TweenUIText")]
#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
#endif
    public class SOAction_TweenText : SOAction_TweenBase<ActionConfig_TweenStringEx, string, Text>
    {
#if USE_DOTween
        protected override Tween CreateTween(ActionTweenRuntimeData<ActionConfig_TweenStringEx, string, Text> runtimeData)
        {
            var config = runtimeData.Config;
            var options = config.Option;
            return runtimeData.Receiver.DOText(config.EndValue, config.Duration, options.RichTextEnabled, options.ScrambleMode, options.ScrambleChars);
        }
#endif
    }
}