using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.Core
{
    public class ValueHolder_Color : ValueHolderBase<ColorEvent, Color>
    {
        public ColorEvent onRGBChanged;//Send RGB chanel only (alpha will always be 1)
        public FloatEvent onAlphaChanged;//Send Alpha chanel

        protected override void NotifyValueChanged(Color value)
        {
            base.NotifyValueChanged(value);

            Color colorRGB = value;
            colorRGB.a = 1;
            onRGBChanged.Invoke(colorRGB);
            onAlphaChanged.Invoke(value.a);
        }
    }
}