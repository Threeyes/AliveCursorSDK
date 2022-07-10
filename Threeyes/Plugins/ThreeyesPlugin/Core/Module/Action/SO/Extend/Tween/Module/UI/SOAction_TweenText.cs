using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Threeyes.Action
{
    [CreateAssetMenu(menuName = EditorDefinition_Action.AssetMenuPrefix_Action_Tween_UI + "Text", fileName = "TweenUIText")]
#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
#endif
    public class SOAction_TweenText : SOAction_TweenBase<ActionConfig_TweenStringEx, string, Text>
    {
        protected override Tween CreateTween(ActionTweenRuntimeData<ActionConfig_TweenStringEx, string, Text> runtimeData)
        {
            var options = runtimeData.config.Option;
            return runtimeData.Receiver.DOText(runtimeData.EndValue, runtimeData.Duration, options.richTextEnabled, options.scrambleMode, options.scrambleChars);
        }
    }
}