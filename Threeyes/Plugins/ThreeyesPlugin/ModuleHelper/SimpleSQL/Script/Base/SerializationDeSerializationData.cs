using UnityEngine;
#if USE_JsonDotNet
using Newtonsoft.Json;
#endif

//序列化和反序列化信息

//interface ISerializationDeSerialization<T>
//{
//    /// <summary>
//    /// 序列化
//    /// </summary>
//    /// <param name="value"></param>
//    /// <returns></returns>
//    string Serialize(T value);

//    /// <summary>
//    /// 反序列化
//    /// </summary>
//    /// <param name="content"></param>
//    /// <returns></returns>
//    T Deserialize(string content);
//}

public abstract class SerializationDeSerializationData<T>
    where T : SerializationDeSerializationData<T>
{
    public virtual string GetSerializeString()//Warning：不能改为字段，否则会在序列化时出现死循环
    { return SerializeFunc(this); }

    protected virtual string SerializeFunc(object value)
    {
#if USE_JsonDotNet
        return JsonConvert.SerializeObject(value, Formatting.Indented);
#else
        Debug.LogError("请在DefineSymbolManager中勾选\"Newtonsoft.Json\"!");
        return null;
#endif
    }

    protected virtual T DeserializeFunc(string content)
    {
#if USE_JsonDotNet
        return JsonConvert.DeserializeObject<T>(content);
#else
        Debug.LogError("请在DefineSymbolManager中勾选\"Newtonsoft.Json\"!");
        return null;
#endif
    }
}