using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class RawImageHelper : ComponentHelperBase<RawImage>
{
    public void SetTexture(Texture texture)
    {
        Comp.texture = texture;
    }
}
