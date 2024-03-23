using System;
using System.Collections.Generic;
using Threeyes.Persistent;
using Threeyes.Data;
using UnityEngine;
using Newtonsoft.Json;
using Threeyes.Config;
using NaughtyAttributes;
using Threeyes.Core;

namespace Threeyes.Steamworks
{
    /// <summary>
    /// Control Material's value
    /// 
    /// PS:
    /// -You can find material's property detail in Inspector Editor mode
    /// 
    /// ToAdd:
    /// -参考AudioVisualizer_IcoSphere，为每个ShaderPropertyBase增加hasChanged字段，只有Changed才进行更新
    /// - 编辑器方法，可以一键导入指定材质的属性
    /// -可以运行时更改SurfaceType等枚举（参考：https://forum.unity.com/threads/making-material-transparant-in-universal-rp.1216053/）
    /// -选项：是否更改SharedMaterial
    /// -新增List<RenderInfo>otherRenderInfo,里面包含其他Renderer及对应的ID，方便针对多个物体使用类类似的材质进行修改（如车体、车门），但又不能使用sharedMaterial（因为多个实例有不同的配色）（或者通过类似EventPlayer.EventListener的方式，向其他MaterialController进行广播，参数为ConfigInfoEvent，然后接收方使用自身的序号进行更新）（或者新增一个MaterialControllerGroup，针对类似情况，先生成一个单独的克隆材质）
    /// </summary>
    public class MaterialController : ConfigurableComponentBase<MaterialController, SOMaterialControllerConfig, MaterialController.ConfigInfo, MaterialController.PropertyBag>
    {
        #region Property & Field
        public Material Material
        {
            get
            {
                Material renderMaterial = GetMaterial(targetRenderer, Config.materialIndex, isShareMaterial);
                if (renderMaterial)
                {
                    return renderMaterial;
                }
                return targetMaterial;//如果无法找到，则返回指定Material资源
            }
        }


        //Set either one to provide material
        [SerializeField] protected Renderer targetRenderer;//Where the material attached
        [SerializeField] protected Material targetMaterial;//[Optional] Target material asset （源资源）
        [SerializeField] protected bool isShareMaterial = false;//是否修改共享材质（仅当targetRenderer不为空时有效），适用于多个物体共用同一个材质
        [SerializeField]
        protected List<RendererMaterialInfo> listRendererMaterialInfo = new List<RendererMaterialInfo>();//其他模型的材质信息，方便针对多个使用了相同或类似材质的模型进行统一修改

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
            SetMaterial(Material);

            //针对额外的Renderer进行修改（因为使用的材质可能不一致，仅仅是某些字段相同，所以不能直接用Material对其他Renderer进行替换）
            foreach (var rmInfo in listRendererMaterialInfo)
            {
                Material renderMaterial = GetMaterial(rmInfo.renderer, rmInfo.materialIndex, false);//一般不使用Share材质，否则也不用指定
                SetMaterial(renderMaterial);
            }
        }

        private void SetMaterial(Material targetMaterial)
        {
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

        #region Utility
        /// <summary>
        /// 通过Renderer组件获取指定的材质
        /// </summary>
        /// <param name="targetRenderer"></param>
        /// <param name="materialIndex"></param>
        /// <param name="isShareMaterial"></param>
        /// <returns>如果找不到，则返回null</returns>
        static Material GetMaterial(Renderer targetRenderer, int materialIndex, bool isShareMaterial)
        {
            if (targetRenderer)
            {
                Material desireMaterial = null;
                if (materialIndex >= 0)
                {
                    if (!Application.isPlaying || isShareMaterial)//非运行模式或共享材质
                    {
                        if (targetRenderer.sharedMaterials.Length > materialIndex)
                            desireMaterial = targetRenderer.sharedMaterials[materialIndex];
                    }
                    else
                    {
                        if (targetRenderer.materials.Length > materialIndex)
                        {
                            desireMaterial = targetRenderer.materials[materialIndex];
                        }
                    }
                }
                return desireMaterial;
            }
            return null;
        }
        #endregion

        #region Editor
#if UNITY_EDITOR
        /// <summary>
        /// 根据材质的配置，初始化listXXXShaderProperty对应的数据
        /// 
        /// Warning：
        /// -Option等仍需要手动设置
        /// </summary>
        [ContextMenu("UpdateConfigUsingComponentData")]
        void EditorUpdateConfigUsingComponentData()
        {
            if (Application.isPlaying)//运行时跳过
                return;
            Material targetMaterial = Material;//非运行模式，获取的是共享材质
            foreach (var tSP in Config.listTextureShaderProperty)
            {
                string propertyName = tSP.name;
                tSP.defaultTexture = targetMaterial.GetTexture(propertyName);
                tSP.offset = targetMaterial.GetTextureOffset(propertyName);
                tSP.scale = targetMaterial.GetTextureScale(propertyName);
            }
            foreach (var cSP in Config.listColorShaderProperty)
            {
                string propertyName = cSP.name;
                cSP.value = targetMaterial.GetColor(propertyName);
            }
            foreach (var iSP in Config.listIntShaderProperty)
            {
                string propertyName = iSP.name;
                iSP.value = targetMaterial.GetInt(propertyName);
            }
            foreach (var fSP in Config.listFloatShaderProperty)
            {
                string propertyName = fSP.name;
                fSP.value = targetMaterial.GetFloat(propertyName);
            }
        }
#endif
        #endregion

        #region Define
        /// <summary>
        /// 针对其他模型
        /// </summary>
        [Serializable]
        public class RendererMaterialInfo
        {
            public Renderer renderer;
            public int materialIndex = 0;//对应的材质信息
        }

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