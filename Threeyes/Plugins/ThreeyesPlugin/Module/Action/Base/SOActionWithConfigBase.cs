using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Text;
#if USE_JsonDotNet
using Newtonsoft.Json;
using Threeyes.Core;
#endif
#if USE_NaughtyAttributes
using NaughtyAttributes;
#endif

namespace Threeyes.Action
{
    /// Design Rule：
    /// 1. 如果一个参数影响整个Action的生命周期或者需要保证Enter/Exit都是调用相同的方法（如SOAction_TweenTransform.transformType），那应该声明在此类的继承类的[Common Setting]下；如果一个参数只影响Enter/Exit某一阶段，那就声明在Config中。
    /// 也就是说,单个Action的Enter/Exit针对同一作用物的某个属性（如Material的同个贴图的Tilling，或者Transform的Position），其中[Common Setting]用于指定该属性（Enter/Exit共用），TActionConfig为该属性在不同状态下的行为
    /// </summary>
    /// <typeparam name="TActionConfig"></typeparam>

    /// <summary>
    /// Action with config
    /// </summary>
    /// <typeparam name="TActionRuntimeData"></typeparam>
    /// <typeparam name="TActionConfig"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TActionReceiver">(If the class don't need receiver, use Transform instead)</typeparam>
    public abstract class SOActionWithConfigBase<TActionRuntimeData, TActionConfig, TValue, TActionReceiver> : SOActionBase
        where TActionRuntimeData : ActionRuntimeData<TActionConfig, TValue, TActionReceiver>, new()
        where TActionConfig : ActionConfigBase<TValue>
        //where TActionReceiver : Component
    {
        public virtual IActionConfig EnterConfig { get { return enterConfig; } }
        public virtual IActionConfig ExitConfig { get { return exitConfig; } }

        protected const string headerCommonSetting = "[Common Setting]";//用于[Header]

        //（PS:本来是用[BoxGroup[和[Foldout]进行分组，但是因为他们不能在NestClass中使用，所以弃用。改为不显示）（https://dbrizov.github.io/na-docs/attributes/meta_attributes/foldout.html）
#if USE_NaughtyAttributes
        [EnableIf(nameof(IsActiveEnter))]
#endif
        [SerializeField] protected TActionConfig enterConfig = default(TActionConfig);

#if USE_NaughtyAttributes
        [EnableIf(nameof(IsActiveExit))]
#endif
        [SerializeField] protected TActionConfig exitConfig = default(TActionConfig);

        //Saved Config(Cache preset)
        //PS: [JsonIgnore]对[SerializeField]无效，因此只能改为public以便编辑，【该字段仅编辑器有效】。
#if USE_NaughtyAttributes
        [ReadOnly]
#endif
        [JsonIgnore] public TActionConfig savedEnterConfig = default(TActionConfig);
#if USE_NaughtyAttributes
        [ReadOnly]
#endif
        [JsonIgnore] public TActionConfig savedExitConfig = default(TActionConfig);

        //——Convert ActionRuntimeData into specify TActionRuntimeData——
        protected override ActionRuntimeData CreateRuntimeData(ActionState actionState, GameObject target, object value, UnityAction actOnComplete, List<IActionModifier> listModifier, string id)
        {
            TActionRuntimeData actionRuntimeData = new TActionRuntimeData();
            actionRuntimeData.Init(actionState, target, value, actOnComplete, listModifier, id);
            actionRuntimeData.config = (actionState == ActionState.Enter) ? enterConfig : exitConfig;//Init config
            return actionRuntimeData;
        }
        protected override void EnterFunc(ActionRuntimeData runtimeData)
        {
            EnterFunc(runtimeData as TActionRuntimeData);
        }
        protected override void ExitFunc(ActionRuntimeData runtimeData)
        {
            EnterFunc(runtimeData as TActionRuntimeData);
        }

        //——SubClass should use there functions instead——
        //Check for receiver
        protected virtual void EnterFunc(TActionRuntimeData runtimeData)
        {
            if (runtimeData.Receiver == null)
            {
                Debug.LogError(runtimeData.target + " doesn't have compoent: " + typeof(TActionReceiver) + "!");
                return;
            }
            EnterExitFunc(runtimeData);
        }
        protected virtual void ExitFunc(TActionRuntimeData runtimeData)
        {
            //Check for receiver
            if (runtimeData.Receiver == null)
            {
                Debug.LogError(runtimeData.target + " doesn't have compoent: " + typeof(TActionReceiver) + "!");
                return;
            }
            EnterExitFunc(runtimeData);
        }
        protected abstract void EnterExitFunc(TActionRuntimeData runtimeData);

        #region Config
        [ContextMenu("SaveConfig")]
#if USE_NaughtyAttributes
        [Button(enabledMode: EButtonEnableMode.Always)]
#endif
        public override void SaveConfig()
        {
            savedEnterConfig = enterConfig.Clone<TActionConfig>();
            savedExitConfig = exitConfig.Clone<TActionConfig>();

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);//！需要调用该方法保存更改
#endif
            Debug.Log(name + " Save Config Completed");
        }

        [ContextMenu("LoadConfig")]
#if USE_NaughtyAttributes
        [Button(enabledMode: EButtonEnableMode.Always)]
#endif
        public override void LoadConfig()
        {
            enterConfig = savedEnterConfig.Clone<TActionConfig>();
            exitConfig = savedExitConfig.Clone<TActionConfig>();

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);//！需要调用该方法保存更改
#endif

            Debug.Log(name + " Load Config Completed");
        }

        //针对对应子类
        static TActionConfig configOnEnterCache = null;
        static TActionConfig configOnExitCache = null;

        //ToAdd:针对通用父类ActionConfig_TweenBase增加Cache,方便 Copy/Paste时复制相同父类的数据

        //暂时只能存储不同的Config
        [ContextMenu("CopyConfig")]
#if USE_NaughtyAttributes
        [Button(enabledMode: EButtonEnableMode.Always)]
#endif
        public virtual void CopyData()
        {
            configOnEnterCache = enterConfig.Clone<TActionConfig>();
            configOnExitCache = exitConfig.Clone<TActionConfig>();
        }

        [ContextMenu("PasteConfig")]
#if USE_NaughtyAttributes
        [Button(enabledMode: EButtonEnableMode.Always)]
#endif
        public virtual void PasteData()
        {
            //ToDo:支持仅拷贝Tween共同的Config
            if (configOnEnterCache != null && configOnExitCache != null)
            {
                enterConfig = configOnEnterCache.Clone<TActionConfig>();
                exitConfig = configOnExitCache.Clone<TActionConfig>();
            }
            else
            {
                Debug.LogError("You should copy other config first!");
            }
        }

        //ToDelete
        //public override void CopyData(SOBase other)
        //{
        //    if (other.GetType() == this.GetType())
        //    {
        //        var soInst = other as SOActionWithConfigBase<TActionRuntimeData, TActionConfig, TValue, TActionReceiver>;
        //        soInst.CopyData();
        //        this.PasteData();
        //    }
        //}
        #endregion


        #region Editor Method
#if UNITY_EDITOR

        public override void SetInspectorGUICommonTextArea(StringBuilder sB, GameObject target, string id = "")
        {
            base.SetInspectorGUICommonTextArea(sB, target);
            if (target && target.GetComponent<TActionReceiver>() == null)
            {
                sB.AppendWarningRichText("Action require Component [" + typeof(TActionReceiver) + "] on target!");
                sB.Append("\r\n");
            }
        }

#endif
        #endregion
    }
}
