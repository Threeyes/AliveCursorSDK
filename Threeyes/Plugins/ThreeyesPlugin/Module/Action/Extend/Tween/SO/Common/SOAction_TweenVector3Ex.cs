using Threeyes.Core;
using UnityEngine;
#if USE_DOTween
using DG.Tweening;
#endif

namespace Threeyes.Action
{
    /// <summary>
    ///
    ///
    /// PS:
    /// 1. Common对应ActionConfig_TweenVector3的配置，因此不需要单独增加SOAction_TweenVector3
    /// </summary>
    [CreateAssetMenu(menuName = EditorDefinition_Action.AssetMenuPrefix_Action_Tween_Common + "Vector3Ex", fileName = "TweenVector3Ex")]
#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
#endif
    public class SOAction_TweenVector3Ex : SOAction_TweenCommonBase<ActionConfig_TweenVector3Ex, Vector3>
    {
#if USE_DOTween
        protected override Tween CreateTween(ActionTweenRuntimeData<ActionConfig_TweenVector3Ex, Vector3, IValueHolder<Vector3>> runtimeData)
        {
            var config = runtimeData.Config;
            var options = config.Option;
            float duration = config.Duration;

            switch (options.TweenType)
            {
                case Vector3TweenType.Common:
                    return DOTween.To(() => runtimeData.Receiver.CurValue, (v) => runtimeData.Receiver.CurValue = v, config.EndValue, duration);
                case Vector3TweenType.Punch:
                    //EndValue: The direction and strength of the punch(ToDo：加一个提示框来进行说明）
                    return DOTween.Punch(() => runtimeData.Receiver.CurValue, (v) => runtimeData.Receiver.CurValue = v, config.EndValue, duration, options.Vibrato, options.Elasticity).SetOptions(options.Snapping);
                case Vector3TweenType.Shake:
                    //EndValue: The shake strength on each axis
                    return DOTween.Shake(() => runtimeData.Receiver.CurValue, (v) => runtimeData.Receiver.CurValue = v, duration, config.EndValue, options.Vibrato, options.Randomness, options.FadeOut/*, config.options.shakeRandomnessMode*/);
                default:
                    Debug.LogError("Not define!");
                    return null;
            }
        }
#endif
    }
}
