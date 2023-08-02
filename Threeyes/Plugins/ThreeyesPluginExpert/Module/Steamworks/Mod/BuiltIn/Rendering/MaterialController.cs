using System;
using System.Collections;
using System.Collections.Generic;
using Threeyes.Persistent;
using Threeyes.Data;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json;
using Threeyes.Config;

namespace Threeyes.Steamworks
{
    /// <summary>
    /// Control Material's value
    ///
    /// </summary>
    public class MaterialController : ConfigurableComponentBase<SOMaterialControllerConfig, MaterialController.ConfigInfo>
        , IModHandler
    {
        public Material Material
        {
            get
            {
                if (targetRenderer)
                    return targetRenderer.material;
                return targetMaterial;
            }
        }
        //Set either one to provide material
        [SerializeField] protected Renderer targetRenderer;//Where the material attached
        [SerializeField] protected Material targetMaterial;//Target material asset

        [Header("Runtime")]
        public Material cloneMaterial;

        #region Unity Method
        private void Awake()
        {
            Config.actionPersistentChanged += OnPersistentChanged;

            if (targetMaterial)
                cloneMaterial = Instantiate(targetMaterial);
        }
        private void OnDestroy()
        {
            Config.actionPersistentChanged -= OnPersistentChanged;

            ///退出后,将项目的Material还原为默认值，可以是启动后直接创建一个克隆的材质，退出时用其值还原（使用Render就没这个问题，因为时针对克隆体进行操作）
            if (Application.isEditor && targetMaterial)
                targetMaterial.CopyPropertiesFromMaterial(cloneMaterial);
        }
        #endregion

        #region Callback
        public void OnModInit()
        {
            UpdateSetting();
        }
        public void OnModDeinit() { }

        void OnPersistentChanged(PersistentChangeState persistentChangeState)
        {
            UpdateSetting();
        }
        public void UpdateSetting()
        {
            if (!Material)
                return;

            SetListFunc(ref Config.listTextureShaderProperty,
            (propertyName, shaderProperty) =>
            {
                Material.SetTexture(propertyName, shaderProperty.Texture);
                Material.SetTextureOffset(propertyName, shaderProperty.offset);
                Material.SetTextureScale(propertyName, shaderProperty.scale);
            });
            SetListFunc(ref Config.listColorShaderProperty,
            (propertyName, shaderProperty) =>
            {
                Color colorResult = shaderProperty.value;
                Material.SetColor(propertyName, colorResult);
            });

            SetListFunc(ref Config.listIntShaderProperty,
            (propertyName, shaderProperty) =>
            {
                Material.SetInteger(propertyName, shaderProperty.value);
            });
            SetListFunc(ref Config.listFloatShaderProperty,
            (propertyName, shaderProperty) =>
            {
                Material.SetFloat(propertyName, shaderProperty.value);
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
        [PersistentChanged(nameof(ConfigInfo.OnPersistentChanged))]
        public class ConfigInfo : SerializableDataBase
        {
            [JsonIgnore] public UnityAction<PersistentChangeState> actionPersistentChanged;

            public List<TextureShaderProperty> listTextureShaderProperty = new List<TextureShaderProperty>();
            public List<ColorShaderProperty> listColorShaderProperty = new List<ColorShaderProperty>();
            public List<IntShaderProperty> listIntShaderProperty = new List<IntShaderProperty>();
            public List<FloatShaderProperty> listFloatShaderProperty = new List<FloatShaderProperty>();

            #region Callback
            void OnPersistentChanged(PersistentChangeState persistentChangeState)
            {
                actionPersistentChanged.Execute(persistentChangeState);
            }
            #endregion
        }

        public class ShaderPropertyBase : SerializableDataBase
        {
            public string name;//shader field name, set by modder（如：_BaseMap）（Todo【非必要，可以供用户动态修改】：runtimeEdit时不可编辑，但在UnityEditor可编辑。如有必要可自行写一个Attribute，增加一个bool值：editorEditableonly）
        }

        [Serializable]
        public class TextureShaderProperty : ShaderPropertyBase
        {
            public Texture Texture { get { return externalTexture ? externalTexture : defaultTexture; } }

            [JsonIgnore] public Texture defaultTexture;
            [JsonIgnore] public Texture externalTexture;
            [PersistentAssetFilePath(nameof(externalTexture), true, defaultAssetFieldName: nameof(defaultTexture))] public string externalTextureFilePath;
            public Vector2 scale = new Vector2(1, 1);
            public Vector2 offset = new Vector2(0, 0);

            //ToAdd：导入图片的类型（如通过TextureTool进一步转为noramlMap）

            [HideInInspector] [JsonIgnore] [PersistentDirPath] public string PersistentDirPath;//必须要有，存储默认路径

            public TextureShaderProperty()
            {
                scale = new Vector2(1, 1);
            }
        }
        [Serializable]
        public class ColorShaderProperty : ShaderPropertyBase
        {
            [ColorUsageEx(dataOptionMemberName: nameof(dataOption_Color))] public Color value;
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