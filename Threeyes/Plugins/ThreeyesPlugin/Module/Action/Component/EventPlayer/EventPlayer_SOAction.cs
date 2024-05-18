using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;
using Threeyes.Core;
using Threeyes.EventPlayer;
using System.Threading.Tasks;
using UnityEngine.Events;
#if USE_NaughtyAttributes
using NaughtyAttributes;
#endif
#if UNITY_EDITOR
using Threeyes.Core.Editor;
#endif
namespace Threeyes.Action
{
    /// <summary>
    /// Common Action EP
    /// 
    /// PS:
    /// -The param here will be SoAction's param, use this to modify the output value
    /// -value can't be drawn on Inspector becauce it is Object type, you have to call it via Script.
    /// </summary>
   	[AddComponentMenu(EditorDefinition_Action.AssetMenuPrefix_Action_EventPlayer + "EventPlayer_SOAction")]
    public class EventPlayer_SOAction : EventPlayerWithParamBase<EventPlayer_SOAction, ObjectEvent, object>
    {
        #region Property & Field

        public override string ValueToString { get { return/* SOAction != null ? SOAction.name :*/ ""; } }//ToAdd【V2】:显示Action的属性（如TweenTransform(Rot)）
        public GameObject GOTarget
        {
            get
            {
                if (!goTarget)
                    goTarget = gameObject;
                return goTarget;
            }
            set { goTarget = value; }
        }

        public SOActionBase SOAction { get { return soAction; } set { soAction = value; } }

        public UnityEvent onPlayComplete = new UnityEvent();
        public UnityEvent onStopComplete = new UnityEvent();
        [SerializeField] protected GameObject goTarget;

#if USE_NaughtyAttributes
        [Expandable]
#endif
        [SerializeField]
        [Tooltip("Event will work without soAction.")]
        protected SOActionBase soAction;

        #endregion

        #region Async (PS：为了方便后续提取通用基类，暂public方法统一返回通用的Task)（仅供脚本调用）
        ///ToAdd:
        ///-增加Play/StopAsync完成的事件，如果是上方的PlayFunc/StopFunc则通过SOAction.Enter的actOnComplete调用
        ///PS：
        ///-以下方法因为带Task返回值所以不能通过UnityEvent调用，仅供代码调用。有需要直接调用上面不带Async的方法
        public async Task PlayAsync(bool isPlay)
        {
            await PlayWithParamAsync(isPlay, isPlay ? IsPlayWithParam : IsStopWithParam, Value);
        }
        public async Task PlayWithParamAsync(object value)
        {
            await PlayWithParamAsync(true, true, value);
        }
        public async Task StopWithParamAsync(object value)
        {
            await PlayWithParamAsync(false, true, value);
        }
        /// <summary>
        /// Play using the exist value
        /// </summary>
        /// <param name="isPlay"></param>
        /// <param name="isPlayStopWithParam"></param>
        public async Task PlayWithParamAsync(bool isPlay, object value)
        {
            await PlayWithParamAsync(isPlay, true, value);
        }

        public async Task PlayWithParamAsync(bool isPlay, bool isPlayStopWithParam, object value)
        {
            //PS:All the public play-relate method will call this method
            if (!IsActive)
                return;

            if (isPlay && !IsReverse || !isPlay && IsReverse)//Actual Play
            {
                if (IsPlayOnce && isPlayed || !CanPlay)
                    return;

                if (isPlayStopWithParam)
                {
                    if (!(IsDetectMatch && !CompareValue(value, TargetValue)))
                        await PlayWithParamAsyncFunc(value);
                }
                else
                    await PlayAsyncFunc();
            }
            else if (!isPlay && !IsReverse || isPlay && IsReverse)//Actual Stop
            {
                if (!CanStop)
                    return;

                if (isPlayStopWithParam)
                {
                    if (!(IsDetectMatch && !CompareValue(value, TargetValue)))
                        await StopWithParamAsyncFunc(value);
                }
                else
                    await StopAsyncFunc();
            }
        }

        protected virtual async Task<ActionRuntimeData> PlayWithParamAsyncFunc(object value)
        {
            Task<ActionRuntimeData> task = null;
            if (SOAction)
                task = SOAction.EnterAsync(true, GOTarget, value, FirePlayCompleteEvent, ActionModifierSettings);//在调用base.PlayWithParamFunc之前就需要执行Enter，确保与非Async方法顺序相同
            base.PlayWithParamFunc(value);

            if (task != null)
                return await task;
            return null;
        }
        protected virtual async Task<ActionRuntimeData> StopWithParamAsyncFunc(object value)
        {
            Task<ActionRuntimeData> task = null;
            if (SOAction)
                task = SOAction.EnterAsync(false, GOTarget, value, FireStopCompleteEvent, ActionModifierSettings);
            base.StopWithParamFunc(value);

            if (task != null)
                return await task;
            return null;
        }
        protected virtual async Task<ActionRuntimeData> PlayAsyncFunc()
        {
            Task<ActionRuntimeData> task = null;
            if (SOAction)
                task = SOAction.EnterAsync(true, GOTarget, actOnComplete: FirePlayCompleteEvent,modifierSettings: ActionModifierSettings);//在调用base.PlayWithParamFunc之前开始，确保与非Async顺序相同
            base.PlayFunc();

            if (task != null)
                return await task;
            return null;
        }
        protected virtual async Task<ActionRuntimeData> StopAsyncFunc()
        {
            Task<ActionRuntimeData> task = null;
            if (SOAction)
                task = SOAction.EnterAsync(false, GOTarget, actOnComplete: FireStopCompleteEvent,modifierSettings: ActionModifierSettings);//在调用base.PlayWithParamFunc之前开始，确保与非Async顺序相同
            base.StopFunc();

            if (task != null)
                return await task;
            return null;
        }
        #endregion

        #region Inner Method
        protected override void PlayFunc()
        {
            SOAction?.Enter(true, GOTarget, actOnComplete: FirePlayCompleteEvent, modifierSettings: ActionModifierSettings);//PS：Get all IActionModifier from this gameobject
            base.PlayFunc();
        }
        protected override void StopFunc()
        {
            SOAction?.Enter(false, GOTarget, actOnComplete: FireStopCompleteEvent, modifierSettings: ActionModifierSettings);
            base.StopFunc();
        }

        protected override void PlayWithParamFunc(object value)
        {
            SOAction?.Enter(true, GOTarget, value, FirePlayCompleteEvent, ActionModifierSettings);//Set SOAction's value
            base.PlayWithParamFunc(value);
        }

        protected override void StopWithParamFunc(object value)
        {
            SOAction?.Enter(false, GOTarget, value, FireStopCompleteEvent, ActionModifierSettings);
            base.StopWithParamFunc(value);
        }
        #endregion

        #region Utility
        void FirePlayCompleteEvent()
        {
            onPlayComplete.Invoke();
        }
        void FireStopCompleteEvent()
        {
            onStopComplete.Invoke();
        }

        ActionModifierSettings ActionModifierSettings { get { return new ActionModifierSettings(ListActionModifier); } }
        /// <summary>
        /// Get all modifiers from this gameobject
        /// 
        /// ToUpdate：
        /// -增加枚举，可以选择是从EP物体（好处是分别控制，不会影响主物体）还是从GOTarget获取，或者是自行链接对应的Modifier组件
        /// </summary>
        List<IActionModifier> ListActionModifier
        {
            get
            {
                return GetComponents<IActionModifier>().ToList();
            }
        }
        #endregion

        #region Editor Method
#if UNITY_EDITOR
        //public override bool IsCustomInspector => false;
        //——MenuItem——
        protected const int intActionMenuOrder = 500;
        static string instName = "ActionEP ";
        [UnityEditor.MenuItem(strMenuItem_Root_Extend + "Action/EventPlayer", false, intActionMenuOrder + 0)]
        public static void CreateActionEventPlayer()
        {
            EditorTool.CreateGameObjectAsChild<EventPlayer_SOAction>(instName);
        }

        public override string ShortTypeName { get { return "Act"; } }

        //——Inspector GUI——
        public override void SetInspectorGUIUnityEventProperty(GUIPropertyGroup group)
        {
            base.SetInspectorGUIUnityEventProperty(group);
            group.listProperty.Add(new GUIProperty(nameof(onPlayComplete)));
            group.listProperty.Add(new GUIProperty(nameof(onStopComplete)));
        }
        public override void SetInspectorGUISubProperty(GUIPropertyGroup group)
        {
            base.SetInspectorGUISubProperty(group);
            group.listProperty.Add(new GUIProperty(nameof(goTarget), "Target"));
            group.listProperty.Add(new GUIProperty(nameof(soAction), "Action"));
        }

        public override void SetInspectorGUICommonTextArea(StringBuilder sB)
        {
            base.SetInspectorGUICommonTextArea(sB);
            if (SOAction)
                SOAction.SetInspectorGUICommonTextArea(sB, GOTarget);//让Action判断传入类型是否有效
        }
#endif
        #endregion
    }
}
