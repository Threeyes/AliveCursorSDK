using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Call all MaterialChanger in child
/// </summary>
public class RendererHelperGroup : ComponentGroupBase<RendererHelper>
{
    public void SetMaterial()
    {
        ForEachChildComponent((c) => c.SetMaterial());
    }

}
