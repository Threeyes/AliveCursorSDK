using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Threeyes.Core
{
    /// <summary>
    /// 
    /// PS：
    /// -
    /// -以下方法考虑了Flag的情况，针对Editor显示做了适配
    /// -因为方法主要参数为Type，因此不适合放到LazyExtension_Enum中
    /// 
    /// EnumName:
    /// 0
    /// -1
    /// 
    /// DisplayName:
    /// 0: Nothing
    /// -1: Everything
    /// </summary>
    public static class EnumTool
    {
        public const string defaultNothingEnumName = "0";
        public const string defaultEverythingEnumName = "-1";

        /// <summary>
        /// Convert from name/value to Enum type，支持传入','分割的多个枚举值
        /// </summary>
        /// <param name="enumNameOrValue">enum name (can be multi name splited by ','), or value in string format）</param>
        /// <returns>null if a parameter is invalid</returns>
        public static Enum Parse(Type enumType, string enumNameOrValue)
        {
            if (enumType != null && enumNameOrValue.NotNullOrEmpty())
            {
                //判断当前所有名称是否都是该枚举的定义，避免更换枚举或改名的情况。注意如果是Flag就要将添加的"0"和"-1"考虑进去 (PS:不能用Enum.IsDefined，因为该方法不能判断多个值）
                string[] arrCurEnum = enumNameOrValue.Split(',');//分离出所有传入的枚举值
                if (arrCurEnum.ToList().TrueForAll((str) => GetNamesEx(enumType).Contains(str.Trim())))
                {
                    object result = null;
                    try
                    {
                        result = Enum.Parse(enumType, enumNameOrValue);
                        if (result != null)
                            return result as Enum;
                    }
                    catch
                    {
                        //暂不处理
                    }
                }
            }
            return null;
        }

        public static bool HasFlagsAttribute(Type enumType)
        {
            if (enumType != null)
            {
                return enumType.IsDefined(typeof(FlagsAttribute), false);
            }
            return false;
        }

        //——Name——

        /// <summary>
        /// 获取单个值的Enum名，如果是Flag且没有定义义0/-1，则会自动返回对应值。
        /// </summary>
        /// <param name="enumType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetNameEx(Type enumType, object value)
        {
            string enumName = "";

            if (enumType != null)
            {
                if (HasFlagsAttribute(enumType))
                {
                    //如果Enum没有定义0/-1，则返回通用的名称
                    if (value.Equals(0) && !Enum.IsDefined(enumType, 0))
                        return defaultNothingEnumName;
                    if (value.Equals(-1) && !Enum.IsDefined(enumType, -1))
                        return defaultEverythingEnumName;
                }
                try
                {
                    enumName = Enum.GetName(enumType, value);
                }
                catch (Exception e)
                {
                    Debug.LogError("GetFriendlyName failed:" + e);
                }
            }
            return enumName;
        }


        /// <summary>
        /// 获取每个Enum对应的string名称，如果是Flag且没有定义义0/-1，则会自动添加。
        ///
        /// PS：
        /// -可用于Editor Inspector展示
        /// -
        /// 
        /// 默认顺序：
        /// 0
        /// -1
        /// 其他值...
        /// </summary>
        public static List<string> GetNamesEx(Type enumType)
        {
            List<string> listResult = new List<string>();
            if (enumType != null)
            {
                listResult.AddRange(Enum.GetNames(enumType));
                if (HasFlagsAttribute(enumType))//检查是否有对应的Flag
                {
                    //如果Enum没有定义0/-1，则添加（需要注意顺序不能弄错）
                    if (!Enum.IsDefined(enumType, 0))
                        listResult.Insert(0, defaultNothingEnumName);
                    if (!Enum.IsDefined(enumType, -1))
                        listResult.Add(defaultEverythingEnumName);
                }
            }
            return listResult;
        }

        //——DisplayName——

        /// <summary>
        /// 存储特殊Enum名的映射
        /// PS：
        /// 1.因为Enum不一定是int，还可能为long等。因此统一通过string存储Enum值，便于转换）
        /// 2.只有未定义时，其EnumName为其数值的对应字符串；如果已经定义，则无需转换
        /// </summary>

        public static Dictionary<string, string> defaultDicSpeicalEnumNameToDisplayName
        {
            get
            {
                if (_defaultDicSpeicalEnumNameToDisplayName == null)
                    _defaultDicSpeicalEnumNameToDisplayName = new Dictionary<string, string>
                {
                    {defaultNothingEnumName,"Nothing" },
                    {defaultEverythingEnumName,"Everything" }
                };
                return _defaultDicSpeicalEnumNameToDisplayName;
            }
        }
        static Dictionary<string, string> _defaultDicSpeicalEnumNameToDisplayName;
        /// <summary>
        /// 获取Enum的UI显示名称（与EnumName一一对应，转换而成）
        /// Return all Name or value in string type, mainly for UI display
        /// （针对Unity官方的Flag枚举（如CameraType)，因为其无定义0/1对应的枚举，所以会保存为其值的字符串）
        /// <paramref name="dicEnumNameToDisplayName"/>针对特殊值的自定义显示<paramref name="dicEnumNameToDisplayName"/>>
        /// <returns></returns>
        public static List<string> GetDisplayNames(Type enumType, Dictionary<string, string> dicEnumNameToDisplayName = null)
        {
            if (dicEnumNameToDisplayName == null)
                dicEnumNameToDisplayName = defaultDicSpeicalEnumNameToDisplayName;

            List<string> listEnumName = new List<string>(GetNamesEx(enumType));

            //对EnumName进行名字替换
            for (int i = 0; i != listEnumName.Count; i++)
            {
                string curEnumName = listEnumName[i];
                if (dicEnumNameToDisplayName.ContainsKey(curEnumName))
                {
                    listEnumName[i] = dicEnumNameToDisplayName[curEnumName];
                }
            }
            return listEnumName;
        }

        public static string DisplayNameToEnumName(string displayName, Dictionary<string, string> dicEnumNameToDisplayName = null)
        {
            string enumName = displayName;

            if (dicEnumNameToDisplayName == null)
                dicEnumNameToDisplayName = defaultDicSpeicalEnumNameToDisplayName;
            if (dicEnumNameToDisplayName.ContainsValue(displayName))//如果是特殊值，则查找对应的EnumName
                enumName = dicEnumNameToDisplayName.GetKey(displayName);
            return enumName;
        }
        #region Get
        /// <summary>
        /// 获取随机的枚举
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="enumVal"></param>
        /// <returns></returns>
        public static E GetRandom<E>()
        {
            var values = Enum.GetValues(typeof(E));
            return (E)values.GetValue(UnityEngine.Random.Range(0, values.Length - 1));
        }
        #endregion

        ///// <summary>
        ///// 所有值
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <returns></returns>
        //public static IEnumerable<string> ToValueList<T>()
        //{
        //    return Enum.GetValues(typeof(T))
        //            .Cast<int>()
        //            .Select(v => v.ToString());
        //}
        ///// <summary>
        ///// 所有名称
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <returns></returns>
        //public static IEnumerable<string> ToStringList<T>()
        //{
        //    return Enum.GetNames(typeof(T));
        //}

        #region Bit Shifing //以下代码仅做参考，Ref from: https://www.alanzucconi.com/2015/07/26/enum-flags-and-bitwise-operators/

        //public static T SetFlag<T>(T a, T b) 
        //{
        //    return a | b;
        //}
        //public static T UnsetFlag<T>(T a, T b)
        //{
        //    return a & (~b);
        //}
        //// Works with "None" as well
        //public static bool HasFlag<T>(T a, T b)
        //{
        //    return (a & b) == b;
        //}
        //public static T ToogleFlag<T>(T a, T b)
        //{
        //    return a ^ b;
        //}

        #endregion
    }
}