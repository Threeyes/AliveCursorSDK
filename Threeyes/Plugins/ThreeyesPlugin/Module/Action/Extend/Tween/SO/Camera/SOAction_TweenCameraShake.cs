using UnityEngine;
#if USE_NaughtyAttributes
using NaughtyAttributes;
#endif
#if USE_DOTween
using DG.Tweening;
#endif

namespace Threeyes.Action
{
    /// <summary>
    /// 
    /// </summary>
	[CreateAssetMenu(menuName = EditorDefinition_Action.AssetMenuPrefix_Action_Tween_Camera + "Shake", fileName = "TweenCameraShake")]
#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
#endif
    public class SOAction_TweenCameraShake : SOAction_TweenBase<SOAction_TweenCameraShake.ActionConfig_TweenCameraShake, Vector3, Camera>
    {
#if USE_DOTween
        protected override Tween CreateTween(ActionTweenRuntimeData<ActionConfig_TweenCameraShake, Vector3, Camera> runtimeData)
        {
            Tween tween = null;
            var config = runtimeData.Config;
            var options = config.Option;
            Vector3 endValue = config.EndValue;
            var receiverCamera = runtimeData.Receiver;

            ///ps:
            ///-该模块只专门处理Shake，如有需要在Exit时重置可以与SOAction_TweenTransform配合使用
            switch (options.cameraTransformType)
            {
                case CameraTransformType.Position:
                    tween = receiverCamera.DOShakePosition(config.Duration, endValue, options.Vibrato, options.Randomness, options.FadeOut/*, config.shakeRandomnessMode*/); break;
                case CameraTransformType.Rotation:
                    tween = receiverCamera.DOShakeRotation(config.Duration, endValue, options.Vibrato, options.Randomness, options.FadeOut/*, config.shakeRandomnessMode*/); break;
                default:
                    Debug.LogError("Not define!"); break;
            }
            return tween;
        }
#endif

        [System.Serializable]
        public class ActionConfig_TweenCameraShake : ActionConfig_TweenVector3Base<TweenOption_CameraShake> { }

        [System.Serializable]
        public class TweenOption_CameraShake : TweenOption_Vector3Ex
        {
            public CameraTransformType cameraTransformType = CameraTransformType.Position;

            public TweenOption_CameraShake() : base()
            {
                TweenType = Vector3TweenType.Shake;//Init as Shake for Camera
            }
        }

#region Define
        public enum CameraTransformType
        {
            Position,
            Rotation
        }
#endregion
    }
}