using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Linq;

namespace Threeyes.Core
{
    /// <summary>
    /// Set a certain field/property/method of an instance through reflection (applicable to situations where it cannot be displayed in UnityEvent or the corresponding field is private)
    /// 通过反射设置实例的某个字段/属性/方法（适用于在UnityEvent中无法显示或对应字段是私有的情况）
    ///
    /// PS:
    /// 1.因为继承IValueHolder，因此可以被EventPlayer_SOAction自动调用
    /// 2.仅为了避免不必要的代码编写（如修改简单值类型），遇到复杂的类还是需要写代码（如Color等）
    /// 
    /// ToUpdate：
    /// -增加宏定义Threeyes_DisableReflection,方便用户不需要反射类方法时隐藏，或者自动判断平台并禁用
    /// </summary>
    public abstract class ReflectionValueHolderBase : MonoBehaviour
    {
        public Type TargetType { get { return Target ? Target.GetType() : null; } }
        public abstract Type ValueType { get; }

        public UnityEngine.Object Target { get { return target; } set { target = value; } }

        /// <summary>
        /// Target Script instance in Scene or Asset window 
        /// </summary>
        [SerializeField] protected UnityEngine.Object target;

        //Target对应成员的序列化信息(PS:不能修改修饰符，因为Inspector脚本需要使用）
        public virtual MemberType TargetMemberType { get { return targetMemberType; } set { targetMemberType = value; } }

        public string TargetSerializeFieldName { get { return targetSerializeFieldName; } set { targetSerializeFieldName = value; } }
        public string TargetSerializePropertyName { get { return targetSerializePropertyName; } set { targetSerializePropertyName = value; } }
        public string TargetSerializeGetMethodName { get { return targetSerializeGetMethodName; } set { targetSerializeGetMethodName = value; } }
        public string TargetSerializeSetMethodName { get { return targetSerializeSetMethodName; } set { targetSerializeSetMethodName = value; } }

        [SerializeField] protected MemberType targetMemberType = MemberType.Property;

        [SerializeField] protected string targetSerializeFieldName;
        [SerializeField] protected string targetSerializePropertyName;
        [SerializeField] protected string targetSerializeGetMethodName;
        [SerializeField] protected string targetSerializeSetMethodName;

        //——反射查找时的调用方法——
        /// <summary>
        /// 检查Member的类型是否匹配
        /// </summary>
        /// <param name="typeInput"></param>
        /// <returns></returns>
        public virtual bool IsTypeMatch(Type typeInput)
        {
            return Equals(typeInput, ValueType);
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
        /// <summary>
        /// Member types available for Value Holder
        /// </summary>
        public enum MemberType
        {
            Field,
            Property,
            Method,
            //Event//ToAdd
        }

        public static string emptyMemberName = "___";//占位，代表不选，用于EditorGUI
        public const BindingFlags defaultBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        #endregion
    }

    /// <summary>
    /// Get/Set Value using reflection
    /// 
    /// ToAdd：
    /// -增加不带参数的通用方法反射，方便EP调用
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class ReflectionValueHolder<TValue> : ReflectionValueHolderBase, IValueHolder<TValue>
    {
        public override Type ValueType { get { return typeof(TValue); } }

        /// <summary>
        /// TargetMember's Value
        /// </summary>
        public virtual TValue CurValue
        {
            set
            {
                switch (TargetMemberType)
                {
                    case MemberType.Field: TargetFieldValue = value; break;
                    case MemberType.Property: TargetPropertyValue = value; break;
                    case MemberType.Method: TargetMethodValue = value; break;
                    default:
                        Debug.LogError($"The get method for {TargetMemberType} is not available!"); break;
                }
            }
            get
            {
                switch (TargetMemberType)
                {
                    case MemberType.Field: return TargetFieldValue;
                    case MemberType.Property: return TargetPropertyValue;
                    case MemberType.Method: return TargetMethodValue;
                    default:
                        Debug.LogError($"The get method for {TargetMemberType} is not available!");
                        return default;
                }
            }
        }

        public TValue TargetFieldValue
        {
            get { return GetMember(fieldInfo, (mI, inst) => mI.GetValue(inst)); }
            set { SetMember(fieldInfo, (mI, inst, val) => mI.SetValue(inst, val), value); }
        }

        FieldInfo fieldInfo { get { return GetMemberInfo((type, name, bf) => type.GetField(name, bf), targetSerializeFieldName); } }
        public TValue TargetPropertyValue
        {
            get { return GetMember(propertyInfo, (mI, inst) => mI.GetValue(inst), (mI) => mI.CanRead); }
            set { SetMember(propertyInfo, (mI, inst, val) => mI.SetValue(inst, val), value, (mI) => mI.CanWrite); }
        }
        public PropertyInfo propertyInfo { get { return GetMemberInfo((type, name, bf) => type.GetProperty(name, bf), targetSerializePropertyName); } }

        public TValue TargetMethodValue
        {
            //PS:指明方法的特征，否则可能会报错：Ambiguous match found
            get { return GetMember(GetMemberInfo((type, name, bf) => type.GetMethods(bf).FirstOrDefault(mI => mI.Name == name && IsDesireGetMethod(mI)), targetSerializeGetMethodName), (mI, inst) => mI.Invoke(inst, new object[] { })); }
            set { SetMember(GetMemberInfo((type, name, bf) => type.GetMethods(bf).FirstOrDefault(mI => mI.Name == name && IsDesireSetMethod(mI)), targetSerializeSetMethodName), (mI, inst, val) => mI.Invoke(inst, new object[] { val }), value); }
        }

        //Todo:将GetMemberInfo弄成链式调用，而不是包含
        protected TValue GetMember<TMemberInfo>(TMemberInfo memberInfo, Func<TMemberInfo, object, object> actGetValue, Func<TMemberInfo, bool> actCheckIfCanGet = null)
            where TMemberInfo : MemberInfo
        {
            if (memberInfo != null)
            {
                bool canGet = actCheckIfCanGet != null ? actCheckIfCanGet(memberInfo) : true;//默认代表可写
                if (canGet)
                    return (TValue)actGetValue(memberInfo, Target);
            }
            return default;
        }

        protected bool SetMember<TMemberInfo>(TMemberInfo memberInfo, UnityAction<TMemberInfo, object, object>
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
                            actSetValue(memberInfo, Target, value);
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

        protected TMemberInfo GetMemberInfo<TMemberInfo>(Func<Type, string, BindingFlags, TMemberInfo> actGetMember, string memberName, BindingFlags bindingFlags = defaultBindingFlags)
            where TMemberInfo : MemberInfo
        {
            if (TargetType == null)
                return null;

            if (memberName == emptyMemberName || string.IsNullOrEmpty(memberName))//该字段没选择任意Member，不当报错
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
        //[ContextMenu("LogValue")]//泛型无法生成菜单，后期可通过ToString放到父类中
        //public void LogTargetMemberValue()
        //{
        //    Debug.Log(
        //        "Field: " + TargetFieldValue + " | " +
        //        "Property: " + TargetPropertyValue + " | " +
        //        "Method: " + TargetMethodValue);
        //}
#endif
        #endregion
    }
}