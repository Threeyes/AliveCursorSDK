using DG.Tweening;
using DG.Tweening.Core.Easing;
using UnityEngine;
#if USE_NaughtyAttributes
using NaughtyAttributes;
#endif

namespace Threeyes.Action
{
    public abstract class ActionConfig_TweenBase<TParam> : ActionConfigBase<TParam>
    {
        #region Property & Field

        //——Tween——

        //Ref：DOTweenAnimation
        public float duration = 1;
        public float delay;

        /// <summary>
        /// 动画曲线，如果设置为INTERNAL_Custom，那就使用CustomEaseCurve
        /// </summary>
#if USE_NaughtyAttributes
        [AllowNesting]
        [OnValueChanged("InspectorUpdateEaseCurve")]
#endif
        public Ease easeType = Ease.OutQuad;
#if USE_NaughtyAttributes
        [AllowNesting]
        [HideIf(nameof(IsCustromCurve))]
#endif
        [Tooltip("Demonstrate current selected easeType, don't change it!")]
        public AnimationCurve demonstrateEaseCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));//可视化显示当前选择的曲线类型
#if USE_NaughtyAttributes
        [AllowNesting]
        [ShowIf(nameof(IsCustromCurve))]
#endif
        public AnimationCurve customEaseCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));//Valid if easeType is INTERNAL_Custom

        [Tooltip("Number of cycles to play (-1 for infinite - will be converted to 1 in case the tween is nested in a Sequence)")]
        public int loops = 1;
        public LoopType loopType = LoopType.Restart;

        public string id = null;
        [Tooltip("Should the target value be calculated as startValue + endValue?")]
        public bool isRelative;
        public bool isFrom;
        public bool isIndependentUpdate = false;
        public bool autoKill = false;//通过代码Kill
        public bool isCompleteOnKill = false;//建议在Exit时设为true，避免Stop被杀掉

        #endregion

        #region Editor Utility

        public override TInst Clone<TInst>()
        {
            var clone = base.Clone<TInst>();

            var cloneRealType = clone as ActionConfig_TweenBase<TParam>;//Conver to this type
            //We need to create new one for class type（ref：https://www.c-sharpcorner.com/blogs/cloning-of-object-shallow-copy-and-deep-copy-in-c-sharp）
            cloneRealType.customEaseCurve = new AnimationCurve(this.customEaseCurve.keys);
            cloneRealType.demonstrateEaseCurve = new AnimationCurve(this.demonstrateEaseCurve.keys);

            return clone;
        }

        #endregion

        #region NaughtAttribute
        bool IsCustromCurve { get { return easeType == Ease.INTERNAL_Custom; } }

#if UNITY_EDITOR
        /// <summary>
        /// 更新界面上的Curve
        /// (用于easeType)
        /// </summary>
        public void InspectorUpdateEaseCurve()
        {
            if (easeType != Ease.INTERNAL_Custom)
            {
                //PS:当传入的easeType为Custom时，才需要传特定customEase
                while (demonstrateEaseCurve.length > 0)
                {
                    demonstrateEaseCurve.RemoveKey(0);
                }
                int step = 20;
                for (int i = 0; i != step; i++)
                {
                    float curTime = (float)i / step;
                    float result = EaseManager.Evaluate(easeType, null, curTime, 1, DOTween.defaultEaseOvershootOrAmplitude, DOTween.defaultEasePeriod);
                    Keyframe nKF = new Keyframe(curTime, result);
                    demonstrateEaseCurve.AddKey(nKF);
                    UnityEditor.AnimationUtility.SetKeyLeftTangentMode(demonstrateEaseCurve, i, UnityEditor.AnimationUtility.TangentMode.Auto);
                    UnityEditor.AnimationUtility.SetKeyRightTangentMode(demonstrateEaseCurve, i, UnityEditor.AnimationUtility.TangentMode.Auto);
                }
            }
        }
#endif
        #endregion
    }

    //ToAdd:增加单独的命名空间
    /// <summary>
    /// Action config for DoTween
    /// </summary>
    /// <typeparam name="TParam">基础值</typeparam>
    public abstract class ActionConfig_TweenBase<TParam, TOption> : ActionConfig_TweenBase<TParam>
        where TOption : ActionOptionBase, new()
    {
        #region Property & Field

        public virtual TOption Option { get { return option; } }
        [SerializeField] protected TOption option = new TOption();//Extra config

        #endregion

        #region Editor Utility

        public override TInst Clone<TInst>()
        {
            var clone = base.Clone<TInst>();

            var cloneRealType = clone as ActionConfig_TweenBase<TParam, TOption>;//Conver to this type
            //We need to create new one for class type（ref：https://www.c-sharpcorner.com/blogs/cloning-of-object-shallow-copy-and-deep-copy-in-c-sharp）

            cloneRealType.option = Option.Clone<TOption>();
            return clone;
        }

        #endregion
    }

    //——Common——

    [System.Serializable]
    public class ActionConfig_TweenString : ActionConfig_TweenBase<string>
    {
        public override string ScaleEndValue(string origin, object scale)
        {
            return StringScaler.Instance.Scale(origin, scale);
        }
    }

    [System.Serializable]
    public class ActionConfig_TweenInt : ActionConfig_TweenBase<int>
    {
        public override int ScaleEndValue(int origin, object scale)
        {
            return IntScaler.Instance.Scale(origin, scale);
        }
    }
    [System.Serializable]
    public class ActionConfig_TweenFloat : ActionConfig_TweenBase<float>
    {
        public override float ScaleEndValue(float origin, object scale)
        {
            return FloatScaler.Instance.Scale(origin, scale);
        }
    }

    [System.Serializable]
    public class ActionConfig_TweenVector2 : ActionConfig_TweenBase<Vector2>
    {
        public override Vector2 ScaleEndValue(Vector2 origin, object scale)
        {
            return Vector2Scaler.Instance.Scale(origin, scale);
        }
    }

    [System.Serializable]
    public class ActionConfig_TweenVector3Base<TOption> : ActionConfig_TweenBase<Vector3, TOption>
      where TOption : ActionOptionBase, new()
    {
        public override Vector3 ScaleEndValue(Vector3 origin, object scale)
        {
            return Vector3Scaler.Instance.Scale(origin, scale);
        }
    }
    [System.Serializable]
    public class ActionConfig_TweenVector3 : ActionConfig_TweenVector3Base<ActionOption_Empty> { }

    [System.Serializable]
    public class ActionConfig_TweenVector4 : ActionConfig_TweenBase<Vector4>
    {
        public override Vector4 ScaleEndValue(Vector4 origin, object scale)
        {
            return Vector4Scaler.Instance.Scale(origin, scale);
        }
    }

    public class ActionConfig_TweenColorBase<TOption> : ActionConfig_TweenBase<Color, TOption>
        where TOption : ActionOptionBase, new()
    {
        public override Color EndValue { get { return endValueEx; } set { endValueEx = value; } }

        public override Color ScaleEndValue(Color origin, object scale)
        {
            return ColorScaler.Instance.Scale(origin, scale);
        }

#if USE_NaughtyAttributes
        [AllowNesting]
        [Label("End Value")]
#endif
        [ColorUsage(true, true)]//Show Alpha and HDR at the same time, in case the developer modify any of these field
        public Color endValueEx = Color.white;

        #region NaughtAttribute
        protected override bool IsShowDefaultEndValue => false;//Hide default endValue and use endValueEx instead
        #endregion
    }
    [System.Serializable]
    public class ActionConfig_TweenColor : ActionConfig_TweenColorBase<ActionOption_Empty> { }

    //——Config with Options——

    [System.Serializable]
    public class ActionConfig_TweenStringEx : ActionConfig_TweenBase<string, TweenOption_StringEx>
    {
        public override string ScaleEndValue(string origin, object scale)
        {
            return StringScaler.Instance.Scale(origin, scale);
        }
    }
    [System.Serializable]
    public class TweenOption_StringEx : ActionOptionBase
    {
        [Tooltip("If TRUE (default), rich text will be interpreted correctly while animated, otherwise all tags will be considered as normal text")]
        public bool richTextEnabled = true;
        [Tooltip("The type of scramble mode to use, if any")]
        public ScrambleMode scrambleMode = ScrambleMode.None;
        [Tooltip("Use as many characters as possible (minimum 10) because DOTween uses a fast scramble mode which gives better results with more characters. Leave it to NULL (default) to use default ones")]
        public string scrambleChars = null;
    }


    [System.Serializable]
    public class ActionConfig_TweenVector3Ex : ActionConfig_TweenVector3Base<TweenOption_Vector3Ex> { }

    [System.Serializable]
    public class TweenOption_Vector3Ex : ActionOptionBase
    {
        //#Tween Type
        public Vector3TweenType tweenType = Vector3TweenType.Common;

        //——Punch || Shake——
#if USE_NaughtyAttributes
        [AllowNesting]
        [ShowIf(EConditionOperator.Or, "IsPunchMode", "IsShakeMode")]
#endif
        [Tooltip("Indicates how much will the punch vibrate")]
        public int vibrato = 10;
#if USE_NaughtyAttributes
        [AllowNesting]
        [ShowIf(EConditionOperator.Or, "IsPunchMode", "IsShakeMode")]
#endif
        [Tooltip("If TRUE the tween will smoothly snap all values to integers")]
        public bool snapping = false;

        //——Punch——
#if USE_NaughtyAttributes
        [AllowNesting]
        [ShowIf(nameof(IsPunchMode))]
#endif
        [Range(0, 1)]
        [Tooltip("Represents how much (0 to 1) the vector will go beyond the starting rotation when bouncing backwards. 1 creates a full oscillation between the punch rotation and the opposite rotation, while 0 oscillates only between the punch and the start rotation")]
        public float elasticity = 1;

        //——Shake——
#if USE_NaughtyAttributes
        [AllowNesting]
        [ShowIf(nameof(IsShakeMode))]
#endif
        [Range(0, 180)]
        [Tooltip("Indicates how much the shake will be random (0 to 180 - values higher than 90 kind of suck, so beware). Setting it to 0 will shake along a single direction.")]
        public float randomness = 90;

#if USE_NaughtyAttributes
        [AllowNesting]
        [ShowIf(nameof(IsShakeMode))]
#endif
        [Tooltip("If TRUE the shake will automatically fadeOut smoothly within the tween's duration, otherwise it will not")]
        public bool fadeOut = true;

        #region Experimental
        //PS: These fields are interal for DOTween.Shake only, keep them for further usage
        //[Tooltip("If TRUE only shakes on the X Y axis (looks better with things like cameras).")]
        //public bool ignoreZAxis = false;
        //[Tooltip("If TRUE will be treat as Quaternion")]
        //public bool vectorBased = false;

        //PS: These field has not published on AssetStore version! active it later
        //        public DG.Tweening.ShakeRandomnessMode shakeRandomnessMode { get { return (DG.Tweening.ShakeRandomnessMode)_shakeRandomnessMode; } set { _shakeRandomnessMode = (ShakeRandomnessMode)value; } }
        //#if USE_NaughtyAttributes
        //        [AllowNesting]
        //        [ShowIf("IsShakeMode")]
        //#endif
        //        [SerializeField] protected ShakeRandomnessMode _shakeRandomnessMode = ShakeRandomnessMode.Full;
        #endregion

        #region NaughtAttribute
        protected bool IsCommonMode { get { return tweenType == Vector3TweenType.Common; } }
        protected bool IsPunchMode { get { return tweenType == Vector3TweenType.Punch; } }
        protected bool IsShakeMode { get { return tweenType == Vector3TweenType.Shake; } }

        #endregion
    }
    public enum Vector3TweenType
    {
        Common = 1,
        Punch = 2,
        Shake = 3
    }

    [System.Serializable]
    public class ActionConfig_TweenColorEx : ActionConfig_TweenColorBase<TweenOption_ColorEx> { }

    [System.Serializable]
    public class TweenOption_ColorEx : ActionOptionBase
    {
        public bool onlyChangeAlpha = false;//Change Alpha chanel only
    }
}
