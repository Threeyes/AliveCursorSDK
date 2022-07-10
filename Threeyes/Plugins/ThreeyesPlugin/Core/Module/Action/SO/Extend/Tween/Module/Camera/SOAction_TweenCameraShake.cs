using UnityEngine;
using DG.Tweening;

namespace Threeyes.Action
{
	[CreateAssetMenu(menuName = EditorDefinition_Action.AssetMenuPrefix_Action_Tween_Camera + "Shake", fileName = "TweenCameraShake")]
#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
#endif
    public class SOAction_TweenCameraShake : SOAction_TweenBase<SOAction_TweenCameraShake.ActionConfig_TweenCameraShake, Vector3, Camera>
    {
        protected override Tween CreateTween(ActionTweenRuntimeData<ActionConfig_TweenCameraShake, Vector3, Camera> runtimeData)
        {
            Tween tween = null;
            var option = runtimeData.config.Option;
            var receiver = runtimeData.Receiver;
            switch (option.shakeType)
            {
                case TweenOption_CameraShake.CameraShakeType.Position:
                    tween = receiver.DOShakePosition(runtimeData.Duration, runtimeData.EndValue, option.vibrato, option.randomness, option.fadeOut/*, config.shakeRandomnessMode*/); break;
                case TweenOption_CameraShake.CameraShakeType.Rotation:
                    tween = receiver.DOShakeRotation(runtimeData.Duration, runtimeData.EndValue, option.vibrato, option.randomness, option.fadeOut/*, config.shakeRandomnessMode*/); break;
                default:
                    Debug.LogError("Not define!"); break;
            }
            return tween;
        }

        [System.Serializable]
        public class ActionConfig_TweenCameraShake : ActionConfig_TweenVector3Base<TweenOption_CameraShake> { }

        [System.Serializable]
        public class TweenOption_CameraShake : TweenOption_Vector3Ex
        {
            public CameraShakeType shakeType = CameraShakeType.Position;

            public TweenOption_CameraShake() : base()
            {
                tweenType = Vector3TweenType.Shake;//Camera will remain Shake
            }

            public enum CameraShakeType
            {
                Position,
                Rotation
            }
        }
    }
}