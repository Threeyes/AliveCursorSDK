using Newtonsoft.Json;
using System;
using Threeyes.Data;
namespace Threeyes.RuntimeSerialization
{
    /// <summary>
    /// 标记继承的类支持序列化/反序列化
    /// 
    /// PS：
    /// -不应与Newtown的Callback重名：https://www.newtonsoft.com/json/help/html/SerializationCallbacks.htm
    /// </summary>
    public partial interface IRuntimeSerializable
    {
        /// <summary>
        /// 目标类型，可以是自身
        /// </summary>
        public System.Type ContainerType { get; }

        //public Object BaseContainer { get; }
    }


    public interface IRuntimeSerializableOptionHolder<TOption>
        where TOption : IRuntimeSerializableOption
    {
        TOption Option { get; }
    }

    public interface IRuntimeSerializableOption { }
    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public class RuntimeSerializableOption : IRuntimeSerializableOption
    {
        public RuntimeSerializableOption() { }
    }
}