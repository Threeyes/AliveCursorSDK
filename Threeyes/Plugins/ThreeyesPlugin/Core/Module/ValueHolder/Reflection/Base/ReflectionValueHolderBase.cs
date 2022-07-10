using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Linq;
namespace Threeyes.ValueHolder
{
    /// <summary>
    /// 通过反射设置实例的某个字段/属性/方法（适用于在UnityEvent中无法显示或对应字段是私有的情况）
    /// </summary>
    public abstract class ReflectionValueHolderBase : MonoBehaviour
    {
        public abstract Type ValueType { get; }

        /// <summary>
        /// Object in Scene or Asset window
        /// </summary>
        public UnityEngine.Object target;

        //对应序列化的信息（ReadOnly）(PS:不能修改修饰符，因为Inspector脚本需要使用）
        [HideInInspector] public string serializeFieldName;
        [HideInInspector] public string serializePropertyName;
        [HideInInspector] public string serializeGetMethodName;
        [HideInInspector] public string serializeSetMethodName;

        //——反射查找时的调用方法——
        /// <summary>
        /// 检查Member的类型是否匹配
        /// </summary>
        /// <param name="typeInput"></param>
        /// <returns></returns>
        public virtual bool IsTypeMatch(Type typeInput)
        {
            return typeInput.Equals(ValueType);
        }
        public bool IsDesireGetMethod(MethodInfo methodInfo)
        {
            //Get Method format: TValue Method();//应无参返回对应类型的参数
            ParameterInfo[] parameterInfos = methodInfo.GetParameters();
            return IsTypeMatch(methodInfo.ReturnType) && parameterInfos.Length == 0;
        }
        public bool IsDesireSetMethod(MethodInfo methodInfo)
        {
            //Set Method format: AnyReturnValue Method(TValue);//仅一个参数，返回值不限
            ParameterInfo[] parameterInfos = methodInfo.GetParameters();
            return parameterInfos.Length == 1 && IsTypeMatch(parameterInfos[0].ParameterType);
        }

        #region Define
        public static string emptyMemberName = "___";//占位，代表不选，用于EditorGUI
        public const BindingFlags defaultBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        #endregion
    }
    public class ReflectionValueHolder<TValue> : ReflectionValueHolderBase, IValueHolder<TValue>
    {
        //统一管理
        public TValue CurValue
        {
            set
            {
                FieldCurValue = value;
                PropertyCurValue = value;
                MethodCurValue = value;
            }
            get
            {
                //Warning：get返回的值无效，开发者应该自行调用对应的属性
                Debug.LogError("The get method is not available! Use the specify property instead!");
                return default(TValue);
            }
        }

        public TValue FieldCurValue
        {
            get { return GetMember(fieldInfo, (mI, inst) => mI.GetValue(inst)); }
            set { SetMember(fieldInfo, (mI, inst, val) => mI.SetValue(inst, val), value); }
        }

        FieldInfo fieldInfo { get { return GetMemberInfo((type, name, bf) => type.GetField(name, bf), serializeFieldName); } }
        public TValue PropertyCurValue
        {
            get { return GetMember(propertyInfo, (mI, inst) => mI.GetValue(inst), (mI) => mI.CanRead); }
            set { SetMember(propertyInfo, (mI, inst, val) => mI.SetValue(inst, val), value, (mI) => mI.CanWrite); }
        }
        public PropertyInfo propertyInfo { get { return GetMemberInfo((type, name, bf) => type.GetProperty(name, bf), serializePropertyName); } }

        public TValue MethodCurValue
        {
            //PS:指明方法的特征，否则可能会报错：Ambiguous match found
            get { return GetMember(GetMemberInfo((type, name, bf) => type.GetMethods(bf).FirstOrDefault(mI => mI.Name == name && IsDesireGetMethod(mI)), serializeGetMethodName), (mI, inst) => mI.Invoke(inst, new object[] { })); }
            set { SetMember(GetMemberInfo((type, name, bf) => type.GetMethods(bf).FirstOrDefault(mI => mI.Name == name && IsDesireSetMethod(mI)), serializeSetMethodName), (mI, inst, val) => mI.Invoke(inst, new object[] { val }), value); }
        }

        public override Type ValueType { get { return typeof(TValue); } }
        public Type TargetType { get { return target ? target.GetType() : null; } }


        //Todo:将GetMenberInfo弄成链式调用，而不是包含
        public TValue GetMember<TMemberInfo>(TMemberInfo memberInfo, Func<TMemberInfo, object, object> actGetValue, Func<TMemberInfo, bool> actCheckIfCanGet = null)
            where TMemberInfo : MemberInfo
        {
            if (memberInfo != null)
            {
                bool canGet = actCheckIfCanGet != null ? actCheckIfCanGet(memberInfo) : true;//默认代表可写
                if (canGet)
                    return (TValue)actGetValue(memberInfo, target);
            }
            return default(TValue);
        }

        public bool SetMember<TMemberInfo>(TMemberInfo memberInfo, UnityAction<TMemberInfo, object, object>
        actSetValue, object value, Func<TMemberInfo, bool> actCheckIfCanSet = null)
            where TMemberInfo : MemberInfo
        {
            if (memberInfo != null)
            {
                bool canSet = actCheckIfCanSet != null ? actCheckIfCanSet(memberInfo) : true;//默认代表可写
                if (canSet)
                {
                    if (IsSetValueValid(memberInfo, value))
                    {
                        try
                        {
                            actSetValue(memberInfo, target, value);
                            return true;
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("SetMember with error: \r\n" + e);
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 在SetValue时判断是否有效
        /// （eg：如果需要排除值为null的情况，可以覆写该方法）
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected virtual bool IsSetValueValid(MemberInfo memberInfo, object value) { return true; }

        public TMemberInfo GetMemberInfo<TMemberInfo>(Func<Type, string, BindingFlags, TMemberInfo> actGetMember, string memberName, BindingFlags bindingFlags = defaultBindingFlags)
            where TMemberInfo : MemberInfo
        {
            if (TargetType == null)
                return null;

            if (memberName == emptyMemberName || memberName.IsNullOrEmpty())//该字段没选择任意Member，不当报错
                return null;

            TMemberInfo memberInfo = actGetMember(TargetType, memberName, bindingFlags);
            if (memberInfo == null)
            {
                Debug.LogError("Can't find " + typeof(TMemberInfo) + " with name " + memberName + "in" + TargetType + "!");
            }
            return memberInfo;
        }

        #region Editor
#if UNITY_EDITOR
        [ContextMenu("LogValue")]
        public void LogValue()
        {
            Debug.Log(
                "Field: " + FieldCurValue + " | " +
                "Property: " + PropertyCurValue + " | " +
                "Method: " + MethodCurValue);
        }
#endif
        #endregion
    }
}