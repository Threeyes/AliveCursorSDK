using System.Collections;
using System.Collections.Generic;
using Threeyes.Core;
using UnityEngine;
#if USE_VFX
using UnityEngine.VFX;
#endif

namespace Threeyes.ModuleHelper
{
    public class VisualEffectHelperGroup :
#if USE_VFX
        ComponentHelperGroupBase<VisualEffectHelper, VisualEffect>
#else
    MonoBehaviour
#endif
    {
#if USE_VFX
        public void SetFloat(float value)
        {
            ForEachChildComponent((c) => c.SetFloat(value));
        }
        public void SetInt(int value)
        {
            ForEachChildComponent((c) => c.SetInt(value));
        }
#endif
    }
}