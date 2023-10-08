using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Threeyes.Data;
using UnityEngine;
using UnityEngine.Events;

namespace Threeyes.Persistent
{
    /// <summary>
    /// 适用于需要RuntimeEditor的复杂类
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <typeparam name="TEvent"></typeparam>
    /// <typeparam name="TDataOption"></typeparam>
    public abstract class PersistentDataComplexBase<TValue, TEvent, TDataOption> : PersistentDataBase<TValue, TEvent, TDataOption>, IPersistentData_ComplexData<TValue>
        where TEvent : UnityEvent<TValue>
        where TDataOption : IDataOption
    {
        public override Type ValueType { get { return TargetValue != null ? TargetValue.GetType() : null; } }

        public virtual FilePathModifier FilePathModifier { get { if (filePathModifier_PD == null) filePathModifier_PD = new FilePathModifier_PD(this); return filePathModifier_PD; } set { Debug.LogError("This property can't set!"); /*暂时不允许设置，避免用户魔改*/} }
        private FilePathModifier_PD filePathModifier_PD;

        public override TValue ValueToSaved { get { return TargetValue; } }//直接存储TargetValue，确保即使不通过UIField_XX修改该值，也能正常保存字段
        public abstract TValue TargetValue { get; set; }

        public override void Init()
        {
            base.Init();

            //使用默认的TargetValue作为DefaultValue，便于Controller.Delete时重置
            if (defaultValue == null && TargetValue != null)
            {
                defaultValue = CloneInst(TargetValue);//PS:与cacheTargetValueClone不能是同一个引用，否则会导致重置时被修改
            }
        }
        /// <summary>
        /// 调用时机：
        /// #1: Load/Set/EditorReset
        /// #2: PersistentControllerBase.NotifyValueChanged
        /// #3: UIObjectModifierManager.OnUIInspectorElementAfterValueChanged (任意子成员的值更改都会调用全局刷新)
        /// Warning：因为不一定通过RunEdit调用（如初始化），因此加载方法不能放到那个类中
        /// </summary>
        /// <param name="value"></param>
        public override void OnValueChanged(TValue value, PersistentChangeState persistentChangeState)
        {
            try
            {
                PersistentObjectTool.CopyFiledsAndLoadAsset(TargetValue, value, persistentChangeState, PersistentDirPath);

                base.OnValueChanged(TargetValue, persistentChangeState);//通知外界Event
            }
            catch (Exception e)
            {
                Debug.LogError($"{Key} OnValueChanged with error:\r\n" + e);
            }
        }

        protected virtual TValue CloneInst(TValue origin) { return /*ReflectionTool.DeepCopy(origin);*/UnityObjectTool.DeepCopy(origin); }
    }
}