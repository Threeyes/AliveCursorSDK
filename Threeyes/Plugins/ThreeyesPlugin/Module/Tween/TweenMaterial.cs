#if USE_DOTween
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
/// <summary>
/// Tween材质的值
/// </summary>
public class TweenMaterial : TweenLoopValue
{
    [Header("设置材质")]
    public new Renderer renderer;//待绑定的组件
    public Renderer Renderer
    {
        get
        {
            if (!renderer)
                renderer = GetComponent<Renderer>();
            return renderer;
        }
    }
    public int matIndex = 0;
    public Material _material;
    Material Material
    {
        get
        {
            if (!_material)
            {
                if (matIndex == 0)
                    _material = Renderer.material;
                else
                {
                    if (Renderer.materials.Length >= matIndex + 1)
                    {
                        _material = Renderer.materials[matIndex];
                    }
                    else
                    {
                        Debug.LogError("越界！");
                    }
                }

            }
            return _material;
        }
    }

    public MaterialTweenType_Obsolete materialTweenType = MaterialTweenType_Obsolete.AlphaOrEmission;

    [Header("Texture")]
    public MaterialTextureType_Obsolete materialTextureType = MaterialTextureType_Obsolete._MainTex;
    public string customMaterialTextureName = "";//如果MaterialColorType设置为_Custom，则使用该名称
    public Vector2 offset;//贴图的位移值

    [Header("Color")]
    public MaterialColorType_Obsolete materialColor = MaterialColorType_Obsolete._Color;
    public string customMaterialColorName = "";//如果MaterialColorType设置为_Custom，则使用该名称
    public Color maxColor = Color.white;

    [Header("Value")]
    public MaterialValueType_Obsolete materialValueType = MaterialValueType_Obsolete._Cutoff;


    public string GetMaterialTextureName(MaterialTextureType_Obsolete materialTextureType)
    {
        if (materialTextureType == MaterialTextureType_Obsolete._Custom)
            return customMaterialTextureName;
        return materialTextureType.ToString();

    }

    public string GetMaterialColorName(MaterialColorType_Obsolete materialColorType)
    {
        if (materialColorType == MaterialColorType_Obsolete._Custom)
            return customMaterialColorName;
        return materialColorType.ToString();
    }

    protected override Tweener InitTweener()
    {
        switch (materialTweenType)
        {
            case MaterialTweenType_Obsolete.AlphaOrEmission:
                {
                    if (materialColor == MaterialColorType_Obsolete._EmissionColor)
                        return Material.DOColor(maxColor * TargetValue, GetMaterialColorName(materialColor), duration.ResultValue);//更改亮度
                    else
                        return Material.DOFade(TargetValue, GetMaterialColorName(materialColor), duration.ResultValue);//更改透明度
                }
            case MaterialTweenType_Obsolete.Color:
                return Material.DOColor(maxColor * TargetValue, GetMaterialColorName(materialColor), duration.ResultValue);
            case MaterialTweenType_Obsolete.Value:
                return Material.DOFloat(TargetValue, materialValueType.ToString(), duration.ResultValue);
            case MaterialTweenType_Obsolete.Offset:
                return Material.DOOffset(offset, GetMaterialTextureName(materialTextureType), duration.ResultValue);
            default:
                return base.InitTweener();
        }
    }

    protected override void SetValue(float value)
    {
        switch (materialTweenType)
        {
            case MaterialTweenType_Obsolete.AlphaOrEmission:
                Color resultColor = Color.black;

                if (materialColor == MaterialColorType_Obsolete._EmissionColor)//更改亮度
                {
                    resultColor = maxColor * value;
                }
                else//更改透明度
                {
                    resultColor = Material.GetColor(GetMaterialColorName(materialColor));
                    resultColor.a = value;//更改当前颜色的Alpha值
                }
                Material.SetColor(GetMaterialColorName(materialColor), resultColor);
                break;
            case MaterialTweenType_Obsolete.Color:
                Material.SetColor(GetMaterialColorName(materialColor), maxColor * value);
                break;
            case MaterialTweenType_Obsolete.Value:
                Material.SetFloat(materialValueType.ToString(), value);
                break;
            default:
                base.SetValue(value);
                break;
        }
    }
}
#endif