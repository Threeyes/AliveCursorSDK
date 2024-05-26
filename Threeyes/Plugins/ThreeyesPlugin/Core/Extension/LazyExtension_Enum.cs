using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.Core
{
    public static class LazyExtension_Enum
    {
        /// <summary>
        /// Parse string to Enum,Int, Etc
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="myString"></param>
        /// <returns></returns>
        public static T Parse<T>(this string myString) where T : struct
        {
            T enumerable = default;
            try
            {
                enumerable = (T)Enum.Parse(typeof(T), myString);
                //Foo(enumerable); //Now you have your enum, do whatever you want.
            }
            catch (Exception)
            {
                Debug.LogErrorFormat("Parse: Can't convert {0} to enum, please check the spell.", myString);
            }
            return enumerable;
        }

        //——Flag——

        /// <summary>
        /// 检测使用位运算负责的枚举（标记为[Flag])，是否包含某个值（如果报错不会影响后续代码）
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="en">实例</param>
        /// <param name="flag">某个枚举</param>
        /// <returns></returns>
        public static bool Has(this Enum en, Enum flag, bool ignoreZero = false)
        {
            try
            {
                if (ignoreZero && (int)(object)flag == 0)//PS:忽略 (None = 0),否则如果flag为0则一定会返回true
                    return false;

                return en.HasFlag(flag);//PS:Unity2021.2的性能比旧版快100倍以上
                                        //return ((int)(object)en & (int)(object)flag) == (int)(object)flag;//PS: 如果上面的方法在旧版C#不可用，可改用下面的判断
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取Enum值的具体枚举名称，排除None或All
        /// 
        /// PS：
        /// -【待确认】仅适用于使用了Flag标记的Enum
        /// </summary>
        /// <typeparam name="TEnumType"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static List<string> GetNamesEx(this Enum value, bool ignoreNone = true, bool ignoreEverything = true)
        {
            List<string> result = new List<string>();
            Type enumType = value.GetType();
            foreach (int i in Enum.GetValues(enumType))//先转为Value，排除None或All为自定义名称的干扰
            {
                if (ignoreNone && i == 0) continue;//排除None

                if (ignoreEverything && i == ~0) continue;//排除All

                Enum curEnum = Enum.Parse(enumType, i.ToString()) as Enum;
                if (value.Has(curEnum))
                {
                    result.Add(curEnum.ToString());
                }
            }
            return result;
        }
    }
}