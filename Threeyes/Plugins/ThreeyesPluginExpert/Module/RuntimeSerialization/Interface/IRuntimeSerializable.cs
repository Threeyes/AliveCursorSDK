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
    public interface IRuntimeSerializable
    {
        /// <summary>
        /// 目标类型，可以是自身
        /// </summary>
        public System.Type ContainerType { get; }

        //public Object BaseContainer { get; }

        /// <summary>
        /// Save Data
        /// </summary>
        /// <returns></returns>
        public string Serialize();

        /// <summary>
        /// Restore Data
        /// </summary>
        /// <param name="content"></param>
        /// <param name="baseOption"> [Optional] Option</param>
        public void Deserialize(string content, IDeserializationOption baseOption = null);
        public void DeserializeBase(IComponentPropertyBag basePropertyBag, IDeserializationOption baseOption = null);
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