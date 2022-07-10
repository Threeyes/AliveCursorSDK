using UnityEngine;
#if USE_NaughtyAttributes
using NaughtyAttributes;
#endif
namespace Threeyes.Action
{
    [CreateAssetMenu(menuName = EditorDefinition_Action.AssetMenuPrefix_Action_BuiltIn_Animator + "State", fileName = "AnimatorState")]
#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
#endif
    /// <summary>
    /// Change Animator's state
    /// EndValue controls Animator.speed
    /// </summary>
    public class SOAction_AnimatorState : SOAction_AnimatorBase<ActionConfig_AnimatorState, float>
    {
        //ToDo:写额外的AnimationModifier，并且自己编写ActionRuntimeData，来设置对应的值
        protected override void EnterExitFunc(ActionRuntimeData<ActionConfig_AnimatorState, float, Animator> runtimeData)
        {
            var config = runtimeData.config;
            var receiver = runtimeData.Receiver;

            if (config.layerIndex > 0 && receiver.layerCount > config.layerIndex)//layerIndex can be -1
            {
                Debug.LogError(receiver + " doesn't have Layerindex " + config.layerIndex + " !");
                return;
            }

            ////ToFix:无效
            //if (!receiver.HasState(config.layerIndex, Animator.StringToHash(config.stateName)))
            //{
            //    Debug.LogError(receiver + " doesn't have state " + config.stateName + " !");
            //    return;
            //}

            switch (config.statePlayType)
            {
                case StatePlayType.Play:
                    receiver.Play(config.stateName, config.layerIndex, config.normalizedTime); break;
                case StatePlayType.PlayInFixedTime:
                    receiver.PlayInFixedTime(config.stateName, config.layerIndex, config.fixedTime); break;
                case StatePlayType.CrossFade:
                    receiver.CrossFade(config.stateName, config.normalizedTransitionDuration, config.layerIndex, config.normalizedTimeOffset, config.normalizedTransitionTime); break;
                case StatePlayType.CrossFadeInFixedTime:
                    receiver.CrossFadeInFixedTime(config.stateName, config.fixedTransitionDuration, config.layerIndex, config.fixedTimeOffset, config.normalizedTransitionTime); break;
            }

            //ToUpdate:研究改为获取指定stateinfo并修改其speed
            receiver.speed = runtimeData.EndValue;
        }
    }

    [System.Serializable]
    public class ActionConfig_AnimatorState : ActionConfigBase<float>
    {
        //PS:EndValue: When you specify a state name, or the string used to generate a hash, it should include the name of the parent layer. For example, if you have a Run state in the Base Layer, the name is Base Layer.Run.(https://docs.unity3d.com/ScriptReference/Animator.CrossFade.html)
        //PS: for field that default is float.NegativeInfinity, don't set the range using [Range(0, 1)]
        public ActionConfig_AnimatorState()
        {
            endValue = 1;//Default is one
        }

        public StatePlayType statePlayType = StatePlayType.Play;
        //PS: if the value needs to be set to float.NegativeInfinity in Inspector, just input "-Infinity " or "-1f / 0f"
        [Tooltip("The layer index (start with 0). If layer is -1, it plays the first state with the given state name or hash.")]
        public int layerIndex = -1;

        public string stateName;

        //Play
#if USE_NaughtyAttributes
        [AllowNesting]
        [ShowIf("statePlayType", StatePlayType.Play)]
#endif
        [Tooltip("The time offset between zero and one.")]
        public float normalizedTime = float.NegativeInfinity;//Set to 0 to Start from beginning even if you play the same state.

        //PlayInFixedTime
#if USE_NaughtyAttributes
        [AllowNesting]
        [ShowIf("statePlayType", StatePlayType.PlayInFixedTime)]
#endif
        [Tooltip("The time offset (in seconds).")]
        public float fixedTime = float.NegativeInfinity;


        //CrossFade
#if USE_NaughtyAttributes
        [AllowNesting]
        [ShowIf("statePlayType", StatePlayType.CrossFade)]
#endif
        [Range(0, 1)]
        [Tooltip("The duration of the transition (normalized).")]
        public float normalizedTransitionDuration;//Set this larger than 0 to smooth the transition
#if USE_NaughtyAttributes
        [AllowNesting]
        [ShowIf("statePlayType", StatePlayType.CrossFade)]
#endif
        [Tooltip("The time of the state (normalized).")]
        public float normalizedTimeOffset = float.NegativeInfinity;

        //CrossFadeInFixedTime
#if USE_NaughtyAttributes
        [AllowNesting]
        [ShowIf("statePlayType", StatePlayType.CrossFadeInFixedTime)]
#endif
        [Tooltip("The duration of the transition (in seconds).")]
        public float fixedTransitionDuration;
#if USE_NaughtyAttributes
        [AllowNesting]
        [ShowIf(nameof(statePlayType), StatePlayType.CrossFadeInFixedTime)]
#endif
        [Tooltip("The time of the state (in seconds).")]
        public float fixedTimeOffset = 0;

        //CrossFade || CrossFadeInFixedTime
#if USE_NaughtyAttributes
        [AllowNesting]
        [ShowIf(nameof(IsCrossFade))]
#endif
        [Range(0, 1)]
        [Tooltip("The time of the transition (normalized).")]
        public float normalizedTransitionTime = 0;

        #region NaughtAttribute
        bool IsCrossFade { get { return statePlayType == StatePlayType.CrossFade || statePlayType == StatePlayType.CrossFadeInFixedTime; } }

        public override float ScaleEndValue(float origin, object scale)
        {
            return FloatScaler.Instance.Scale(origin, scale);
        }
        #endregion
    }

    #region Define

    public enum StatePlayType
    {
        Play = 1,//normalized
        PlayInFixedTime,//in seconds
        CrossFade,
        CrossFadeInFixedTime
    }

    #endregion
}