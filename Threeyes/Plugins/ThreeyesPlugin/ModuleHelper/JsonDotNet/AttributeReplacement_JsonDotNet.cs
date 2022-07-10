using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;

/// <summary>
/// 该组件的作用是：在指定宏定义不激活时，保证相关Attribute及对应数据类可用
/// </summary>

#if !USE_JsonDotNet
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false)]
public class JsonPropertyAttribute : Attribute
{
    public string PropertyName { get; set; }

    public JsonPropertyAttribute()
    {
    }

    public JsonPropertyAttribute(string propertyName)
    {
        PropertyName = propertyName;
    }
}

public abstract class JsonContainerAttribute : Attribute
{
    public string Id = "";
    protected JsonContainerAttribute()
    {
    }

    protected JsonContainerAttribute(string id)
    {
        Id = id;
    }
}
public sealed class JsonObjectAttribute : JsonContainerAttribute
{
    public JsonObjectAttribute()
    {
    }

    public JsonObjectAttribute(MemberSerialization memberSerialization)
    {
    }

    public JsonObjectAttribute(string id)
        : base(id)
    {
    }
}

//
// 摘要:
//     Instructs the Newtonsoft.Json.JsonSerializer not to serialize the public field
//     or public read/write property value.
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public sealed class JsonIgnoreAttribute : Attribute
{
}


//
// 摘要:
//     Specifies the member serialization options for the Newtonsoft.Json.JsonSerializer.
public enum MemberSerialization
{
    //
    // 摘要:
    //     All public members are serialized by default. Members can be excluded using Newtonsoft.Json.JsonIgnoreAttribute
    //     or System.NonSerializedAttribute. This is the default member serialization mode.
    OptOut,
    //
    // 摘要:
    //     Only members marked with Newtonsoft.Json.JsonPropertyAttribute or System.Runtime.Serialization.DataMemberAttribute
    //     are serialized. This member serialization mode can also be set by marking the
    //     class with System.Runtime.Serialization.DataContractAttribute.
    OptIn,
    //
    // 摘要:
    //     All public and private fields are serialized. Members can be excluded using Newtonsoft.Json.JsonIgnoreAttribute
    //     or System.NonSerializedAttribute. This member serialization mode can also be
    //     set by marking the class with System.SerializableAttribute and setting IgnoreSerializableAttribute
    //     on Newtonsoft.Json.Serialization.DefaultContractResolver to false.
    Fields
}
#endif