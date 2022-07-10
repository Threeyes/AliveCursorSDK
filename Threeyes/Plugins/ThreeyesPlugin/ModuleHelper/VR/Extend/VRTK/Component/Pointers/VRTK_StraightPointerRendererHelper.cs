#if USE_VRTK
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
/// <summary>
/// 挂在VRTK_BasePointerRenderer 同级物体上
/// </summary>
public class VRTK_StraightPointerRendererHelper : ComponentHelperBase<VRTK_BasePointerRenderer>
{
    public void SetTracerVisibility(string enumName)
    {
        VRTK_BasePointerRenderer.VisibilityStates visibilityStates = enumName.Parse<VRTK_BasePointerRenderer.VisibilityStates>();
        Comp.tracerVisibility = visibilityStates;
    }
}
#endif