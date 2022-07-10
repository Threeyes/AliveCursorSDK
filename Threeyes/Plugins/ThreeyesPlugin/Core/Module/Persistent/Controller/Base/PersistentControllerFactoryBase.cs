using System;
using System.Reflection;
using UnityEngine;
using System.Linq;
namespace Threeyes.Persistent
{
    public abstract class PersistentControllerFactoryBase
    {
        public abstract IPersistentController Create(IPersistentData persistentData, PersistentControllerOption option);

        protected static Type GetArgumentType(IPersistentData persistentData)
        {
            return ReflectionTool.GetGenericInterfaceArgumentTypes(persistentData.GetType(), typeof(IPersistentData<>)).FirstOrDefault();
        }
    }
}