using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
namespace Threeyes.ValueHolder
{
    /// <summary>
    /// (配合EnumEvent使用（如PersistentData_Enum)）
    /// </summary>
    //ToUpdate:提醒用户Set时要确保传入值属于同一个枚举（目前如果不匹配，会报错提示：System.ArgumentException）
    public class ReflectionValueHolder_Enum : ReflectionValueHolder<Enum>
    {
        public override bool IsTypeMatch(Type targetType)
        {
            return targetType.IsEnum;//PS：仅支持具体的枚举定义，不包括System.Enum
        }
    }
}