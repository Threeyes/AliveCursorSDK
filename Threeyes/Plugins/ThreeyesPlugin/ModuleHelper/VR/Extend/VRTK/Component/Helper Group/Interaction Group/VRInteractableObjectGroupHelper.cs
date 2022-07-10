#if USE_VRTK
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class VRInteractableObjectGroupHelper : ComponentGroupBase<VRTK_InteractableObject>
{
    public void ToggleHighlight(bool toggle)
    {
        ForEachChildComponent((io) =>
        {
            io.ToggleHighlight(toggle);
        });
    }
    public void SetUsable(bool isUse)
    {
        ForEachChildComponent((io) =>
        {
            io.isUsable = isUse;
        });
    }
}
#endif