using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if USE_NaughtyAttributes
using NaughtyAttributes;
#endif
/// <summary>
/// 设置Renderer及其子类的信息，适用于各类RP
/// 
/// Todo:
/// -使用EnumDefinition_Material中的枚举
/// </summary>
public class RendererHelper : ComponentHelperBase<Renderer>
{
    #region Property & Field
    Material Material
    {
        get
        {
            if (!_material)
            {
                if (layer == 0)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                        _material = Comp.sharedMaterial;
                    else
#endif
                        _material = Comp.material;
                }
                else
                {
                    if (Comp.materials.Length > layer)
                        _material = Comp.materials[layer];
                }
            }
            return _material;
        }
    }
    [SerializeField] Material _material;
    [SerializeField] int layer = 0;
    #endregion

    #region Texture
    [Header("——Texture——")]
    public MaterialTextureType materialTextureType = MaterialTextureType.BaseMap;
#if USE_NaughtyAttributes
    [ShowIf(nameof(materialTextureType), MaterialTextureType.Custom)]
#endif
    public string customMaterialTextureName = "";

    public void SetTexture(Texture texture)
    {
        if (!Material)
            return;

        string propertyName = materialTextureType.GetPropertyName(customMaterialTextureName);
#if UNITY_2021_1_OR_NEWER
        if (!Material.HasTexture(propertyName))
        {
            Debug.LogError(Material + " doesn't have texture: " + propertyName + " !");
            return;
        }
#endif
        Material.SetTexture(propertyName, texture);
    }
    #endregion

    #region Color
    [Header("——Set Color——")]
    public MaterialColorType materialColorType = MaterialColorType.BaseColor;//用于设置基础颜色（包括Alpha）、发光等
#if USE_NaughtyAttributes
    [ShowIf(nameof(materialColorType), MaterialColorType.Custom)]
#endif
    public string customMaterialColorName="";
    public bool isKeepAlpha = true;//Keep origin color's alpha
    void SetColor(Color color)
    {
        if (!Material)
            return;

        string propertyName = materialColorType.GetPropertyName(customMaterialColorName);

#if UNITY_2021_1_OR_NEWER
        if (!Material.HasColor(propertyName))
        {
            Debug.LogError(Material + " doesn't have color: " + propertyName + " !");
            return;
        }
#endif
        
        if (isKeepAlpha)
        {
            color.a = Material.GetColor(propertyName).a;
        }
        Material.SetColor(propertyName, color);
    }
    #endregion

    //ToAdd...(参考RendererHelper_BuiltinRP，剔除掉Random、targetXXX等)
}