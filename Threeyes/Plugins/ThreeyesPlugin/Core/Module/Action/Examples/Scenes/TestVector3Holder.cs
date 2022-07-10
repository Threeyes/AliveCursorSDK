using System.Collections;
using System.Collections.Generic;
using Threeyes.ValueHolder;
using UnityEngine;

public class TestVector3Holder : MonoBehaviour, IValueHolder<Vector3>
{
    public Vector3 CurValue
    {
        get
        {
            return value;
        }

        set
        {
            this.value = value;
            Debug.Log("SetValue" + value);
        }
    }

    public Vector3 value;
}
