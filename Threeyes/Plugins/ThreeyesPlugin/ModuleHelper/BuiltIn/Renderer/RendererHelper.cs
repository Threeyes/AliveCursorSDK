using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
/// <summary>
/// 设置材质的特定属性
/// </summary>
public class RendererHelper : ComponentHelperBase<Renderer>
{
    public Material _material;
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
    public int layer = 0;

    #region Texture

    [Header("——Set Texture——")]
    public MaterialTextureType_Obsolete materialTextureType = MaterialTextureType_Obsolete._MainTex;
    public string customMaterialTextureName = "";

    public void SetTexture(Texture texture)
    {
        switch (materialTextureType)
        {
            case MaterialTextureType_Obsolete._MainTex:
                Material.mainTexture = texture; break;
            case MaterialTextureType_Obsolete._Custom:
                Material.SetTexture(customMaterialTextureName, texture); break;
            default:
                Material.SetTexture(materialTextureType.ToString(), texture); break;
        }
    }

    #endregion

    #region Color

    [Header("——Set Color——")]
    public MaterialColorType_Obsolete materialColorType = MaterialColorType_Obsolete._Color;//用于设置Alpha、发光值
    [Tooltip("设置的目标值")]
    public Color targetColor = Color.white;
    [Tooltip("设置百分比的最大值")]
    public Color maxColor = Color.white;
    public float colorPercent = 1;
    public bool isKeepAlpha = true;

    [ContextMenu("SetColor")]
    public void SetColor()
    {
        SetColor(targetColor);
    }

    void SetColor(Color color)
    {
        if (isKeepAlpha)
        {
            color.a = Material.GetColor(materialColorType.ToString()).a;
        }
        Material.SetColor(materialColorType.ToString(), color);
    }

    public List<Color> listRandomColor = new List<Color>();
    public void SetRandomColorByList()
    {
        if (listRandomColor.Count > 0)
        {
            SetColor(listRandomColor.GetRandom());
        }
    }
    public void SetRandomColor()
    {
        SetColor(Random.ColorHSV());
    }

    [ContextMenu("SetColorPercent")]
    public void SetColorPercent()
    {
        SetColorPercent(colorPercent);
    }
    public void SetColorPercent(float percent)
    {
        Material.SetColor(materialColorType.ToString(), maxColor * percent);
    }

    #endregion

    #region Set Value

    [Space]
    [Header("——Set Float——")]
    public MaterialValueType_Obsolete materialValueType = MaterialValueType_Obsolete._BumpScale;
    public string customMaterialValueName = "";
    public float targetValue = 1f;


    //设置值参数
    [ContextMenu("SetValue")]
    public void SetValue()
    {
        SetValue(targetValue);
    }
    public void SetValue(float value)
    {
        Material.SetFloat(materialValueType == MaterialValueType_Obsolete._Custom ? customMaterialValueName.ToString() : materialValueType.ToString(), value);
    }

    #endregion

    #region SetMaterial

    [Header("——Set Material——")]
    public bool isIncludeChild = false;
    public Material targetMaterial;

    public List<Material> listTargetMaterial;//多个材质

    [ContextMenu("SetMaterial")]
    public void SetMaterial()
    {
        /// PS:有可能该物体没有Renderer组件，只是为了更改子对象的材质，所以用Transform比较保险
        Transform target = Comp ? target = Comp.transform : transform;

        if (isIncludeChild)
        {
            target.Recursive((tf) =>
            {
                SetMaterial(tf);
            });
        }
        else
        {
            SetMaterial(target);
        }
    }

    public void SetMaterial(Material material)
    {
        SetMaterial(Comp, material);
    }


    public void EnableEmission(bool isEnable)
    {
        if (isEnable)
        {
            Material.EnableKeyword("_Emission");
        }
        else
        {
            Material.DisableKeyword("_Emission");
        }
    }

    void SetMaterial(Component comp)
    {
        Renderer renderer = comp.GetComponent<Renderer>();
        if (renderer)
        {
            //设置单个材质
            if (targetMaterial)
            {
                SetMaterial(renderer, targetMaterial);
            }

            //设置多个材质
            if (listTargetMaterial.Count > 0)
            {
                renderer.materials = listTargetMaterial.ToArray();
            }
        }
    }

    void SetMaterial(Renderer renderer, Material material)
    {
        if (layer == 0)
            renderer.material = material;
        else//设置指定层的材质
        {
            if (renderer.materials.Length >= layer + 1)
            {
                //返回的是一个数组引用，需要更改此数组 Note that like all arrays returned by Unity, this returns a copy of materials array. If you want to change some materials in it, get the value, change an entry and set materials back.
                Material[] mats = renderer.materials;
                mats[layer] = targetMaterial;
                renderer.materials = mats;
            }
        }
    }

    #endregion

    #region Define


    #endregion
}
