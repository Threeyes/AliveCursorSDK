using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.Persistent
{
    public class PersistentControllerFactory_PlayerPref : PersistentControllerFactoryBase
    {

        public override IPersistentController Create(IPersistentData persistentData, PersistentControllerOption option)
        {
            Type valueType = GetArgumentType(persistentData);
            if (valueType == null)
                return null;

            //——JsonDotNet——
            ///Todo（在option中增加是否使用JsonDotNet存储的选项，如果是，那就使用特殊的Controller_String类进行存储，详细实现参考PersistentController_Json<TValue>；否则使用BuiltIn进行存储）:
            ///1.如果使用JsonDotNet：统一将Value通过Json序列化的方式转为string存储（）
            ///2.如果不使用JsonDotNet：使用默认存储方式

            //——BuiltIn——
            if (valueType == typeof(bool))
                return new PersistentController_PlayerPref_Bool(persistentData as IPersistentData<bool>, option);
            if (valueType == typeof(int))
                return new PersistentController_PlayerPref_Int(persistentData as IPersistentData<int>, option);
            if (valueType == typeof(float))
                return new PersistentController_PlayerPref_Float(persistentData as IPersistentData<float>, option);
            if (valueType == typeof(string))
                return new PersistentController_PlayerPref_String(persistentData as IPersistentData<string>, option);

            Debug.LogError(valueType + " is not support for PlayerPref!");
            return null;
        }
    }
}