using UnityEngine;
using DG.Tweening;
#if USE_NaughtyAttributes
using NaughtyAttributes;
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
        private void OnValidate()
        {
#if UNITY_EDITOR
            //PS:
            //  1.将[Common Setting]的设置传入Enter/Exit config中，便于NaughtyAttributes更新Inspector
            //  2.因为只有选中的才会调用并在Inspector显示，所以不用SetDirty保存
            enterConfig.Option.transformType = transformType;
            exitConfig.Option.transformType = transformType;
            //UnityEditor.EditorUtility.SetDirty(this);// mark as dirty, so the change will be save into scene file
#endif
        }


        [Header(headerCommonSetting)]
        public TransformType transformType = TransformType.Position;

        protected override Tween CreateTween(ActionTweenRuntimeData<ActionConfig_TweenTransform, Vector3, Transform> runtimeData)
        {
            Tween tween = null;
            var options = runtimeData.config.Option;
            Vector3 endValue = runtimeData.EndValue;

            switch (options.tweenType)
            {
                case Vector3TweenType.Common:
                    switch (transformType)
                    {
                        case TransformType.Position:
                            tween = options.isLocal ?
                            runtimeData.Receiver.DOLocalMove(endValue, runtimeData.Duration) :
                             runtimeData.Receiver.DOMove(endValue, runtimeData.Duration); break;
                        case TransformType.Rotation:
                            tween = options.isLocal ?
                            runtimeData.Receiver.DOLocalRotate(endValue, runtimeData.Duration, options.rotateMode) :
                             runtimeData.Receiver.DORotate(endValue, runtimeData.Duration, options.rotateMode); break;
                        case TransformType.Scale:

                            tween = runtimeData.Receiver.DOScale(endValue, runtimeData.Duration); break;//PS:LocalScale only
                    }
                    break;
                case Vector3TweenType.Punch:
                    switch (transformType)
                    {
                        case TransformType.Position:
                            tween = runtimeData.Receiver.DOPunchPosition(endValue, runtimeData.Duration, options.vibrato, options.elasticity, options.snapping); break;
                        case TransformType.Rotation:
                            tween = runtimeData.Receiver.DOPunchRotation(endValue, runtimeData.Duration, options.vibrato, options.elasticity); break;
                        case TransformType.Scale:
                            tween = runtimeData.Receiver.DOPunchScale(endValue, runtimeData.Duration, options.vibrato, options.elasticity); break;
                    }
                    break;
                case Vector3TweenType.Shake:
                    switch (transformType)
                    {
                        //PS:使用endValue作为strength，参考DoTweenAnimation
                        case TransformType.Position:
                            tween = runtimeData.Receiver.DOShakePosition(runtimeData.Duration, endValue, options.vibrato, options.randomness, options.snapping, options.fadeOut/*, options.shakeRandomnessMode*/); break;
                        case TransformType.Rotation:
                            tween = runtimeData.Receiver.DOShakeRotation(runtimeData.Duration, endValue, options.vibrato, options.randomness, options.fadeOut/*, options.shakeRandomnessMode*/); break;
                        case TransformType.Scale:
                            tween = runtimeData.Receiver.DOShakeScale(runtimeData.Duration, endValue, options.vibrato, options.randomness, options.fadeOut/*, options.shakeRandomnessMode*/); break;
                    }
                    break;
            }
            return tween;
        }

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

            [HideInInspector] public TransformType transformType = TransformType.Position; //#Common Setting (SetUp by Extern)
            protected bool IsLocalMode { get { return IsCommonMode && (transformType == TransformType.Position || transformType == TransformType.Rotation); } }
            protected bool IsRotateCommonMode { get { return transformType == TransformType.Rotation && tweenType == Vector3TweenType.Common; } }
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
