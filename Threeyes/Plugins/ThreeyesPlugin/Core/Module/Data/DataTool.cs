using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.Data
{

    public static class DataTool
    {
        /// <summary>
        /// ��ȡ���͵Ļ��࣬�����ں������ض�Ӧ��DataOption
        /// </summary>
        /// <param name="originType"></param>
        /// 
        /// <returns></returns>
        public static Type GetBaseType(Type originType)
        {
            Type basicType = originType;

            ///���Enum����Ӧ��enum���壺
            /// 1.��originType��enum���壺IsEnum����true
            /// 2.��originType��Enumʱ��IsEnum����false����ʱ�����;���Ŀ��ֵ(ԭ���ڲ�������IsSubclassOf��This method also returns false if c and the current Type are equal.��https://docs.microsoft.com/en-us/dotnet/api/system.type.issubclassof?view=net-6.0��
            if (originType.IsEnum)
            {
                basicType = typeof(Enum);
            }
            return basicType;
        }
    }
}