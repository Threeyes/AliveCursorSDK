using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if USE_UIEffect
using Coffee.UIEffects;

/// <summary>
/// Todo:改为基于父类BaseMaterialEffect，并且会自动判断对应子类从而获取其effectFactor
/// </summary>
[RequireComponent(typeof(UIDissolve))]
#endif
public class UIDissolveHelper : MonoBehaviour
{
#if USE_UIEffect
  public UIDissolve comp;//待绑定的组件
    public UIDissolve Comp
    {
        get
        {
            if (!comp)
                comp = GetComponent<UIDissolve>();
            return comp;
        }
        set
        {
            comp = value;
        }
    }
#endif

    public float EffectFactoyReverse
    {
        get
        {
#if USE_UIEffect
            return 1 - Comp.effectFactor;
#endif
            return 0;
        }
        set
        {
#if USE_UIEffect
       Comp.effectFactor = 1 - value;
#endif
        }
    }

}
