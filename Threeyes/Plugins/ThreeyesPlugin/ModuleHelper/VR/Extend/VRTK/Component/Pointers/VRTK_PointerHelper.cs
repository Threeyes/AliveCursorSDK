#if USE_VRTK
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
public class VRTK_PointerHelper : ComponentHelperBase<VRTK_Pointer>
{
    public void SetActivationButton(string buttonAliasName)
    {
        if (!Comp)
            return;

        VRTK_ControllerEvents.ButtonAlias buttonAlias = buttonAliasName.Parse<VRTK_ControllerEvents.ButtonAlias>();
        if (buttonAlias != default(VRTK_ControllerEvents.ButtonAlias))
            Comp.activationButton = buttonAlias;
    }
}
#endif