using UnityEngine;
#if USE_JsonDotNet
using Newtonsoft.Json;
#endif
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
    /// 
    /// PS:
    /// -EndValue: Animator.speed
    /// </summary>
    public class SOAction_AnimatorState : SOActionWithConfigBase<ActionAnimatorRuntimeData, ActionConfig_AnimatorState, float, Animator>
    {
        ///ToDo:
        ///
        ///-写额外的AnimationModifier，并且自己编写ActionRuntimeData，来设置对应的值
        protected override void EnterExitFunc(ActionAnimatorRuntimeData runtimeData, bool isEnter)
        {
            var config = runtimeData.Config;
            var receiver = runtimeData.Receiver;

            if (config.LayerIndex > 0 && receiver.layerCount > config.LayerIndex)//layerIndex can be -1
            {
                Debug.LogError(receiver + " doesn't have Layerindex " + config.LayerIndex + " !");
                return;
            }

            int realLayerIndex = ActionAnimatorRuntimeData.GetRealLayerIndex(config.LayerIndex);
            if (!receiver.HasState(realLayerIndex, Animator.StringToHash(config.StateName)))
            {
                Debug.LogError(receiver + " doesn't have state " + config.StateName + " !");
                return;
            }

            ///ToAdd：
            ///-如果当前state为目标state，且已经播放完成，则重置其值，避免


            ///PS:
            ///-以下方法支持传入layerIndex值为-1
            switch (config.StatePlayType)
            {
                case StatePlayType.Play:
                    receiver.Play(config.StateName, config.LayerIndex, config.NormalizedTime); break;
                case StatePlayType.PlayInFixedTime:
                    receiver.PlayInFixedTime(config.StateName, config.LayerIndex, config.FixedTime); break;
                case StatePlayType.CrossFade:
                    receiver.CrossFade(config.StateName, config.NormalizedTransitionDuration, config.LayerIndex, config.NormalizedTimeOffset, config.NormalizedTransitionTime); break;
                case StatePlayType.CrossFadeInFixedTime:
                    receiver.CrossFadeInFixedTime(config.StateName, config.FixedTransitionDuration, config.LayerIndex, config.FixedTimeOffset, config.NormalizedTransitionTime); break;
            }


            receiver.speed = config.EndValue;//ToUpdate:研究改为获取指定stateinfo并修改其speed
            runtimeData.animator = receiver;
            runtimeData.beginAnimatorTransitionInfo = receiver.GetAnimatorTransitionInfo(ActionAnimatorRuntimeData.GetRealLayerIndex(config.LayerIndex));//记录初始的变换信息
        }
    }


    public class ActionAnimatorRuntimeData : ActionRuntimeData<ActionConfig_AnimatorState, float, Animator>
    {
        ///ToUpdate:
        ///+检查当前的AnimatorState是否完成(该类需要传入Animator等，还需要判断是否为空)
        ///-完成后要调用actOnComplete（可以尝试让SOActionBase统一调用EnterAsync，并在结束后调用actOnComplete而不是监听Tween实现）
        ///-保存并hash比较，可以提升性能
        ///-需要延后检测，因为Crossfade或其他状态，导致还未切换到目标stateName(需要检测当前是否正在切换到目标状态)
        ///
        ///ToAdd：
        ///-类似DoTween的delay等参数

        public Animator animator;
        public AnimatorTransitionInfo beginAnimatorTransitionInfo;//保存初始信息，方便对比
        public override StateProgress Progress
        {
            get
            {
                //if (stateProgress == StateProgress.Begin)//由SOActionBase启动：返回该初始状态(ToDelete：会导致卡住)
                //    stateProgress= StateProgress.Processing;
                if (animator)
                {
                    int realLayerIndex = GetRealLayerIndex(Config.LayerIndex);
                    AnimatorStateInfo curAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(realLayerIndex);
                    //Debug.LogError($"[Test]: {curAnimatorStateInfo.shortNameHash} + {curAnimatorStateInfo.normalizedTime}");

                    //string transName = $"{Config.StateName} -> {Config.StateName}";
                    //transName = "ColorToRed -> ColorToGreen";
                    //typeof(AnimatorTransitionInfo).GetField("")
                    //Debug.LogError($"Test: [{transName}] " + animatorTransitionInfo.IsUserName(transName));
                    if (animator.IsInTransition(realLayerIndex))//#1 先检查是否在Transition状态
                    {
                        AnimatorTransitionInfo animatorTransitionInfo = animator.GetAnimatorTransitionInfo(realLayerIndex);
                        if (animatorTransitionInfo.fullPathHash == beginAnimatorTransitionInfo.fullPathHash)//检查是否仍在从开始的State变换到目标State（通过检查fullPathHash是否有变化即可知）
                            stateProgress = StateProgress.Processing;
                        else//与初始状态不同：标记为已完成
                            stateProgress = StateProgress.Complete;
                    }
                    else if (curAnimatorStateInfo.IsName(Config.StateName))//Animator当前State就是目标State
                    {
                        if (curAnimatorStateInfo.normalizedTime >= 1)//PS:该值会一直增加，其中整数代表loop次数（The normalized time is a progression ratio. The integer part is the number of times the State has looped. The fractional part is a percentage (0-1) that represents the progress of the current loop.）
                        {
                            if (stateProgress == StateProgress.Begin)//如果是开始调用时已经完成该state，则返回Processing，让该动画正常重放
                                stateProgress = StateProgress.Processing;
                            else
                                stateProgress = StateProgress.Complete;
                        }
                        else
                            stateProgress = StateProgress.Processing;
                    }
                }
                else//Animator被销毁：标记为完成，方便退出Task
                    stateProgress = StateProgress.Complete;

                return stateProgress;
            }
        }

        /// <summary>
        /// Get layerIndex start with 0
        /// </summary>
        /// <param name="layerIndex"></param>
        /// <returns></returns>
        public static int GetRealLayerIndex(int layerIndex)
        {
            if (layerIndex < 0)//Warning：如果layerIndex小于0，则HasState/GetCurrentAnimatorStateInfo等会报警告
                layerIndex = 0;
            return layerIndex;
        }
    }

    /// <summary>
    /// 
    /// PS:
    /// -EndValue: When you specify a state name, or the string used to generate a hash, it should include the name of the parent layer. For example, if you have a Run state in the Base Layer, the name is Base Layer.Run.(https://docs.unity3d.com/ScriptReference/Animator.CrossFade.html)
    /// -for field that default is float.NegativeInfinity, don't set the range using [Range(0, 1)]
    /// </summary>
    [System.Serializable]
    public class ActionConfig_AnimatorState : ActionConfigBase<float>
    {
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public int LayerIndex { get { return layerIndex; } set { layerIndex = value; } }
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public string StateName { get { return stateName; } set { stateName = value; } }
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public StatePlayType StatePlayType { get { return statePlayType; } set { statePlayType = value; } }
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public float NormalizedTime { get { return normalizedTime; } set { normalizedTime = value; } }
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public float FixedTime { get { return fixedTime; } set { fixedTime = value; } }
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public float NormalizedTransitionDuration { get { return normalizedTransitionDuration; } set { normalizedTransitionDuration = value; } }
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public float NormalizedTimeOffset { get { return normalizedTimeOffset; } set { normalizedTimeOffset = value; } }
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public float FixedTransitionDuration { get { return fixedTransitionDuration; } set { fixedTransitionDuration = value; } }
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public float FixedTimeOffset { get { return fixedTimeOffset; } set { fixedTimeOffset = value; } }
#if USE_JsonDotNet
        [JsonIgnore]
#endif
        public float NormalizedTransitionTime { get { return normalizedTransitionTime; } set { normalizedTransitionTime = value; } }


        [Tooltip("The layer index (start with 0). If layer is -1, it plays the first state with the given state name or hash.")]
        [SerializeField] protected int layerIndex = 0;
        [SerializeField] protected string stateName;

        ///PS: 
        /// -For normalizedTime/fixedTime/normalizedTimeOffset:
        ///     -if the value is float.NegativeInfinity, the state will either be played from the start if it's not already playing, or will continue playing from its current time and no transition will happen.
        /// -if the value needs to be set to float.NegativeInfinity in Inspector, just input "-Infinity" or "-1f / 0f"
        ///     -Set to 0 to Start from beginning even if you play the same state.
        [SerializeField] protected StatePlayType statePlayType = StatePlayType.Play;

#if USE_NaughtyAttributes
        [AllowNesting]
        [ShowIf(nameof(StatePlayType), StatePlayType.Play)]
#endif
        [Tooltip("The time offset between zero and one.")]
        [SerializeField] protected float normalizedTime = 0;//Can be NegativeInfinity

#if USE_NaughtyAttributes
        [AllowNesting]
        [ShowIf(nameof(StatePlayType), StatePlayType.PlayInFixedTime)]
#endif
        [Tooltip("The time offset (in seconds).")]
        [SerializeField] protected float fixedTime = 0;//Can be NegativeInfinity

#if USE_NaughtyAttributes
        [AllowNesting]
        [ShowIf(nameof(StatePlayType), StatePlayType.CrossFade)]
#endif
        [Range(0, 1)]
        [Tooltip("The duration of the transition (normalized).")]
        [SerializeField] protected float normalizedTransitionDuration;//Set this larger than 0 to smooth the transition
#if USE_NaughtyAttributes
        [AllowNesting]
        [ShowIf(nameof(StatePlayType), StatePlayType.CrossFade)]
#endif
        [Tooltip("The time of the state (normalized).")]
        [SerializeField] protected float normalizedTimeOffset = 0;//Can be NegativeInfinity

#if USE_NaughtyAttributes
        [AllowNesting]
        [ShowIf("statePlayType", StatePlayType.CrossFadeInFixedTime)]
#endif
        [Tooltip("The duration of the transition (in seconds).")]
        [SerializeField] protected float fixedTransitionDuration;
#if USE_NaughtyAttributes
        [AllowNesting]
        [ShowIf(nameof(StatePlayType), StatePlayType.CrossFadeInFixedTime)]
#endif
        [Tooltip("The time of the state (in seconds).")]
        [SerializeField] protected float fixedTimeOffset = 0;

        //CrossFade || CrossFadeInFixedTime
#if USE_NaughtyAttributes
        [AllowNesting]
        [ShowIf(nameof(IsCrossFade))]
#endif
        [Range(0, 1)]
        [Tooltip("The time of the transition (normalized).")]
        [SerializeField] protected float normalizedTransitionTime = 0;

        public ActionConfig_AnimatorState()
        {
            endValue = 1;//endValue (speed) default is one
        }

        #region NaughtAttribute
        bool IsCrossFade { get { return StatePlayType == StatePlayType.CrossFade || StatePlayType == StatePlayType.CrossFadeInFixedTime; } }

        protected override float ModifyEndValue(float origin, object scale)
        {
            return FloatScaler.Scale(origin, scale);
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