using UnityEngine;
using System.Collections.Generic;

#if USE_NaughtyAttributes
using NaughtyAttributes;
#endif
#if USE_JsonDotNet
using Newtonsoft.Json;
#endif
#if USE_DOTween
using DG.Tweening;
using DG.Tweening.Core.Easing;
#endif
namespace Threeyes.Action
{
    /// <summary>
    /// Provide interfaces for ActionConfig_TweenBase<TValue>, to facilitate modifier access
    /// </summary>
    public interface IActionConfig_Tween : IActionConfig
    {
        float Duration { get; set; }
        float Delay { get; set; }
        Ease EaseType { get; set; }
        AnimationCurve CustomEaseCurve { get; set; }
        int Loops { get; set; }
        LoopType LoopType { get; set; }
        string Id { get; set; }
        bool IsRelative { get; set; }
        bool IsFrom { get; set; }
        bool IsIndependentUpdate { get; set; }
        bool AutoKill { get; set; }
        bool IsCompleteOnKill { get; set; }
    }

    /// <summary>
    /// Tween's common setting
    /// 
    /// Ref：DOTweenAnimation
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public abstract class ActionConfig_TweenBase<TValue> : ActionConfigBase<TValue>, IActionConfig_Tween
    {
        #region Property & Field
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public float Duration { get { return duration; } set { duration = value; } }
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public float Delay { get { return delay; } set { delay = value; } }
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public Ease EaseType { get { return easeType; } set { easeType = value; } }
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public AnimationCurve CustomEaseCurve { get { return customEaseCurve; } set { customEaseCurve = value; } }
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public int Loops { get { return loops; } set { loops = value; } }
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public LoopType LoopType { get { return loopType; } set { loopType = value; } }
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public string Id { get { return id; } set { id = value; } }
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public bool IsRelative { get { return isRelative; } set { isRelative = value; } }
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public bool IsFrom { get { return isFrom; } set { isFrom = value; } }
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public bool IsIndependentUpdate { get { return isIndependentUpdate; } set { isIndependentUpdate = value; } }
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public bool AutoKill { get { return autoKill; } set { autoKill = value; } }
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public bool IsCompleteOnKill { get { return isCompleteOnKill; } set { isCompleteOnKill = value; } }


        [SerializeField] protected float duration = 1;
        [SerializeField] protected float delay;

        /// <summary>
        /// 动画曲线，如果设置为INTERNAL_Custom，那就使用CustomEaseCurve
        /// </summary>
#if USE_NaughtyAttributes
        [AllowNesting]
        [OnValueChanged(nameof(InspectorUpdateEaseCurve))]
#endif
        [SerializeField] protected Ease easeType = Ease.OutQuad;
#if USE_NaughtyAttributes
        [AllowNesting]
        [HideIf(nameof(IsCustromCurve))]
#endif
        [Tooltip("Demonstrate current selected easeType, don't change it!")]
        [SerializeField] protected AnimationCurve demonstrateEaseCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));//可视化显示当前选择的曲线类型
#if USE_NaughtyAttributes
        [AllowNesting]
        [ShowIf(nameof(IsCustromCurve))]
#endif
        [SerializeField] protected AnimationCurve customEaseCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));//Valid if easeType is INTERNAL_Custom

        [Tooltip("Number of cycles to play (-1 for infinite - will be converted to 1 in case the tween is nested in a Sequence)")]
        [SerializeField] protected int loops = 1;
        [SerializeField] protected LoopType loopType = LoopType.Restart;

        [SerializeField] protected string id = null;
        [Tooltip("Should the target value be calculated as startValue + endValue?")]
        [SerializeField] protected bool isRelative = false;
        [SerializeField] protected bool isFrom = false;
        [SerializeField] protected bool isIndependentUpdate = false;
        [SerializeField] protected bool autoKill = false;//通过代码Kill
        [SerializeField] protected bool isCompleteOnKill = false;//建议在Exit时设为true，避免Stop被杀掉
        #endregion

        #region Editor Utility
        public override TInst Clone<TInst>()
        {
            var clone = base.Clone<TInst>();

            var cloneRealType = clone as ActionConfig_TweenBase<TValue>;//Conver to this type
            //We need to create new one for class type（ref：https://www.c-sharpcorner.com/blogs/cloning-of-object-shallow-copy-and-deep-copy-in-c-sharp）
            cloneRealType.CustomEaseCurve = new AnimationCurve(this.CustomEaseCurve.keys);
            cloneRealType.demonstrateEaseCurve = new AnimationCurve(this.demonstrateEaseCurve.keys);

            return clone;
        }
        #endregion

        #region NaughtAttribute
        bool IsCustromCurve { get { return EaseType == Ease.INTERNAL_Custom; } }

        /// <summary>
        /// 更新界面上的Curve
        /// (用于easeType)
        /// </summary>
        public void InspectorUpdateEaseCurve()
        {
#if UNITY_EDITOR
#if USE_DOTween
            if (EaseType != Ease.INTERNAL_Custom)//PS:当传入的easeType为Custom时，才需要传特定customEase
            {
                while (demonstrateEaseCurve.length > 0)//移除旧Key
                {
                    demonstrateEaseCurve.RemoveKey(0);
                }

                //使用demonstrateEaseCurve可视化呈现当前的EaseType外形
                int step = 20;//细分
                for (int i = 0; i != step; i++)
                {
                    float curTime = (float)i / step;
                    float result = EaseManager.Evaluate(EaseType, null, curTime, 1, DOTween.defaultEaseOvershootOrAmplitude, DOTween.defaultEasePeriod);
                    Keyframe nKF = new Keyframe(curTime, result);
                    demonstrateEaseCurve.AddKey(nKF);
                    UnityEditor.AnimationUtility.SetKeyLeftTangentMode(demonstrateEaseCurve, i, UnityEditor.AnimationUtility.TangentMode.Auto);
                    UnityEditor.AnimationUtility.SetKeyRightTangentMode(demonstrateEaseCurve, i, UnityEditor.AnimationUtility.TangentMode.Auto);
                }
            }
#endif
#endif
        }
        #endregion
    }

    /// <summary>
    /// Action config for DoTween
    /// </summary>
    /// <typeparam name="TValue">Base value</typeparam>
    /// <typeparam name="TOption">Extra option</typeparam>
    public abstract class ActionConfig_TweenBase<TValue, TOption> : ActionConfig_TweenBase<TValue>
    where TOption : ActionOptionBase, new()
    {
        #region Property & Field
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public virtual TOption Option { get { return option; } }
        [SerializeField] protected TOption option = new TOption();//Extra config

        #endregion

        #region Editor Utility
        public override TInst Clone<TInst>()
        {
            var clone = base.Clone<TInst>();

            var cloneRealType = clone as ActionConfig_TweenBase<TValue, TOption>;//Conver to this type
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
        protected override string ModifyEndValue(string origin, object scale)
        {
            return StringScaler.Scale(origin, scale);
        }
    }

    [System.Serializable]
    public class ActionConfig_TweenInt : ActionConfig_TweenBase<int>
    {
        protected override int ModifyEndValue(int origin, object scale)
        {
            return IntScaler.Scale(origin, scale);
        }
    }
    [System.Serializable]
    public class ActionConfig_TweenFloat : ActionConfig_TweenBase<float>
    {
        protected override float ModifyEndValue(float origin, object scale)
        {
            return FloatScaler.Scale(origin, scale);
        }
    }

    [System.Serializable]
    public class ActionConfig_TweenVector2 : ActionConfig_TweenBase<Vector2>
    {
        protected override Vector2 ModifyEndValue(Vector2 origin, object scale)
        {
            return Vector2Scaler.Scale(origin, scale);
        }
    }

    [System.Serializable]
    public class ActionConfig_TweenVector3Base<TOption> : ActionConfig_TweenBase<Vector3, TOption>
      where TOption : ActionOptionBase, new()
    {
        protected override Vector3 ModifyEndValue(Vector3 origin, object scale)
        {
            return Vector3Scaler.Scale(origin, scale);
        }
    }
    [System.Serializable]
    public class ActionConfig_TweenVector3 : ActionConfig_TweenVector3Base<ActionOption_Empty> { }

    [System.Serializable]
    public class ActionConfig_TweenVector4 : ActionConfig_TweenBase<Vector4>
    {
        protected override Vector4 ModifyEndValue(Vector4 origin, object scale)
        {
            return Vector4Scaler.Scale(origin, scale);
        }
    }

    public class ActionConfig_TweenColorBase<TOption> : ActionConfig_TweenBase<Color, TOption>
        where TOption : ActionOptionBase, new()
    {
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public override Color EndValue { get { return endValueEx; } set { endValueEx = value; } }

#if USE_NaughtyAttributes
        [AllowNesting]
        [Label("End Value")]
#endif
        [ColorUsage(true, true)]//Show Alpha and HDR at the same time, in case the developer modify any of these field
        [SerializeField] protected Color endValueEx = Color.white;

        protected override Color ModifyEndValue(Color origin, object scale)
        {
            return ColorScaler.Scale(origin, scale);
        }


        #region NaughtAttribute
        protected override bool IsShowDefaultEndValue => false;//Hide default endValue and use endValueEx instead（因为要绘制HDR等属性）
        #endregion
    }
    [System.Serializable]
    public class ActionConfig_TweenColor : ActionConfig_TweenColorBase<ActionOption_Empty> { }

    //——Config with Options——

    [System.Serializable]
    public class ActionConfig_TweenStringEx : ActionConfig_TweenBase<string, TweenOption_StringEx>
    {
        protected override string ModifyEndValue(string origin, object scale)
        {
            return StringScaler.Scale(origin, scale);
        }
    }
    [System.Serializable]
    public class TweenOption_StringEx : ActionOptionBase
    {
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public bool RichTextEnabled { get { return richTextEnabled; } set { richTextEnabled = value; } }
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public ScrambleMode ScrambleMode { get { return scrambleMode; } set { scrambleMode = value; } }
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public string ScrambleChars { get { return scrambleChars; } set { scrambleChars = value; } }

        [Tooltip("If TRUE (default), rich text will be interpreted correctly while animated, otherwise all tags will be considered as normal text")]
        [SerializeField] protected bool richTextEnabled = true;
        [Tooltip("The type of scramble mode to use, if any")]
        [SerializeField] protected ScrambleMode scrambleMode = ScrambleMode.None;
        [Tooltip("Use as many characters as possible (minimum 10) because DOTween uses a fast scramble mode which gives better results with more characters. Leave it to NULL (default) to use default ones")]
        [SerializeField] protected string scrambleChars = null;
    }


    [System.Serializable]
    public class ActionConfig_TweenVector3Ex : ActionConfig_TweenVector3Base<TweenOption_Vector3Ex> { }

    [System.Serializable]
    public class TweenOption_Vector3Ex : ActionOptionBase
    {
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public Vector3TweenType TweenType { get { return tweenType; } set { tweenType = value; } }
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public int Vibrato { get { return vibrato; } set { vibrato = value; } }
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public bool Snapping { get { return snapping; } set { snapping = value; } }
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public float Elasticity { get { return elasticity; } set { elasticity = value; } }
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public float Randomness { get { return randomness; } set { randomness = value; } }
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public bool FadeOut { get { return fadeOut; } set { fadeOut = value; } }

        //#Tween Type
        private Vector3TweenType tweenType = Vector3TweenType.Common;

        //——Punch || Shake——
#if USE_NaughtyAttributes
        [AllowNesting]
        [ShowIf(EConditionOperator.Or, "IsPunchMode", "IsShakeMode")]
#endif
        [Tooltip("Indicates how much will the punch vibrate")]
        private int vibrato = 10;
#if USE_NaughtyAttributes
        [AllowNesting]
        [ShowIf(EConditionOperator.Or, "IsPunchMode", "IsShakeMode")]
#endif
        [Tooltip("If TRUE the tween will smoothly snap all values to integers")]
        private bool snapping = false;

        //——Punch——
#if USE_NaughtyAttributes
        [AllowNesting]
        [ShowIf(nameof(IsPunchMode))]
#endif
        [Range(0, 1)]
        [Tooltip("Represents how much (0 to 1) the vector will go beyond the starting rotation when bouncing backwards. 1 creates a full oscillation between the punch rotation and the opposite rotation, while 0 oscillates only between the punch and the start rotation")]
        private float elasticity = 1;

        //——Shake——
#if USE_NaughtyAttributes
        [AllowNesting]
        [ShowIf(nameof(IsShakeMode))]
#endif
        [Range(0, 180)]
        [Tooltip("Indicates how much the shake will be random (0 to 180 - values higher than 90 kind of suck, so beware). Setting it to 0 will shake along a single direction.")]
        private float randomness = 90;

#if USE_NaughtyAttributes
        [AllowNesting]
        [ShowIf(nameof(IsShakeMode))]
#endif
        [Tooltip("If TRUE the shake will automatically fadeOut smoothly within the tween's duration, otherwise it will not")]
        private bool fadeOut = true;

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
        protected bool IsCommonMode { get { return TweenType == Vector3TweenType.Common; } }
        protected bool IsPunchMode { get { return TweenType == Vector3TweenType.Punch; } }
        protected bool IsShakeMode { get { return TweenType == Vector3TweenType.Shake; } }
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
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public bool OnlyChangeAlpha { get { return onlyChangeAlpha; } set { onlyChangeAlpha = value; } }

        [SerializeField] protected bool onlyChangeAlpha = false;//Change Alpha chanel only
    }
}
