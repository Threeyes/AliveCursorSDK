using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomForceGroup : ComponentGroupBase<RandomForce>
{
    public void SetValueAll()
    {
        ForEachChildComponent((c) => c.SetValue());
    }

    [ContextMenu("AddForceAll")]
    public void AddForceAll()
    {
        ForEachChildComponent((t) => t.AddForce());
    }
   [ContextMenu("AddTorqueAll")]
    public void AddTorqueAll()
    {
        ForEachChildComponent((t) => t.AddTorque());
    }
}
