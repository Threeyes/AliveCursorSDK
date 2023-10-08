using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace Threeyes.Steamworks
{
    public static class SteamworksTool
    {
        /// <summary>
        /// 自动注册对应接口字段
        /// PS：
        /// -每个类根据其权限范围不同，可能在不同ManagerHolder中都有对应字段
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="overrideTypeManagerInterface"></param>
        /// <returns>是否注册成功</returns>
        public static bool RegisterManagerHolder(object obj, Type overrideTypeManagerInterface = null)
        {
            bool hasRegistSuccess = false;
            //#0 统一尝试注册InternalManagerHolder类（PS:因为SDK中无此类，所以会无法找到并初始化，但不影响运行；另外因为权限问题，一个实例可能会同时注册不同ManagerHolder中的不同接口实例）
            hasRegistSuccess = RegisterInternalManagerHolder(obj, overrideTypeManagerInterface);

            //#1 尝试注册自定义ManagerHolder类（如AC_ManagerHolder）
            hasRegistSuccess |= RegisterProductManagerHolder(obj, overrideTypeManagerInterface);

            //#3 尝试注册公共ManagerHolder
            //if (!hasRegistSuccess)
            hasRegistSuccess |= RegisterCommonManagerHolder(obj, overrideTypeManagerInterface);
            return hasRegistSuccess;
        }

        static bool RegisterInternalManagerHolder(object obj, Type overrideTypeManagerInterface = null)
        {
            //PS:因为SDK中无此类，所以会无法找到并初始化，但不影响运行
            Type typeManagerHolder =
            //    typeof(InternalManagerHolder);
            //Debug.LogError(typeManagerHolder.FullName);
            GetTypeFromAllAssembly("Threeyes.Steamworks.InternalManagerHolder");//需要带命名空间，否则无法找到
            if (typeManagerHolder != null)
                return ManagerHolderTool.Register(obj, typeManagerHolder, overrideTypeManagerInterface);
            return false;
        }


        /// <summary>
        /// 注册自定义ManagerHolder中的字段
        /// 实现：通过反射检查对应子类是否继承特定接口（如IAC_XXXManager），且该接口是否在ManagerHolder静态类中有对应字段，如果是则自动注册到ManagerHolder的对应字段中
        /// Warning:
        /// -TInstance需要与ManagerHolder在同一程序集中，否则出错
        /// </summary>
        /// <typeparam name="TInstance">obj的类型</typeparam>
        /// <param name="overrideTypeManagerInterface">自定义的接口，如果不是按照明明规则定义的接口则需要设置</param>
        static bool RegisterProductManagerHolder(object obj, Type overrideTypeManagerInterface = null)
        {
            //Assembly assembly = typeManagerInterface.Assembly;//因为SDK程序集与该组件所在程序集不一致，直接使用GetType会报错，因此需要在其自定义SDK程序集中搜索（后期可以是在所有程序集中搜索）
            //Type typeManagerHolder = assembly.GetType(managerHolderTypeName);
            Type typeManagerHolder = GetTypeFromAllAssembly($"{SORuntimeSettingManager.Instance.productNameForShort}_ManagerHolder");
            return ManagerHolderTool.Register(obj, typeManagerHolder, overrideTypeManagerInterface);
        }

        /// <summary>
        /// 注册通用ManagerHolder中的字段
        /// </summary>
        /// <typeparam name="TInstance"></typeparam>
        /// <param name="obj"></param>
        /// <param name="overrideTypeManagerInterface"></param>
        static bool RegisterCommonManagerHolder(object obj, Type overrideTypeManagerInterface = null)
        {
            return ManagerHolderTool.Register(obj, typeof(ManagerHolder), overrideTypeManagerInterface);
        }

        static Type GetTypeFromAllAssembly(string typeName)
        {
            Type targetType = null;
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                //var name = assembly.GetName();
                //Console.WriteLine($"Name={name.Name} Version={name.Version} Location={assembly.Location}");
                targetType = assembly.GetType(typeName);
                if (targetType != null)
                    return targetType;
            }
            return null;
        }

    }
}