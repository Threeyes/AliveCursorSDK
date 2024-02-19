using System;
using System.Collections.Generic;
using Threeyes.Persistent;
using Threeyes.Data;
using UnityEngine;
using Newtonsoft.Json;
using Threeyes.Config;
using NaughtyAttributes;
namespace Threeyes.Steamworks
{
    /// <summary>
    /// Control Material's value
    /// 
    /// PS:
    /// -You can find material's property detail in Inspector Editor mode
    /// 
    /// ToAdd:
    /// - 编辑器方法，可以一键导入指定材质的属性
    /// -可以运行时更改SurfaceType等枚举（参考：https://forum.unity.com/threads/making-material-transparant-in-universal-rp.1216053/）
    /// </summary>
    public class MaterialController : ConfigurableComponentBase<MaterialController, SOMaterialControllerConfig, MaterialController.ConfigInfo, MaterialController.PropertyBag>
    {
        #region Property & Field
        public Material Material
        {
            get
            {
                if (targetRenderer)
                {
                    Material desireMaterial = null;
                    if (Config.materialIndex == 0)
                    {
#if UNITY_EDITOR
                        if (!Application.isPlaying)
                            desireMaterial = targetRenderer.sharedMaterial;
                        else
#endif
                            desireMaterial = targetRenderer.material;
                    }
                    else
                    {
                        if (targetRenderer.materials.Length > Config.materialIndex)
                            desireMaterial = targetRenderer.materials[Config.materialIndex];
                    }
                    return desireMaterial;
                }
                return targetMaterial;
            }
        }
        //Set either one to provide material
        [SerializeField] protected Renderer targetRenderer;//Where the material attached
        [SerializeField] protected Material targetMaterial;//[Optional] Target material asset

        [Header("Runtime")]
        [ReadOnly] public Material cloneMaterial;
        #endregion

        #region Unity Method
        protected override void Awake()
        {
            base.Awake();

#if UNITY_EDITOR
            if (!UModTool.IsUModGameObject(this) && targetMaterial)//忽略UMod加载
                cloneMaterial = Instantiate(targetMaterial);
#endif
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

#if UNITY_EDITOR
            ///退出后,将项目的Material还原为默认值，可以是启动后直接创建一个克隆的材质，退出时用其值还原（使用Render就没这个问题，因为时针对克隆体进行操作）
            if (Application.isEditor && targetMaterial)
                targetMaterial.CopyPropertiesFromMaterial(cloneMaterial);
#endif

            Config.listTextureShaderProperty.ForEach(t => t?.Dispose());//卸载所有运行时加载的资源
        }
        #endregion

        #region Callback
        public override void UpdateSetting()
        {
            Material targetMaterial = Material;//避免重复访问
            if (!targetMaterial)
                return;

            SetListFunc(ref Config.listTextureShaderProperty,
            (propertyName, shaderProperty) =>
            {
                targetMaterial.SetTexture(propertyName, shaderProperty.Texture);
                targetMaterial.SetTextureOffset(propertyName, shaderProperty.offset);
                targetMaterial.SetTextureScale(propertyName, shaderProperty.scale);
            });
            SetListFunc(ref Config.listColorShaderProperty,
            (propertyName, shaderProperty) =>
            {
                Color colorResult = shaderProperty.value;
                targetMaterial.SetColor(propertyName, colorResult);
            });

            SetListFunc(ref Config.listIntShaderProperty,
            (propertyName, shaderProperty) =>
            {
                targetMaterial.SetInteger(propertyName, shaderProperty.value);
            });
            SetListFunc(ref Config.listFloatShaderProperty,
            (propertyName, shaderProperty) =>
            {
                targetMaterial.SetFloat(propertyName, shaderProperty.value);
            });
        }

        void SetListFunc<TShaderProperty>(ref List<TShaderProperty> listProperty, Action<string, TShaderProperty> actionSetProperty)
            where TShaderProperty : ShaderPropertyBase
        {
            foreach (var shaderProperty in listProperty)
            {
                var propertyName = shaderProperty.name;
                if (!Material.HasProperty(propertyName))
                {
                    Debug.LogError(Material + " doesn't have property: " + propertyName + " !");
                }
                else
                {
                    actionSetProperty.Execute(propertyName, shaderProperty);
                }
            }
        }
        #endregion

        #region Define
        //针对每个贴图（包含默认贴图）、float、vector等定义对应数据类，并且存储在各自的List中。具体结构参考（debug模式）shader

        [Serializable]
        public class ConfigInfo : SerializableComponentConfigInfoBase
        {
            public int materialIndex = 0;

            public List<TextureShaderProperty> listTextureShaderProperty = new List<TextureShaderProperty>();
            public List<ColorShaderProperty> listColorShaderProperty = new List<ColorShaderProperty>();
            public List<IntShaderProperty> listIntShaderProperty = new List<IntShaderProperty>();
            public List<FloatShaderProperty> listFloatShaderProperty = new List<FloatShaderProperty>();
        }
        public class PropertyBag : ConfigurableComponentPropertyBagBase<MaterialController, ConfigInfo> { }

        public class ShaderPropertyBase : SerializableDataBase
        {
            public string name;//shader field name, set by modder（如：_BaseMap）（Todo【非必要，可以供用户动态修改】：runtimeEdit时不可编辑，但在UnityEditor可编辑。如有必要可自行写一个Attribute，增加一个bool值：editorEditableonly）
        }

        /// <summary>
        /// Todo：
        /// -继承IDisposable，在推出后卸载externalTexture
        /// 
        /// 常见属性：
        /// -_BaseMap
        /// </summary>
        [Serializable]
        public class TextureShaderProperty : ShaderPropertyBase, System.IDisposable
        {
            public Texture Texture { get { return externalTexture ? externalTexture : defaultTexture; } }

            [JsonIgnore] public Texture defaultTexture;
            [JsonIgnore] public Texture externalTexture;
            [PersistentAssetFilePath(nameof(externalTexture), true, defaultAssetFieldName: nameof(defaultTexture))] public string externalTextureFilePath;
            public Vector2 scale = new Vector2(1, 1);//Warning:Remember to set this field, or your material may display incorrectly
            public Vector2 offset = new Vector2(0, 0);

            //ToAdd：导入图片的类型（如通过TextureTool进一步转为noramlMap）

            [HideInInspector] [JsonIgnore] [PersistentDirPath] public string PersistentDirPath;//必须要有，存储默认路径

            public TextureShaderProperty()
            {
                scale = new Vector2(1, 1);
            }

            public void Dispose()
            {
                //if (externalTexture)
                //{
                //DestroyImmediate(externalTexture, true);//ToTest:看有无必要，因为Unity会自行管理加载后的图像
                //}
            }
        }

        /// <summary>
        /// 
        /// 常见属性：
        /// -_BaseColor
        /// </summary>
        [Serializable]
        public class ColorShaderProperty : ShaderPropertyBase
        {
            //使用[ColorUsage]，确保编辑模式能提供所有选项
            [ColorUsageEx(dataOptionMemberName: nameof(dataOption_Color))] [ColorUsage(showAlpha: true, hdr: true)] public Color value;


            public DataOption_Color dataOption_Color;

            public ColorShaderProperty()
            {
                value = Color.white;
                dataOption_Color = new DataOption_Color(true, true);
            }
        }

        [Serializable]
        public class IntShaderProperty : ShaderPropertyBase
        {
            [RangeEx(dataOptionMemberName: nameof(dataOption_Int))] public int value;
            public DataOption_Int dataOption_Int;
        }

        /// <summary>
        ///
        /// 特殊值：
        /// _AlphaClip：0为禁用；1为启用
        /// _Cutoff：对应AlphaClipping的Threshold
        /// </summary>
        [Serializable]
        public class FloatShaderProperty : ShaderPropertyBase
        {
            [RangeEx(dataOptionMemberName: nameof(dataOption_Float))] public float value;
            public DataOption_Float dataOption_Float;
        }
        #endregion
    }
}