using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
namespace Threeyes.Core
{
    /// <summary>
    /// (配合EnumEvent使用（如PersistentData_Enum)）
    ///ToUpdate:
    ///-提醒用户Set时要确保传入值属于同一个枚举，或能转换（目前如果不匹配，会报错提示：System.ArgumentException）
    ///-增加通过对应的数字更改枚举
    /// </summary>
    public class ReflectionValueHolder_Enum : ReflectionValueHolder<Enum>
    {
        public override bool IsValueTypeMatch(Type typeInput)
        {
            return typeInput.IsEnum;//PS：仅支持具体的枚举定义，不包括System.Enum
        }
    }
}