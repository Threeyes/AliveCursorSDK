using System;
using System.Reflection;
using UnityEngine;

namespace Threeyes.Persistent
{
    public class PersistentControllerFactory_File : PersistentControllerFactoryBase
    {
        protected virtual Type ControllerType { get { return typeof(PersistentController_File<>); } }//Override this to use custom Controller

        public override IPersistentController Create(IPersistentData persistentData, PersistentControllerOption option)
        {
            if (persistentData == null)
            {
                Debug.LogError("The persistentData is null!");
                return null;
            }

            Type valueType = GetArgumentType(persistentData);
            if (valueType == null)
                return null;

            MethodInfo methodInfo = GetType().GetMethod(nameof(Create), BindingFlags.NonPublic | BindingFlags.Instance, isGenericMethod: true);//获取CreateInstance<TValue> (要指定泛型方法，否则会调用到本方法)
            return methodInfo.MakeGenericMethod(valueType).Invoke(this, new object[] { persistentData, option }) as IPersistentController;
        }

        protected IPersistentController<TValue> Create<TValue>(IPersistentData<TValue> persistentData, PersistentControllerOption option)
        {
            try
            {
                if (persistentData != null)
                {
                    IPersistentController<TValue> controller = null;
                    var objControllerInst = Activator.CreateInstance(ControllerType.MakeGenericType(typeof(TValue)), persistentData, option);
                    if (objControllerInst != null)
                        controller = objControllerInst as IPersistentController<TValue>;//调用Controller对应的构造函数
                    else
                        Debug.LogError("Failed to Create Controller!");

                    return controller;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Create Controller Instance error: \r\n" + e);
            }
            return null;
        }
    }
}