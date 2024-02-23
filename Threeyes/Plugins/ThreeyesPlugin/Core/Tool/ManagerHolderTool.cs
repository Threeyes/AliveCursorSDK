using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Threeyes.Core
{
    /// <summary>
    /// 管理ManagerHolder的接口字段注册
    /// 
    /// PS:
    /// -仅内部使用
    /// </summary>
    public static class ManagerHolderTool
    {

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <param name="obj"></param>
        /// <param name="typeManagerHolder"></param>
        /// <param name="overrideTypeManagerInterface"></param>
        /// <returns>是否注册成功</returns>
        public static bool Register(object obj, Type typeManagerHolder, Type overrideTypeManagerInterface = null, bool logErrOnFailed = false)
        {
            //#1 搜索接口
            List<Type> listTypeManagerInterface = new List<Type>();
            if (overrideTypeManagerInterface == null)
            {
                listTypeManagerInterface = obj.GetType().GetInterfaces().ToList();
                //.FirstOrDefault(t => t.Name.StartsWith($"I{SORuntimeSettingManager.Instance.productNameForShort}_") && t.Name.EndsWith("Manager"));//搜索以 IAC、Manager结尾的接口（ToDelete：有限制，无法兼容通用接口）
            }

            //#2 为ManagerHolder特定字段赋值
            PropertyInfo propertyInfoStatic = typeManagerHolder.GetProperties(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(
                (pI) =>
                {
                    if (overrideTypeManagerInterface != null)//如果指定接口，则按名称匹配
                        return pI.PropertyType == overrideTypeManagerInterface;
                    else
                        return listTypeManagerInterface.Contains(pI.PropertyType);
                }
            );
            if (propertyInfoStatic != null)
            {
                propertyInfoStatic.SetValue(null, obj);
                return true;
            }
            else
            {
                if (logErrOnFailed)
                    Debug.LogError($"Can't find static Property for type {obj.GetType()} in {typeManagerHolder.Name}");
            }
            return false;
        }

    }
}