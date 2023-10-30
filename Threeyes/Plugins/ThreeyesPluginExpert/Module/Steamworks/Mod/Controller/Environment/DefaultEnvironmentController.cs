using NaughtyAttributes;
using Newtonsoft.Json;
using System;
using Threeyes.Config;
using Threeyes.Persistent;
using UnityEngine;
using UnityEngine.Events;
using DefaultValue = System.ComponentModel.DefaultValueAttribute;

namespace Threeyes.Steamworks
{
    /// <summary>
    /// Control Environment Setting
    /// 
    /// PS:
    /// 1.Default Environment Lighting/Reflections Sources come from Skybox, inheric this class if your want to change them
    /// 2.该类只是提供常见的实现，不一定需要继承该类，使用父类也可
    /// </summary>
    //[AddComponentMenu(Steamworks_EditorDefinition.ComponentMenuPrefix_Root_Mod_Controller + "DefaultEnvironmentController")]
    public class DefaultEnvironmentController<TSOConfig, TConfig> : EnvironmentControllerBase<TSOConfig, TConfig>
       where TSOConfig : SOConfigBase<TConfig>, ISOEnvironmentControllerConfig
        where TConfig : DefaultEnvironmentControllerConfigInfo
    {
        #region Property & Field
        //PS：以下是场景相关的配置，暂不需要通过EnableIf来激活
        [Header("Lights")]
        [Tooltip("The Root gameobject for all lights")] [Required] [SerializeField] protected GameObject goLightGroup;
        [Tooltip("When the Skybox Material is a Procedural Skybox, use this setting to specify a GameObject with a directional Light component to indicate the direction of the sun (or whatever large, distant light source is illuminating your Scene). If this is set to None, the brightest directional light in the Scene is assumed to represent the sun. Lights whose Render Mode property is set to Not Important do not affect the Skybox.")] [Required] [SerializeField] protected Light sunSourceLight;//(Can be null)

        [Header("Reflection")]
        [Tooltip("The main ReflectionProbe")] [Required] [SerializeField] protected ReflectionProbe reflectionProbe;

        #endregion

        #region Unity Method
        protected virtual void Awake()
        {
            Config.actionIsUseLightsChanged += OnIsUseLightsChanged;
            Config.actionIsUseReflectionChanged += OnIsUseReflectionChanged;
            Config.actionIsUseSkyboxChanged += OnIsUseSkyboxChanged;
            Config.actionPersistentChanged += OnPersistentChanged;//Get called at last
        }
        protected virtual void OnDestroy()
        {
            Config.actionIsUseLightsChanged -= OnIsUseLightsChanged;
            Config.actionIsUseReflectionChanged -= OnIsUseReflectionChanged;
            Config.actionIsUseSkyboxChanged -= OnIsUseSkyboxChanged;
            Config.actionPersistentChanged -= OnPersistentChanged;
        }
        #endregion

        public override void OnModControllerInit()
        {
            UpdateSetting();
        }
        #region Config Callback
        void OnIsUseLightsChanged(PersistentChangeState persistentChangeState)
        {
        }
        void OnIsUseReflectionChanged(PersistentChangeState persistentChangeState)
        {
        }
        void OnIsUseSkyboxChanged(PersistentChangeState persistentChangeState)
        {
        }
        void OnPersistentChanged(PersistentChangeState persistentChangeState)
        {
            UpdateSetting();
        }

        protected virtual void UpdateSetting()
        {
            SetLights(Config.isUseLights);
            SetReflectionProbe(Config.isUseReflection);//Update ReflectionProbe's gameobject active state before skybox changes, or else the render may not update property
            SetSkybox(Config.isUseSkybox);
        }
        #endregion

        #region Module Setting
        bool lastReflectionProbeUsed = false;//Cache state, avoid render multi times
        public virtual void SetLights(bool isUse)
        {
            goLightGroup?.SetActive(isUse);
            if (isUse)
            {
                RenderSettings.sun = sunSourceLight;
                if (sunSourceLight)
                {
                    sunSourceLight.transform.eulerAngles = Config.sunLightRotation;
                    sunSourceLight.intensity = Config.sunLightIntensity;
                    sunSourceLight.color = Config.sunLightColor;
                    sunSourceLight.shadows = Config.lightShadowType;
                }
            }
        }
        public virtual void SetReflectionProbe(bool isUse)
        {
            if (!reflectionProbe)
                return;
            bool activeStateChanged = lastReflectionProbeUsed != isUse;
            lastReflectionProbeUsed = isUse;
            reflectionProbe.gameObject.SetActive(isUse);
            if (isUse && activeStateChanged)//在重新激活时要重新刷新
                RefreshReflectionProbe();
        }

        bool lastSkyboxUsed = false;//Cache state, avoid render multi times
        /// <summary>
        /// 
        /// </summary>
        /// <param name="isUse"></param>
        /// <returns>If skybox changed</returns>
        public virtual void SetSkybox(bool isUse)
        {
            bool needRefresh = lastSkyboxUsed != isUse;
            lastSkyboxUsed = isUse;
            if (isUse)
            {
                needRefresh |= UpdatePanoramaSkyboxMaterialTexture();//Try update texture first
                needRefresh |= UpdatePanoramaSkyboxMaterialRotation();
                if (RenderSettings.skybox != Config.SkyboxMaterial)//Check if skybox material changed
                {
                    RenderSettings.skybox = Config.SkyboxMaterial;
                    needRefresh = true;
                }
            }
            else
            {
                if (RenderSettings.skybox != null)
                {
                    RenderSettings.skybox = null;
                    needRefresh = true;
                }
            }
            if (needRefresh)
                DynamicGIUpdateEnvironment();
        }
        #endregion

        #region Utility
        /// <summary>
        /// Schedules an update of the environment cubemap.
        /// 
        /// Warning: Expensive operation! Only call it when the skybox changed
        /// 
        /// Ref: Changing the skybox at runtime will not update the ambient lighting automatically. You need to call DynamicGI.UpdateEnvironment() to let the engine know you want to update the ambient lighting. 
        /// Warning: This is a relatively expensive operation, which is why it’s not done automatically while the game is running.
        /// (https://forum.unity.com/threads/changing-skybox-materials-via-script.544854/#:~:text=Changing%20the%20skybox%20at%20runtime%20will%20not%20update,not%20done%20automatically%20while%20the%20game%20is%20running.)
        /// 
        /// PS:因为刷新GI会有损耗，因此只有当Skybox材质有变化时才调用该方法（注意不会更新Reflection，需要用户自行使用Reflection Probe实现）
        /// </summary>
        protected virtual void DynamicGIUpdateEnvironment()
        {
            RuntimeTool.ExecuteOnceInCurFrameAsync(DynamicGI.UpdateEnvironment);//Update environment cubemap
            RefreshReflectionProbe();
        }
        /// <summary>
        /// Update ReflectionProbe to refresh reflection
        /// </summary>
        void RefreshReflectionProbe()
        {
            if (!reflectionProbe)
                return;
            if (!lastReflectionProbeUsed)//PS:未激活时调用无效
                return;
            RuntimeTool.ExecuteOnceInCurFrameAsync(() => reflectionProbe.RenderProbe());//PS:RenderProbe会返回ID，可用于后续检查Render完成时间
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="texture"></param>
        /// <returns>Skybox texture changed</returns>
        bool UpdatePanoramaSkyboxMaterialTexture()
        {
            string panoMatTextureName = "_MainTex";
            if (Config.panoramaSkyboxMaterial && Config.panoramaSkyboxMaterial.HasTexture(panoMatTextureName)/* && Config.PanoramaSkyboxTexture*/)//不管有无贴图，都需要更新，便于重置
            {
                if (Config.panoramaSkyboxMaterial.GetTexture(panoMatTextureName) != Config.PanoramaSkyboxTexture)
                {
                    Config.panoramaSkyboxMaterial.SetTexture(panoMatTextureName, Config.PanoramaSkyboxTexture);
                    return true;
                }
            }
            return false;
        }
        bool UpdatePanoramaSkyboxMaterialRotation()
        {
            string panoMatRotationName = "_Rotation";
            if (Config.panoramaSkyboxMaterial && Config.panoramaSkyboxMaterial.HasFloat(panoMatRotationName) && Config.PanoramaSkyboxTexture)
            {
                if (Config.panoramaSkyboxMaterial.GetFloat(panoMatRotationName) != Config.panoramaSkyboxRotation)
                {
                    Config.panoramaSkyboxMaterial.SetFloat(panoMatRotationName, Config.panoramaSkyboxRotation);
                    return true;
                }
            }
            return false;
        }
        #endregion
    }

    #region Define

    /// <summary>
    ///
    ///
    /// Note:
    /// 1.在1.1.4版本中，isUseLights及isUsePanoramicSkybox经过了重命名，因此需要使用[DefaultValue]及[JsonProperty]指定默认值，避免Json中无对应字段而使用false作为初始值
    /// 2.为了方便ConfigInfo的命名，该类不加Base后缀（以后都按此规则）
    /// </summary>
    [Serializable]
    [PersistentChanged(nameof(DefaultEnvironmentControllerConfigInfo.OnPersistentChanged))]
    public class DefaultEnvironmentControllerConfigInfo : SerializableDataBase
    {
        [JsonIgnore] public UnityAction<PersistentChangeState> actionIsUseReflectionChanged;
        [JsonIgnore] public UnityAction<PersistentChangeState> actionIsUseLightsChanged;
        [JsonIgnore] public UnityAction<PersistentChangeState> actionIsUseSkyboxChanged;
        [JsonIgnore] public UnityAction<PersistentChangeState> actionPersistentChanged;

        public Material SkyboxMaterial { get { return skyboxType == SkyboxType.Default ? defaultSkyboxMaterial : panoramaSkyboxMaterial; } }
        public Texture PanoramaSkyboxTexture { get { return externalPanoramaTexture ? externalPanoramaTexture : defaultPanoramaTexture; } }

        [Header("Lights")]
        [PersistentValueChanged(nameof(OnPersistentValueChanged_IsUseLights))] public bool isUseLights = true;
        [EnableIf(nameof(isUseLights))] [AllowNesting] public Vector3 sunLightRotation = new Vector3(30, 30, 240);
        [EnableIf(nameof(isUseLights))] [AllowNesting] [Range(0, 8)] public float sunLightIntensity = 0.3f;
        [EnableIf(nameof(isUseLights))] [AllowNesting] public Color sunLightColor = Color.white;
        [EnableIf(nameof(isUseLights))] public LightShadows lightShadowType = LightShadows.None;

        [Header("ReflectionProbe")]
        [PersistentValueChanged(nameof(OnPersistentValueChanged_IsUseReflection))] public bool isUseReflection = true;

        [Header("Skybox")]
        [DefaultValue(true)] [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)] [PersistentValueChanged(nameof(OnPersistentValueChanged_IsUseSkybox))] public bool isUseSkybox = true;
        [EnableIf(nameof(isUseSkybox))] public SkyboxType skyboxType = SkyboxType.Default;
        //Default
        [ValidateInput(nameof(ValidateDefaultSkyboxMaterial), "The defaultSkyboxMaterial's shader should be the one in \"Skybox/...\" catelogy")] [EnableIf(nameof(isUseDefaultSkybox))] [AllowNesting] [JsonIgnore] public Material defaultSkyboxMaterial;
        //Panorama  
        [ValidateInput(nameof(ValidatePanoramaSkyboxMaterial), "The panoramaSkyboxMaterial's shader should be the one in \"Skybox/...\" catelogy")] [EnableIf(nameof(isUsePanoramicSkybox))] [AllowNesting] [JsonIgnore] public Material panoramaSkyboxMaterial;
        ///Skybox/Panoramic Shader中的全景图。（PS：Panorama类型的图片不要选中 "generate mipmaps"，否则会产生缝（外部加载的图片默认都不会生成））
        [EnableIf(nameof(isUsePanoramicSkybox))] [AllowNesting] [JsonIgnore] public Texture defaultPanoramaTexture;
        [EnableIf(nameof(isUsePanoramicSkybox))] [ReadOnly] [AllowNesting] [JsonIgnore] public Texture externalPanoramaTexture;
        [EnableIf(nameof(isUsePanoramicSkybox))] [AllowNesting] [PersistentAssetFilePath(nameof(externalPanoramaTexture), true)] public string externalPanoramaTextureFilePath;
        [EnableIf(nameof(isUsePanoramicSkybox))] [AllowNesting] [Range(0, 360)] public float panoramaSkyboxRotation = 0;
        [HideInInspector] [JsonIgnore] [PersistentDirPath] public string PersistentDirPath;


        #region Callback
        void OnPersistentValueChanged_IsUseReflection(PersistentChangeState persistentChangeState)
        {
            actionIsUseReflectionChanged.Execute(persistentChangeState);
        }
        void OnPersistentValueChanged_IsUseLights(PersistentChangeState persistentChangeState)
        {
            actionIsUseLightsChanged.Execute(persistentChangeState);
        }
        void OnPersistentValueChanged_IsUseSkybox(PersistentChangeState persistentChangeState)
        {
            actionIsUseSkyboxChanged.Execute(persistentChangeState);
        }

        void OnPersistentChanged(PersistentChangeState persistentChangeState)
        {
            actionPersistentChanged.Execute(persistentChangeState);
        }
        #endregion

        #region NaughtAttribute
        bool isUseDefaultSkybox { get { return isUseSkybox && skyboxType == SkyboxType.Default; } }
        bool isUsePanoramicSkybox { get { return isUseSkybox && skyboxType == SkyboxType.Panoramic; } }
        //PS:用户可能会自定义SkyboxShader，同时系统会自动判断材质是否有效，而且其他类型的Shader也能用，因此不需要判断是否使用了Skybox类型Shader（仅作为提示，不限制使用或打包）
        bool ValidateDefaultSkyboxMaterial(Material material)
        {
            if (material)
            {
                return material.shader.name.StartsWith("Skybox");
            }
            return true;//值为空不作错误处理
        }
        bool ValidatePanoramaSkyboxMaterial(Material material)
        {
            //string panoramaSkyboxShaderName = "Skybox/Panoramic";//PS:不限定，便于用户自行实现shader
            if (material)
            {
                return material.shader.name.StartsWith("Skybox");
            }
            return true;//值为空不作错误处理
        }
        #endregion

        #region Define
        public enum SkyboxType
        {
            Default,
            Panoramic
        }
        //PS:useColorTemperature等参数并不通用，为减少复杂度，让用户自行实现
        #endregion
    }

    #endregion
}