using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace Threeyes.Core
{
    /// <summary>
    /// Using reflection to call methods
    /// 
    /// </summary>
    public abstract class ReflectionMethodHolderBase : ReflectionMemberHolderBase
    {
        public string TargetSerializeMethodName { get { return targetSerializeMethodName; } set { targetSerializeMethodName = value; } }
        [SerializeField] protected string targetSerializeMethodName;

        public abstract bool IsDesireMethod(MethodInfo methodInfo);
        protected MethodInfo targetMethodInfo { get { return GetMemberInfo((type, name, bf) => type.GetMethods(bf).FirstOrDefault(mI => mI.Name == name && IsDesireMethod(mI)), targetSerializeMethodName); } }


        /// <summary>
        /// Invoke target method
        /// </summary>
        public void InvokeTarget()
        {
            TryInvokeMethod(InvokeTargetMethodFunc);
        }

        protected void TryInvokeMethod(UnityAction action)
        {
            if (targetMethodInfo == null)
            {
                if (!target || targetSerializeMethodName == emptyMemberName)//有部分信息无效
                    Debug.LogError("Please set all necessary member information first!");
                else
                    Debug.LogError($"Can't find target method {targetSerializeMethodName} on Object {target}!");
            }
            else
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    Debug.LogError("Invoke target method with error: \r\n" + e);
                }
            }
        }

        protected virtual void InvokeTargetMethodFunc()
        {
            targetMethodInfo.Invoke(Target, null);
        }
    }


    /// <summary>
    /// Method with specified parameters
    /// 
    /// Todo:
    /// -带参子类可选择传入指定参数，或者带参调用
    /// -能够绑定0~1个参数（可以拆分为不同子类），外部能够通过唯一方法调用（避免uMod的重名绑定bug）
    /// -增加一个可以反射带任意类型参数的方法的特殊类。因为Inspector可能不支持绘制Value类型，所以只能通过代码InvokeTargetWithParam调用，
    /// -参考UnityEngine.Events.PersistentCall，优化结构（主要是缓存类名）
    /// </summary>
    /// <typeparam name="TValue">Param value</typeparam>
    public abstract class ReflectionMethodHolderBase<TValue> : ReflectionMethodHolderBase
    {
        public virtual Type ParamType { get { return typeof(TValue); } }
        public virtual TValue Value { get { return value; } set { this.value = value; } }
        [SerializeField] protected TValue value = default(TValue);

        public override bool IsDesireMethod(MethodInfo methodInfo)
        {
            //Set Method format: void Method(TValue);//无返回值，仅一个参数
            ParameterInfo[] parameterInfos = methodInfo.GetParameters();
            return parameterInfos.Length == 1 && IsParamTypeMatch(parameterInfos[0].ParameterType) && methodInfo.ReturnType == typeof(void);
        }
        public virtual bool IsParamTypeMatch(Type typeInput)
        {
            return Equals(typeInput, ParamType);
        }

        protected override void InvokeTargetMethodFunc()
        {
            InvokeTargetWithParam(Value);
        }
        public virtual void InvokeTargetWithParam(TValue param)
        {
            TryInvokeMethod(() => targetMethodInfo.Invoke(Target, new object[] { param }));
        }
    }
}