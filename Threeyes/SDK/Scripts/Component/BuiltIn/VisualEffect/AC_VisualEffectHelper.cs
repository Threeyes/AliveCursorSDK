using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class AC_VisualEffectHelper : ComponentHelperBase<VisualEffect>
{
    public string propertyName;
    public void SetFloat(float value)
    {
        Comp.SetFloat(propertyName, value);
    }
    public void SetInt(int value)
    {
        Comp.SetInt(propertyName, value);
    }
}
