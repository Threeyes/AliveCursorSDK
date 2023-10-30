using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_PIPELINE_URP
using UnityEngine.Rendering.Universal;
#endif
public class URPDecalProjectorHelper :
#if UNITY_PIPELINE_URP
    ComponentHelperBase<DecalProjector>
#else
    MonoBehaviour
#endif
{
    public AspectMode aspectMode = AspectMode.FitInParent;
    public bool keepAspect = true;
    public string texturePropertyName = "Base_Map";
    public Vector3 parentSize = Vector3.one;//Parent的尺寸，主要用于FitInParent等计算
    public void SetTexture(Texture texture)
    {
#if UNITY_PIPELINE_URP
        //PS:参考Renderer，应该在修改时需要克隆材质，避免修改到原资源
        Material materialClone = Instantiate(Comp.material);
        materialClone.SetTexture(texturePropertyName, texture);
        Comp.material = materialClone;

        SetAspect(texture);
#endif
    }


    public virtual void SetAspect()
    {
#if UNITY_PIPELINE_URP
        Material material = Comp.material;
        if (material)
            SetAspect(material.GetTexture(texturePropertyName));
#endif
    }
    public virtual void SetAspect(Texture texture)
    {
#if UNITY_PIPELINE_URP
        if (texture != null)
        {
            ///更改比例：
            ///-PS：width对应size.x，height对应size.y,projectionDepth对应size.z
            if (aspectMode != AspectMode.None && texture.height != 0 && texture.width != 0)
            {
                float aspectHeiDevWid = (float)texture.height / texture.width;
                Vector3 curSize = Comp.size;

                switch (aspectMode)
                {
                    case AspectMode.WidthControlsHeight:
                        curSize.y = curSize.x * aspectHeiDevWid; break;
                    case AspectMode.HeightControlsWidth:
                        curSize.x = curSize.y / aspectHeiDevWid; break;
                    case AspectMode.FitInParent://对比parentSize的比例及aspectHeiDevWid，然后根据小边进行缩放计算
                        float aspectHeiDevWidParent = parentSize.x / parentSize.y;
                        if (aspectHeiDevWidParent >= aspectHeiDevWid)//图片小于Parent的高宽比:WidthControlsHeight
                        {
                            curSize.x = parentSize.x;
                            curSize.y = curSize.x * aspectHeiDevWid;
                        }
                        else//其他：HeightControlsWidth
                        {
                            curSize.y = parentSize.y;
                            curSize.x = curSize.y / aspectHeiDevWid;
                        }
                        break;
                    case AspectMode.EnvelopeParent:
                        float aspectHeiDevWidParent2 = parentSize.x / parentSize.y;
                        if (aspectHeiDevWidParent2 >= aspectHeiDevWid)//图片小于Parent的高宽比:HeightControlsWidth
                        {
                            curSize.y = parentSize.y;
                            curSize.x = curSize.y / aspectHeiDevWid;
                        }
                        else//其他：WidthControlsHeight
                        {
                            curSize.x = parentSize.x;
                            curSize.y = curSize.x * aspectHeiDevWid;
                        }
                        break;
                }
                Comp.size = curSize;
            }
        }
#endif
    }

    /// <summary>
    /// Ref: AspectRatioFitter
    /// </summary>
    public enum AspectMode
    {
        None,
        WidthControlsHeight,
        HeightControlsWidth,
        FitInParent,
        EnvelopeParent
    }
}
