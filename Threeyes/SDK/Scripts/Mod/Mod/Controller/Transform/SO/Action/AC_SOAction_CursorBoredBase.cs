using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[JsonObject(MemberSerialization.Fields)]//PS:会影响子类
public abstract class AC_SOAction_CursorBoredBase : ScriptableObject, IAC_SOAction_CursorBored
{
    public virtual void UpdateMovement_Bored(IAC_TransformController transformController, bool isBoredInit = false)
    {
    }
}
