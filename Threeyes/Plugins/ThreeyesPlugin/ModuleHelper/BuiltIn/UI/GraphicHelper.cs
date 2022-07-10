using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphicHelper : ComponentHelperBase<Graphic>
{
    public float alphaGraphic
    {
        get { return Comp.color.a; }
        set
        {
            var color = Comp.color;
            color.a = value;
            Comp.color = color;
        }
    }

    public Color targetColor = Color.white;

    public void SetColor()
    {
        SetColor(targetColor);
    }

    void SetColor(Color color)
    {
        Comp.color = color;
    }
}
