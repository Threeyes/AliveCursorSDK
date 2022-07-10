using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
namespace Threeyes.ValueHolder
{
    /// <summary>
    /// (���EnumEventʹ�ã���PersistentData_Enum)��
    /// </summary>
    //ToUpdate:�����û�SetʱҪȷ������ֵ����ͬһ��ö�٣�Ŀǰ�����ƥ�䣬�ᱨ����ʾ��System.ArgumentException��
    public class ReflectionValueHolder_Enum : ReflectionValueHolder<Enum>
    {
        public override bool IsTypeMatch(Type targetType)
        {
            return targetType.IsEnum;//PS����֧�־����ö�ٶ��壬������System.Enum
        }
    }
}