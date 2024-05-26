using UnityEngine;
using System;
using System.Reflection;
using UnityEngine.Events;

namespace Threeyes.Core
{
    public abstract class ReflectionMemberHolderBase : MonoBehaviour
    {
        public static string emptyMemberName = "___";//占位，代表不选，用于EditorGUI
        public const BindingFlags defaultBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        public Type TargetType { get { return Target ? Target.GetType() : null; } }
        public UnityEngine.Object Target { get { return target; } set { target = value; } }

        /// <summary>
        /// Target Script instance in Scene or Asset window 
        /// </summary>
        [SerializeField] protected UnityEngine.Object target;

        #region Utility

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

        /// <summary>
        /// 
        /// Todo:
        ///     -将GetMemberInfo弄成链式调用，而不是包含
        /// </summary>
        /// <typeparam name="TMemberInfo"></typeparam>
        /// <param name="actGetMemberInfo"></param>
        /// <param name="memberName"></param>
        /// <param name="bindingFlags"></param>
        /// <returns></returns>
        protected TMemberInfo GetMemberInfo<TMemberInfo>(Func<Type, string, BindingFlags, TMemberInfo> actGetMemberInfo, string memberName, BindingFlags bindingFlags = defaultBindingFlags)
            where TMemberInfo : MemberInfo
        {
            if (TargetType == null)
                return null;

            if (memberName == emptyMemberName || string.IsNullOrEmpty(memberName))//该字段没选择任意Member，不当报错
                return null;

            TMemberInfo memberInfo = actGetMemberInfo(TargetType, memberName, bindingFlags);
            if (memberInfo == null)
            {
                Debug.LogError("Can't find " + typeof(TMemberInfo) + " with name " + memberName + "in" + TargetType + "!");
            }
            return memberInfo;
        }
        #endregion
    }
}