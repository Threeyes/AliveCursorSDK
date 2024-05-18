using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Text;
using Threeyes.Core;
#if USE_JsonDotNet
using Newtonsoft.Json;
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
    /// <typeparam name="TActionReceiver">(If the class don't need receiver, you can use Transform instead)</typeparam>
    public abstract class SOActionWithConfigBase<TActionRuntimeData, TActionConfig, TValue, TActionReceiver> : SOActionBase
        where TActionRuntimeData : ActionRuntimeData<TActionConfig, TValue, TActionReceiver>, new()
        where TActionConfig : ActionConfigBase<TValue>
        //where TActionReceiver : Component
    {
        public override Type ReceiverType { get { return typeof(TActionReceiver); } }
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


        //——Convert ActionRuntimeData into specify TActionRuntimeData——
        protected override ActionRuntimeData CreateRuntimeData(ActionState actionState, object receiver, ObjectID objectID, object value, UnityAction actOnComplete, ActionModifierSettings modifierSettings)
        {
            TActionRuntimeData actionRuntimeData = new TActionRuntimeData();//使用具体数据类进行初始化

            //#1 Clone and Modify the origin config
            //actionRuntimeData.Config = targetConfig;
            TActionConfig targetConfig = (actionState == ActionState.Enter) ? enterConfig : exitConfig;//Init config(需要优先设置，否则Init中的ApplyModifiers会报错)
            //#警告：该Clone可能导致内存溢出，需要测试(可能原因是直接复制AnimationCurve导致引用相同)（解决办法是直接用原类实例创建，参考ActionConfig_TweenBase的Clone重载）
            var configClone = targetConfig.Clone<TActionConfig>();//改为深度克隆原Config，避免影响SO中的原值，且方便Modifier直接修改而不是将目标值缓存在RuntimeData（如EndValue）
            configClone.ApplyModifierSettings(actionState, value, modifierSettings);
            actionRuntimeData.Config = configClone;

            actionRuntimeData.Init(actionState, receiver, objectID, value, actOnComplete);
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

        //——SubClass should override there functions instead——    
        protected virtual void EnterFunc(TActionRuntimeData runtimeData)
        {
            if (runtimeData.Receiver == null)//Check for receiver
            {
                Debug.LogError($"Try to play Action {this.name} but doesn't have receiver with type: [{typeof(TActionReceiver)}]!");
                return;
            }
            EnterExitFunc(runtimeData, true);
        }
        protected virtual void ExitFunc(TActionRuntimeData runtimeData)
        {
            //Check for receiver
            if (runtimeData.Receiver == null)
            {
                Debug.LogError($"Try to play Action {this.name} but doesn't have receiver with type: [{typeof(TActionReceiver)}]!");
                return;
            }
            EnterExitFunc(runtimeData, false);
        }
        protected abstract void EnterExitFunc(TActionRuntimeData runtimeData, bool isEnter);

        #region Save Config 
        //        //#Saved Config(Cache preset)【ToDelete】Unity已经在Inspector提供直接复制字段的方法，可以保留savedEnterConfig让用户自行复制
        //        //PS: [JsonIgnore]对[SerializeField]无效，因此只能改为public以便编辑，【该字段仅编辑器有效】。
        //#if USE_NaughtyAttributes
        //        [ReadOnly]
        //#endif
        //#if USE_JsonDotNet
        //        [JsonIgnore]
        //#endif
        //        public TActionConfig savedEnterConfig = default(TActionConfig);
        //#if USE_NaughtyAttributes
        //        [ReadOnly]
        //#endif
        //#if USE_JsonDotNet
        //        [JsonIgnore]
        //#endif
        //        public TActionConfig savedExitConfig = default(TActionConfig);

        //        [ContextMenu("SaveConfig")]
        //#if USE_NaughtyAttributes
        //        [Button(enabledMode: EButtonEnableMode.Always)]
        //#endif
        //        public override void SaveConfig()
        //        {
        //            savedEnterConfig = enterConfig.Clone<TActionConfig>();
        //            savedExitConfig = exitConfig.Clone<TActionConfig>();

        //#if UNITY_EDITOR
        //            UnityEditor.EditorUtility.SetDirty(this);//！需要调用该方法保存更改
        //#endif
        //            Debug.Log(name + " Save Config Completed");
        //        }
        //        [ContextMenu("LoadConfig")]
        //#if USE_NaughtyAttributes
        //        [Button(enabledMode: EButtonEnableMode.Always)]
        //#endif
        //        public override void LoadConfig()
        //        {
        //            enterConfig = savedEnterConfig.Clone<TActionConfig>();
        //            exitConfig = savedExitConfig.Clone<TActionConfig>();

        //#if UNITY_EDITOR
        //            UnityEditor.EditorUtility.SetDirty(this);//！需要调用该方法保存更改
        //#endif
        //            Debug.Log(name + " Load Config Completed");
        //        }

        //#可用于相同类型的SO进行数据交换，或者在对SO进行大改之间进行保存
        //#ToAdd:针对通用父类ActionConfig_TweenBase增加Cache,方便Copy/Paste时复制父类相同的数据（可以是复制时把数据缓存为ActionConfig_TweenBase,然后通过反射等拷贝相同的数据）
        static TActionConfig configOnEnterCache = null;
        static TActionConfig configOnExitCache = null;


        //暂时只能存储不同的Config
        [ContextMenu("CopyConfig")]
#if USE_NaughtyAttributes
        [Button(enabledMode: EButtonEnableMode.Always)]
#endif
        public virtual void CopyConfig()
        {
            configOnEnterCache = enterConfig.Clone<TActionConfig>();
            configOnExitCache = exitConfig.Clone<TActionConfig>();
        }

        [ContextMenu("PasteConfig")]
#if USE_NaughtyAttributes
        [Button(enabledMode: EButtonEnableMode.Always)]
#endif
        public virtual void PasteConfig()
        {
            //ToDo:支持仅拷贝Tween共同的Config
            if (configOnEnterCache != null && configOnExitCache != null)
            {
                enterConfig = configOnEnterCache.Clone<TActionConfig>();
                exitConfig = configOnExitCache.Clone<TActionConfig>();

#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);//！需要调用该方法保存更改
#endif
            }
            else
            {
                Debug.LogError("You should copy other config first!");
            }
        }
        #endregion

        #region Editor Method
#if UNITY_EDITOR
        public override void SetInspectorGUICommonTextArea(StringBuilder sB, GameObject target, string id = "")
        {
            base.SetInspectorGUICommonTextArea(sB, target);

            if (target && !target.TryGetComponent(out TActionReceiver tReturn))////Warning:不要直接使用GetComponent，因为如果TReturn为Component，其内部会通过CastHelper<T>返回对应类型实例的Default值而不是null而导致意外赋值给tReturn
            {
                sB.AppendWarningRichText("Action require Component [" + typeof(TActionReceiver) + "] on target!");
                sB.Append("\r\n");
            }
        }
#endif
        #endregion
    }
}
