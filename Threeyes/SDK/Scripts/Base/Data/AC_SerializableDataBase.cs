using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for all data that are Serializable via Json
/// </summary>
[JsonObject(MemberSerialization.Fields)]//Only Serialize Fields, child class will inheric this Attribute
public abstract class AC_SerializableDataBase
{
}
