using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Threeyes.Core;
#if USE_NaughtyAttributes
using NaughtyAttributes;
#endif

namespace Threeyes.ModuleHelper
{
    /// <summary>
    /// 设置Renderer及其子类的信息，适用于各类RP
    /// 
    /// Todo:
    /// -使用EnumDefinition_Material中的枚举
    /// </summary>
    public class RendererHelper : ComponentHelperBase<Renderer>
    {
        #region Property & Field
        public Material Material
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

        #region Material
        public void SetMaterial(Material material)
        {
            if (layer == 0)
                Comp.material = material;
            else//设置指定层的材质
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    if (Comp.sharedMaterials.Length >= layer + 1)
                    {
                        //返回的是一个数组引用，需要更改此数组 Note that like all arrays returned by Unity, this returns a copy of materials array. If you want to change some materials in it, get the value, change an entry and set materials back.
                        Material[] mats = Comp.sharedMaterials;
                        mats[layer] = material;
                        Comp.sharedMaterials = mats;
                    }
                    else
                    {
                        Debug.LogError($"{Comp.gameObject} don't have material in index {layer} !");
                    }
                }
                else
#endif
                {
                    if (Comp.materials.Length >= layer + 1)
                    {
                        //返回的是一个数组引用，需要更改此数组 Note that like all arrays returned by Unity, this returns a copy of materials array. If you want to change some materials in it, get the value, change an entry and set materials back.
                        Material[] mats = Comp.materials;
                        mats[layer] = material;
                        Comp.materials = mats;
                    }
                    else
                    {
                        Debug.LogError($"{Comp.gameObject} don't have material in index {layer} !");
                    }
                }
            }
        }
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
        public string customMaterialColorName = "";
        public bool isKeepAlpha = true;//Keep origin color's alpha
        public void SetColor(Color color)
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

        //ToAdd:SetAlpha、SetAlphaPercent
        #endregion

        #region Value
        [Header("——Set Value——")]
        public MaterialFloatType materialFloatType = MaterialFloatType.NormalScale;//用于设置基础Float字段
        public string customMaterialFloatName = "";

        public void SetFloat(float value)
        {
            string propertyName = materialFloatType.GetPropertyName(customMaterialFloatName);

#if UNITY_2021_1_OR_NEWER
            if (!Material.HasFloat(propertyName))
            {
                Debug.LogError(Material + " doesn't have float: " + propertyName + " !");
                return;
            }
#endif

            Material.SetFloat(propertyName, value);
        }

        #endregion

        #region Keyword
        /// <summary>
        /// 常见Keyword（可通过Inspector-Debug模式查看Valid Keywords）：
        /// -_EMISSION
        /// </summary>
        /// <param name="keyword"></param>
        public void EnableKeyword(string keyword)
        {
            Material.EnableKeyword(keyword);
        }
        public void DisableKeyword(string keyword)
        {
            Material.DisableKeyword(keyword);
        }
        #endregion


        //ToAdd...(参考RendererHelper_BuiltinRP，剔除掉Random、targetXXX等不通用方法)
    }
}