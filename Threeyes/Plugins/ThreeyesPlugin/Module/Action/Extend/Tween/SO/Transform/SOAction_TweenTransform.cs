using UnityEngine;
#if USE_NaughtyAttributes
using NaughtyAttributes;
#endif
#if USE_DOTween
using DG.Tweening;
#endif

namespace Threeyes.Action
{
    [CreateAssetMenu(menuName = EditorDefinition_Action.AssetMenuPrefix_Action_Tween_Transform + "Transform", fileName = "TweenTransform")]
#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
#endif
    /// <summary>
    /// PS:
    /// 1.将3种Transform类型整合在一起，方便切换
    /// 2.Punch and Shake通常只需要在Enter时使用，Exit应使用对应的Common，方便恢复到默认值
    /// </summary>

    public class SOAction_TweenTransform : SOAction_TweenBase<SOAction_TweenTransform.ActionConfig_TweenTransform, Vector3, Transform>
    {
        [Header(headerCommonSetting)]
        public TransformType transformType = TransformType.Position;

#if USE_DOTween
        protected override Tween CreateTween(ActionTweenRuntimeData<ActionConfig_TweenTransform, Vector3, Transform> runtimeData)
        {
            Tween tween = null;
            var config = runtimeData.Config;
            var options = config.Option;
            Vector3 endValue = config.EndValue;
            float duration = config.Duration;
            TransformType targetTransformType = transformType;//使用该类的transformType统一管理，而不是使用options.transformType
            var receiver = runtimeData.Receiver;

            switch (options.TweenType)
            {
                case Vector3TweenType.Common:
                    switch (targetTransformType)
                    {
                        case TransformType.Position:
                            tween = options.isLocal ?
                            receiver.DOLocalMove(endValue, duration) :
                             receiver.DOMove(endValue, duration); break;
                        case TransformType.Rotation:
                            tween = options.isLocal ?
                            receiver.DOLocalRotate(endValue, duration, options.rotateMode) :
                             receiver.DORotate(endValue, duration, options.rotateMode); break;
                        case TransformType.Scale:

                            tween = receiver.DOScale(endValue, duration); break;//PS:LocalScale only
                    }
                    break;
                case Vector3TweenType.Punch:
                    switch (targetTransformType)
                    {
                        case TransformType.Position:
                            tween = receiver.DOPunchPosition(endValue, duration, options.Vibrato, options.Elasticity, options.Snapping); break;
                        case TransformType.Rotation:
                            tween = receiver.DOPunchRotation(endValue, duration, options.Vibrato, options.Elasticity); break;
                        case TransformType.Scale:
                            tween = receiver.DOPunchScale(endValue, duration, options.Vibrato, options.Elasticity); break;
                    }
                    break;
                case Vector3TweenType.Shake:
                    switch (targetTransformType)
                    {
                        //PS:使用endValue作为strength，参考DoTweenAnimation
                        case TransformType.Position:
                            tween = receiver.DOShakePosition(duration, endValue, options.Vibrato, options.Randomness, options.Snapping, options.FadeOut/*, options.shakeRandomnessMode*/); break;
                        case TransformType.Rotation:
                            tween = receiver.DOShakeRotation(duration, endValue, options.Vibrato, options.Randomness, options.FadeOut/*, options.shakeRandomnessMode*/); break;
                        case TransformType.Scale:
                            tween = receiver.DOShakeScale(duration, endValue, options.Vibrato, options.Randomness, options.FadeOut/*, options.shakeRandomnessMode*/); break;
                    }
                    break;
            }
            return tween;
        }
        private void OnValidate()
        {
#if UNITY_EDITOR
            //PS:
            //  1.将[Common Setting]的设置传入Enter/Exit config 中，便于Option中的相关属性更新Inspector（基于NaughtyAttributes）。
            //  2.因为只有选中的才会调用并在Inspector显示，而且config的相关类型仅用于更新基于NaughtyAttributes，不需要使用，所以不用SetDirty保存
            enterConfig.Option.transformType = transformType;
            exitConfig.Option.transformType = transformType;
            //UnityEditor.EditorUtility.SetDirty(this);// mark as dirty, so the change will be save into scene file
#endif
        }
#endif

        [System.Serializable]
        public class ActionConfig_TweenTransform : ActionConfig_TweenVector3Base<TweenOption_Transform> { }
        [System.Serializable]
        public class TweenOption_Transform : TweenOption_Vector3Ex
        {
            //Common
#if USE_NaughtyAttributes
            [AllowNesting]
            [ShowIf(nameof(IsLocalMode))]
#endif
            public bool isLocal = true;//是否为局部坐标

            //#Rotation
#if USE_NaughtyAttributes
            [AllowNesting]
            [ShowIf(nameof(IsRotateCommonMode))]
#endif
            public RotateMode rotateMode = RotateMode.Fast;

            #region NaughtAttribute
            [HideInInspector] public TransformType transformType = TransformType.Position; //#Common Setting (Runtime SetUp by SOAction_TweenTransform)（PS：仅用于NaughtAttribute的其他字段判断，不使用该值）
            protected bool IsLocalMode { get { return IsCommonMode && (transformType == TransformType.Position || transformType == TransformType.Rotation); } }
            protected bool IsRotateCommonMode { get { return transformType == TransformType.Rotation && TweenType == Vector3TweenType.Common; } }
            #endregion
        }

        #region Define

        /// <summary>
        /// Transform的子模块
        /// Sub module of the Transform
        /// </summary>
        public enum TransformType
        {
            Position = 1,
            Rotation = 2,
            Scale = 3
        }

        #endregion
    }

}
