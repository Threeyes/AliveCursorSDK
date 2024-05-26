using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace Threeyes.Core
{
    /// <summary>
    /// Todo:
    /// -在Inspector中，绘制Value对应的enum值
    /// </summary>
    public class ReflectionMethodHolder_Enum : ReflectionMethodHolderBase<Enum>
    {
        public override Enum Value { get { return EnumTool.Parse(ParamRealType, serializeValue); } set { serializeValue = value.ToString(); } }//PS：参考PersistentData_Enum

        public Type ParamRealType
        {
            get
            {
                if (targetMethodInfo == null)
                    return null;

                ParameterInfo[] parameterInfos = targetMethodInfo.GetParameters();
                return parameterInfos[0].ParameterType;
            }
        }
        public string SerializeValue { get { return serializeValue; } set { serializeValue = value; } }
        [SerializeField] string serializeValue;//Save enum value in long format

        public override bool IsParamTypeMatch(Type typeInput)
        {
            return typeInput.IsEnum;//PS：仅支持具体的枚举定义，不包括System.Enum
        }

        /// <summary>
        /// Set enum via int value
        /// 
        /// Warning:
        /// - You have to make sure the target EnumType has the intValue
        /// </summary>
        /// <param name="intValue"></param>
        public virtual void InvokeTargetWithInt(int intValue)
        {
            TryInvokeMethod(() => targetMethodInfo.Invoke(Target, new object[] { intValue }));
        }
    }
}