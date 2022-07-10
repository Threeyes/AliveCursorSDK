using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderHelper : ComponentHelperBase<Slider>
{
    public void SetValueWithoutNotify(float value)
    {
        Comp.SetValueWithoutNotify(value);
    }
}
