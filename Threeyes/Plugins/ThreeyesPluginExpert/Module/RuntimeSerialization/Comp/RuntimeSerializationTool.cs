using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.RuntimeSerialization
{
    public static class RuntimeSerializationTool
    {
        public static readonly Formatting DefaultComponentFormatting = Formatting.None;//因为默认以string格式存储，因此可以不添加回车等多余的字符
        public static readonly Formatting DefaultFormatting = Formatting.Indented;
        public static readonly JsonSerializerSettings DefaultJsonSerializerSettings = new JsonSerializerSettings
        {
            //TypeNameAssemblyFormatHandling = Newtonsoft.Json.TypeNameAssemblyFormatHandling.Simple,
            TypeNameHandling = TypeNameHandling.Objects,//增加$type字段，用于标记PropertyBag的类型
        };
    }
}