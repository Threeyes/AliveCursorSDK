using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Threeyes.BuiltIn
{
    public class Toggle_Ex : Toggle
    {
        protected override void OnDestroy()
        {
            //base.OnDestroy();//注意：Toggle销毁时，也会调用ToggleGroup的EnsureValidState
        }
    }
}