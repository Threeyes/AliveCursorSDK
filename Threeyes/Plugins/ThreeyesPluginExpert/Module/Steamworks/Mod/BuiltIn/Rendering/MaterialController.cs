using System;
using System.Collections.Generic;
using Threeyes.Persistent;
using Threeyes.Data;
using UnityEngine;
using Newtonsoft.Json;
using Threeyes.Config;
using NaughtyAttributes;
using Threeyes.Core;
using Threeyes.Core.Editor;

namespace Threeyes.Steamworks
{
    /// <summary>
    /// Control Material's properties
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
    public class MaterialController : MaterialControllerBase<MaterialController, SOMaterialControllerConfig, MaterialController.ConfigInfo, MaterialController.PropertyBag>
        , IMaterialProvider
    {
        #region Property & Field
        public Material Material { get { return GetMaterial(useSharedMaterial, targetMaterialIndex); } }
        public Material TargetMaterial { get { return GetMaterial(false, targetMaterialIndex); } }
        public Material TargetSharedMaterial { get { return GetMaterial(true, targetMaterialIndex); } }

        [Header("Material Source")]
        //#Note: Only one of the following fields is required to provide a valid material
        [SerializeField] protected Renderer targetRenderer;//Where the main material attached
        [Tooltip("Only valid when the target is Renderer")] [SerializeField] protected int targetMaterialIndex = 0;
        [SerializeField] protected Material targetMaterial;//Target material asset （资源文件，会直接修改原文件）

        [Header("Config")]
        [SerializeField] protected bool useSharedMaterial = false;//是否使用共享材质（仅当targetRenderer不为空时有效），适用于多个物体共用同一个材质(ToUpdate：可以是如果为true则克隆targetMaterial并缓存到一个临时字段中)
        [SerializeField] protected List<RendererMaterialInfo> listRendererMaterialInfo = new List<RendererMaterialInfo>();//其他模型的材质信息，方便针对多个使用了相同或类似材质的模型进行统一修改

        [Header("Runtime")]
        Material cloneTargetMaterial;
        #endregion

        #region Unity Method
        protected override void Awake()
        {
            base.Awake();

            //#if UNITY_EDITOR
            //            if (!UModTool.IsUModGameObject(this) && targetMaterial)//忽略UMod加载
            //                cloneMaterial = Instantiate(targetMaterial);
            //#endif
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            //#if UNITY_EDITOR
            //            ///退出后,将项目的Material还原为默认值，可以是启动后直接创建一个克隆的材质，退出时用其值还原（使用Render就没这个问题，因为是针对克隆体进行操作）
            //            if (Application.isEditor && targetMaterial)
            //                targetMaterial.CopyPropertiesFromMaterial(cloneMaterial);
            //#endif

            Config.listTextureShaderProperty.ForEach(t => t?.Dispose());//卸载所有运行时加载的资源
        }
        #endregion

        #region Callback
        protected ConfigInfo cacheValidConfig = null;//缓存上次有效的配置
        public override void UpdateSetting()
        {
            if (cacheValidConfig == null && cacheDefaultConfig != null)//避免因为物体未显示导致cacheDefaultConfig未初始化
                cacheValidConfig = UnityObjectTool.DeepCopy(cacheDefaultConfig);
            ///ToUpdate:
            ///-应该只有在开始的时候才需要这样比较（或者是比较上次有效修改的材质）。
            ///- 一旦生成材质克隆，就不要比较（可以检查指定材质是否有(instance)后缀）。
            ///-只有当Config与cacheDefaultConfig不同时（可以先简单按顺序匹配，检查每个元素是否一致）（需要重载各种类的Equal方法，包括DataOption。参考BasicData<TValue>的实现），才进行更新，避免出现多个克隆材质
            if (Equals(Config, cacheValidConfig))
            {
                //Debug.LogError("[Debug]Checking Config equal!");
                return;
            }
            else
            {
                //PS:由Json等调用时，可能会进入
                //Debug.LogError("[Debug]Checking Config not equal!");
            }

            SetMaterialProperty(Material);

            //针对额外的Renderer进行修改（因为使用的材质可能不一致，仅仅是某些字段相同，所以不能直接用Material对其他Renderer进行替换）
            foreach (var rmInfo in listRendererMaterialInfo)
            {
                if (rmInfo == null) continue;

                Material renderMaterial = GetMaterialFromRenderer(rmInfo.renderer, rmInfo.materialIndex, false);//一般不使用Share材质，否则也不用指定
                SetMaterialProperty(renderMaterial);
            }

            cacheValidConfig = UnityObjectTool.DeepCopy(Config);
        }

        void SetMaterialProperty(Material targetMaterial)
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
            SetListFunc(ref Config.listBoolShaderProperty,
            (propertyName, shaderProperty) =>
            {
                targetMaterial.SetFloat(propertyName, shaderProperty.FloatValue);
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
        protected Material GetMaterial(bool useSharedMaterial, int materialIndex)
        {
            //#1 尝试从可选项中找到首个有效材质
            Material renderMaterial = GetMaterialFromRenderer(targetRenderer, materialIndex, useSharedMaterial);//因为Config.materialIndex的值可能会修改，所以每次都重新获取而不是缓存
            if (renderMaterial)
                return renderMaterial;

            //#2 如果上述无效，则从实例材质中返回
            if (!Application.isPlaying || useSharedMaterial)//非运行模式，或使用share材质：返回原材质
            {
                return targetMaterial;
            }
            else//其他情况：克隆资源并返回，可避免资源直接修改
            {
                if (!cloneTargetMaterial)
                    cloneTargetMaterial = Instantiate(targetMaterial);
                return cloneTargetMaterial;
            }
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
            Material targetMaterial = GetMaterial(true, targetMaterialIndex);//非运行模式，获取的是共享材质
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
            foreach (var fSP in Config.listBoolShaderProperty)
            {
                string propertyName = fSP.name;
                fSP.value = BoolShaderProperty.IsOn(targetMaterial.GetFloat(propertyName));
            }
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
        #endregion

        #region Define
        //针对每个贴图（包含默认贴图）、float、vector等定义对应数据类，并且存储在各自的List中。具体结构参考（debug模式）shader

        [Serializable]
        public class ConfigInfo : SerializableComponentConfigInfoBase
            , IEquatable<ConfigInfo>
        {
            public List<TextureShaderProperty> listTextureShaderProperty = new List<TextureShaderProperty>();
            public List<ColorShaderProperty> listColorShaderProperty = new List<ColorShaderProperty>();
            public List<IntShaderProperty> listIntShaderProperty = new List<IntShaderProperty>();
            public List<FloatShaderProperty> listFloatShaderProperty = new List<FloatShaderProperty>();
            public List<BoolShaderProperty> listBoolShaderProperty = new List<BoolShaderProperty>();

            #region IEquatable
            public override bool Equals(object obj) { return Equals(obj as ConfigInfo); }
            public override int GetHashCode() { return base.GetHashCode(); }
            public bool Equals(ConfigInfo other)
            {
                if (other == null)
                    return false;

                //bool isEqual =
                //    materialIndex.Equals(other.materialIndex) &&
                //    listTextureShaderProperty.IsSequenceEqual(other.listTextureShaderProperty) &&
                //    listColorShaderProperty.IsSequenceEqual(other.listColorShaderProperty) &&
                //    listIntShaderProperty.IsSequenceEqual(other.listIntShaderProperty) &&
                //    listFloatShaderProperty.IsSequenceEqual(other.listFloatShaderProperty);
                bool isEqual = true;// materialIndex.Equals(other.materialIndex);
                isEqual &= listTextureShaderProperty.IsSequenceEqual(other.listTextureShaderProperty);
                isEqual &= listColorShaderProperty.IsSequenceEqual(other.listColorShaderProperty);
                isEqual &= listIntShaderProperty.IsSequenceEqual(other.listIntShaderProperty);
                isEqual &= listFloatShaderProperty.IsSequenceEqual(other.listFloatShaderProperty);
                isEqual &= listBoolShaderProperty.IsSequenceEqual(other.listBoolShaderProperty);

                return isEqual;
            }
            #endregion
        }
        public class PropertyBag : ConfigurableComponentPropertyBagBase<MaterialController, ConfigInfo> { }

        //——Warning:以下字段不能挪到其他位置，否则会导致用户之前的设置丢失——

        /// <summary>
        /// 
        /// PS:
        /// -因为每个子类的value字段都有对应的attribute，所以不能在该基类中提前定义
        /// </summary>
        public class ShaderPropertyBase : SerializableDataBase
            , IEquatable<ShaderPropertyBase>
        {
            public string name;//shader field name, set by modder（如：_BaseMap）（Todo【非必要，可以供用户动态修改】：runtimeEdit时不可编辑，但在UnityEditor可编辑。如有必要可自行写一个Attribute，增加一个bool值：editorEditableonly）

            #region IEquatable
            public override bool Equals(object obj) { return Equals(obj as ShaderPropertyBase); }
            public override int GetHashCode() { return base.GetHashCode(); }
            public virtual bool Equals(ShaderPropertyBase other)
            {
                if (other == null)
                    return false;
                return Equals(name, other.name);
            }
            #endregion
        }

        /// <summary>
        /// Todo：
        /// -继承IDisposable，在推出后卸载externalTexture
        /// 
        /// 常见属性：
        /// -_BaseMap
        /// </summary>
        [Serializable]
        public class TextureShaderProperty : ShaderPropertyBase, IDisposable
            , IEquatable<TextureShaderProperty>
        {
            public Texture Texture { get { return externalTexture ? externalTexture : defaultTexture; } }

            [JsonIgnore] public Texture defaultTexture;
            [JsonIgnore] public Texture externalTexture;
            [PersistentAssetFilePath(nameof(externalTexture), true, defaultAssetFieldName: nameof(defaultTexture))] public string externalTextureFilePath;
            public Vector2 scale = new Vector2(1, 1);//Warning:Remember to set this field, or your material may display incorrectly
            public Vector2 offset = new Vector2(0, 0);

            //ToAdd：导入图片的类型（如通过TextureTool进一步转为noramlMap）
            //#Runtime
            [HideInInspector] [JsonIgnore] [PersistentDirPath] public string PersistentDirPath;//必须要有，存储默认路径

            public TextureShaderProperty()
            {
                scale = new Vector2(1, 1);
            }

            public void Dispose()
            {
                //ToTest:看有无必要，因为Unity会自行管理加载后的图像
                //if (externalTexture)
                //{
                //DestroyImmediate(externalTexture, true);
                //}
            }

            #region IEquatable
            public override bool Equals(object obj) { return Equals(obj as TextureShaderProperty); }
            public override int GetHashCode() { return base.GetHashCode(); }
            public bool Equals(TextureShaderProperty other)
            {
                if (!base.Equals(other))//会同时判断是否为空，因此后续就不用判断
                    return false;

                ///PS:
                ///-仅当externalTextureFilePath有效时，才需要比较PersistentDirPath（因为其影响相对路径）
                if (externalTextureFilePath.NotNullOrEmpty() && Equals(externalTextureFilePath, other.externalTextureFilePath))
                {
                    if (!Equals(PersistentDirPath, other.PersistentDirPath))
                        return false;
                }

                bool isEqual =
                      Equals(defaultTexture, other.defaultTexture) &&
                     Equals(externalTexture, other.externalTexture) &&
                     Equals(scale, other.scale) &&
                     Equals(offset, other.offset);
                return isEqual;
            }
            #endregion
        }

        /// <summary>
        /// 
        /// 常见属性：
        /// -_BaseColor
        /// </summary>
        [Serializable]
        public class ColorShaderProperty : ShaderPropertyBase
            , IEquatable<ColorShaderProperty>
        {
            //使用[ColorUsage]，确保编辑模式能提供所有选项
            [ColorUsageEx(dataOptionMemberName: nameof(dataOption_Color))] [ColorUsage(showAlpha: true, hdr: true)] public Color value;
            public DataOption_Color dataOption_Color;

            public ColorShaderProperty()
            {
                value = Color.white;
                dataOption_Color = new DataOption_Color(true, true);
            }

            #region IEquatable
            public override bool Equals(object obj) { return Equals(obj as ColorShaderProperty); }
            public override int GetHashCode() { return base.GetHashCode(); }
            public bool Equals(ColorShaderProperty other)
            {
                if (!base.Equals(other))//会同时判断是否为空，因此后续就不用判断
                    return false;

                return Equals(value, other.value) && Equals(dataOption_Color, other.dataOption_Color);
            }
            #endregion
        }

        [Serializable]
        public class IntShaderProperty : ShaderPropertyBase
        , IEquatable<IntShaderProperty>
        {
            [RangeEx(dataOptionMemberName: nameof(dataOption_Int))] public int value;
            public DataOption_Int dataOption_Int;

            #region IEquatable
            public override bool Equals(object obj) { return Equals(obj as IntShaderProperty); }
            public override int GetHashCode() { return base.GetHashCode(); }
            public bool Equals(IntShaderProperty other)
            {
                if (!base.Equals(other))//会同时判断是否为空，因此后续就不用判断
                    return false;

                return Equals(value, other.value) && Equals(dataOption_Int, other.dataOption_Int);
            }
            #endregion
        }

        /// <summary>
        ///
        /// 特殊值：
        /// _AlphaClip：0为禁用；1为启用（ToUpdate：全部转为BoolShaderProperty）
        /// _Cutoff：对应AlphaClipping的Threshold
        /// </summary>
        [Serializable]
        public class FloatShaderProperty : ShaderPropertyBase
            , IEquatable<FloatShaderProperty>
        {
            [RangeEx(dataOptionMemberName: nameof(dataOption_Float))] public float value;
            public DataOption_Float dataOption_Float;

            #region IEquatable
            public override bool Equals(object obj) { return Equals(obj as FloatShaderProperty); }
            public override int GetHashCode() { return base.GetHashCode(); }
            public bool Equals(FloatShaderProperty other)
            {
                if (!base.Equals(other))//会同时判断是否为空，因此后续就不用判断
                    return false;

                return Equals(value, other.value) && Equals(dataOption_Float, other.dataOption_Float);
            }
            #endregion
        }

        /// <summary>
        /// 
        /// PS:
        /// -在Shader中，统一以float代替bool，且1为true，0为false(在Inspector中会绘制成Toggle)。因此在读写时需要转换为float（https://forum.unity.com/threads/shader-properties-no-bool-support.157580/）
        /// </summary>
        [Serializable]
        public class BoolShaderProperty : ShaderPropertyBase
            , IEquatable<BoolShaderProperty>
        {
            public float FloatValue { get { return value ? 1 : 0; } }
            public bool value;

            public static bool IsOn(float value)
            {
                return value != 0;
            }
            #region IEquatable
            public override bool Equals(object obj) { return Equals(obj as BoolShaderProperty); }
            public override int GetHashCode() { return base.GetHashCode(); }
            public bool Equals(BoolShaderProperty other)
            {
                if (!base.Equals(other))//会同时判断是否为空，因此后续就不用判断
                    return false;

                return Equals(value, other.value);
            }
            #endregion

        }

        #endregion
    }
}