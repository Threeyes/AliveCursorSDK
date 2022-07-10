using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class AC_VisualEffectHelperGroup : ComponentHelperGroupBase<AC_VisualEffectHelper, VisualEffect>
{
    public void SetFloat(float value)
    {
        ForEachChildComponent((c) => c.SetFloat(value));
    }
    public void SetInt(int value)
    {
        ForEachChildComponent((c) => c.SetInt(value));
    }
}