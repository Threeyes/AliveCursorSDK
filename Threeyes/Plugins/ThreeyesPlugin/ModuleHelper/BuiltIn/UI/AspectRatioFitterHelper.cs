using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Threeyes.ModuleHelper
{
    [RequireComponent(typeof(AspectRatioFitter))]

    public class AspectRatioFitterHelper : UIAutoFitHelperBase<AspectRatioFitter>
    {
        protected override void SetAspectAtOnce(float aspectHeiDevWid)
        {
            //AspectRatioFitter中的aspectRatio是 Width/Height
            Comp.aspectRatio = 1 / aspectHeiDevWid;
        }
    }
}