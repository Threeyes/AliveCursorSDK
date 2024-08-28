using NaughtyAttributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Threeyes.Config;
using Threeyes.Core;
using Threeyes.Persistent;
using Threeyes.RuntimeEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using DefaultValue = System.ComponentModel.DefaultValueAttribute;

namespace Threeyes.GameFramework
{
    /// <summary>
    /// Control Environment Setting
    /// 
    /// PS:
    /// 1.Default Environment Lighting/Reflections Sources come from Skybox, inheric this class if your want to change them
    /// 2.该类只是提供常见的实现，不一定需要继承该类，使用父类也可
    /// </summary>
    //[AddComponentMenu(GameFramework_EditorDefinition.ComponentMenuPrefix_Root_Mod_Controller + "DefaultEnvironmentController")]
    public class DefaultEnvironmentController<TSOConfig, TConfig> : EnvironmentControllerBase<TSOConfig, TConfig>
       where TSOConfig : SOConfigBase<TConfig>, ISOEnvironmentControllerConfig
        where TConfig : DefaultEnvironmentControllerConfigInfo, new()
    {
        #region Property & Field
        public override Light SunSourceLight { get { return sunSourceLight; } }

        //PS：以下是场景相关的配置，暂不需要通过EnableIf来激活
        [Header("Lights")]
        [Tooltip("The Root gameobject for all lights")] [Required] [SerializeField] protected GameObject goLightGroup;
        [Tooltip("When the Skybox Material is a Procedural Skybox, use this setting to specify a GameObject with a directional Light component to indicate the direction of the sun (or whatever large, distant light source is illuminating your Scene). If this is set to None, the brightest directional light in the Scene is assumed to represent the sun. Lights whose Render Mode property is set to Not Important do not affect the Skybox.")] [Required] [SerializeField] protected Light sunSourceLight;

        [Header("Reflection")]
        [Tooltip("The main ReflectionProbe. Set type to 'Realtime'&'Via scripting' if the scene supports real-time updates of the radiation probe.")] [Required] [SerializeField] protected ReflectionProbe reflectionProbe;
        #endregion

        #region Unity Method
        protected virtual void Awake()
        {
            //if (Application.isEditor)
            //{
            //    //如果是Unity内部的SO，则克隆，并在退出时还原，避免其原值被修改（Todo：放到所有使用了SOConfig的父类中；要检查是否被PD引用，如果是就不适合这样操作，因为会破坏链接。PersistentData_SO有类似的备份还原功能，但是不是每个SO都配有PD）【V2】
            //    if (soOverrideConfig&& AssetDatabase.Contains(soOverrideConfig))
            //        var soClone = Instantiate(soOverrideConfig);
            //}

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

        #region Config Callback
        public override void OnModControllerInit()
        {
            UpdateSetting();
        }
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
            SetLight();
            SetReflectionProbe();//Update ReflectionProbe's gameobject active state before skybox changes, or else the render may not update property
            SetSkyboxActive();
        }
        #endregion

        #region Module Setting
        bool lastReflectionProbeUsed = false;//Cache state, avoid render multi times
        bool lastSkyboxUsed = false;//Cache state, avoid render multi times
        protected virtual void SetLight()
        {
            bool isUse = Config.isUseLights;
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
        protected virtual void SetReflectionProbe()
        {
            if (!reflectionProbe)
                return;
            bool isUse = Config.isUseReflection;
            bool activeStateChanged = lastReflectionProbeUsed != isUse;
            lastReflectionProbeUsed = isUse;
            reflectionProbe.gameObject.SetActive(isUse);
            if (isUse && activeStateChanged)//在重新激活时要重新刷新
                RefreshReflectionProbe();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isUse"></param>
        /// <returns>If skybox changed</returns>
        protected virtual void SetSkyboxActive()
        {
            bool isUse = Config.isUseSkybox;
            bool needRefresh = lastSkyboxUsed != isUse;
            lastSkyboxUsed = isUse;
            if (isUse)//使用：尝试更新参数
            {
                //#1 尝试更新参数（不管PanoramaSkybox材质是否在用，都要设置）
                needRefresh |= TrySetPanoramaSkybox_Texture();//update texture first
                needRefresh |= TrySetPanoramaSkybox_Rotation();

                //#2 尝试更新全局skybox材质
                needRefresh |= TrySetSkybox();
            }
            else//不使用任意Skybox：清空Skybox设置
            {
                needRefresh = TrySetSkyboxFunc(null);//设置为null
            }
            if (needRefresh)//修改天空盒后：更新GI（如反射探头）
                DynamicGIUpdateEnvironment();
        }


        protected bool TrySetSkyboxFunc(Material material)
        {
            //PS：不检查material是否为null，因为null可用于清空
            bool hasChange = false;
            if (RenderSettings.skybox != material)
            {
                RenderSettings.skybox = material;
                hasChange = true;
            }
            hasChange |= TrySetActiveProceduralSkybox_SunSize();//不管材质有无变化，都需要更新
            return hasChange;
        }

        /// <summary>
        /// 如果当前Skybox材质为Procedural，则更新sunSize
        /// 
        /// 调用时机：更改材质、更改SunEntity
        /// </summary>
        protected bool TrySetActiveProceduralSkybox_SunSize()
        {
            Material activeSkyboxMaterial = RenderSettings.skybox;
            if (!activeSkyboxMaterial)
                return false;

            return TrySetActiveProceduralSkybox_SunSizeFunc(activeSkyboxMaterial);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activeSkyboxMaterial">Warning：需要确保传入的材质使用了Procedural</param>
        protected virtual bool TrySetActiveProceduralSkybox_SunSizeFunc(Material activeSkyboxMaterial)
        {
            if (activeSkyboxMaterial.HasFloat(ShaderID_SunSize))
                if (activeSkyboxMaterial.GetFloat(ShaderID_SunSize) != Config.defaultSkyboxSunSize)
                {
                    activeSkyboxMaterial.SetFloat(ShaderID_SunSize, Config.defaultSkyboxSunSize);
                    return true;
                }
            return false;
        }

        //protected const string proceduralSkyboxShaderName = "Skybox/Procedural";
        protected int ShaderID_SunSize { get { if (shaderID_SunSize == 0) shaderID_SunSize = Shader.PropertyToID("_SunSize"); return shaderID_SunSize; } }
        int shaderID_SunSize = 0;

        protected int ShaderID_MainTex { get { if (shaderID_MainTex == 0) shaderID_MainTex = Shader.PropertyToID("_MainTex"); return shaderID_MainTex; } }
        int shaderID_MainTex = 0;
        protected int ShaderID_Rotation { get { if (shaderID_Rotation == 0) shaderID_Rotation = Shader.PropertyToID("_Rotation"); return shaderID_Rotation; } }
        int shaderID_Rotation = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="texture"></param>
        /// <returns>Skybox texture changed</returns>
        bool TrySetPanoramaSkybox_Texture()
        {
            if (Config.panoramaSkyboxMaterial && Config.panoramaSkyboxMaterial.HasTexture(ShaderID_MainTex)/* && Config.PanoramaSkyboxTexture*/)//不管有无贴图，都需要更新，便于无图时清空
            {
                if (Config.panoramaSkyboxMaterial.GetTexture(ShaderID_MainTex) != Config.PanoramaSkyboxTexture)//仅当贴图不同才更新
                {
                    Config.panoramaSkyboxMaterial.SetTexture(ShaderID_MainTex, Config.PanoramaSkyboxTexture);
                    return true;
                }
            }
            return false;
        }
        bool TrySetPanoramaSkybox_Rotation()
        {
            if (Config.panoramaSkyboxMaterial && Config.panoramaSkyboxMaterial.HasFloat(ShaderID_Rotation) && Config.PanoramaSkyboxTexture)
            {
                if (Config.panoramaSkyboxMaterial.GetFloat(ShaderID_Rotation) != Config.panoramaSkyboxRotation)
                {
                    Config.panoramaSkyboxMaterial.SetFloat(ShaderID_Rotation, Config.panoramaSkyboxRotation);
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Custom Skybox
        public override SkyboxController ActiveSkyboxController { get { return activeSkyboxController; } }
        public override int SkyboxControllerCount { get { return listSkyboxController.Count; } }//方便检查当前自定义天空盒的数量(可用于Inspector，当检查场景有多于一个SkyboxController时进行警告)
        SkyboxController activeSkyboxController;
        List<SkyboxController> listSkyboxController = new List<SkyboxController>();
        public override void RegisterSkyboxController(SkyboxController skyboxController)
        {
            listSkyboxController.AddOnce(skyboxController);
            listSkyboxController.Remove(null);//移除可能因场景切换等情况被删除的实例
            if (!Config.isUseSkybox)
                return;

            //更新天空盒设置TrySetSkybox，如果有更新则调用DynamicGIUpdateEnvironment
            bool needRefresh = TrySetSkybox();
            if (needRefresh)
                DynamicGIUpdateEnvironment();
        }
        public override void UnRegisterSkyboxController(SkyboxController skyboxController)
        {
            if (listSkyboxController.Count > 0)
            {
                listSkyboxController.Remove(skyboxController);
                listSkyboxController.Remove(null);//移除可能被删除的实例
            }

            if (!Config.isUseSkybox)//控制全局是否使用Skybox（包括Custom）
                return;

            bool needRefresh = TrySetSkybox();
            if (needRefresh)
                DynamicGIUpdateEnvironment();
        }

        /// <summary>
        /// 自动设置首个有效的Skybox（包括Custom）
        /// </summary>
        bool TrySetSkybox()
        {
            Material targetMaterial = Config.SkyboxMaterial;//默认使用Config的配置，避免listCustomSkyboxController中无有效元素

            activeSkyboxController = listSkyboxController.LastOrDefault();//以最后加入的自定义天空盒作为有效组件（可以为null）
            if (activeSkyboxController)
            {
                targetMaterial = activeSkyboxController.SkyboxMaterial;//如果有自定义的Skybox，则优先使用其材质

                listSkyboxController.ForEach(sC => sC.SetActive(sC == activeSkyboxController));//更新所有SC的激活状态
            }
            return TrySetSkyboxFunc(targetMaterial);
        }
        #endregion

        #region Utility
        /// <summary>
        /// Schedules an update of the environment cubemap.
        /// （更新GI及反射）
        /// 
        /// Warning: Expensive operation! Only call it when the skybox changed
        /// 
        /// Ref: Changing the skybox at runtime will not update the ambient lighting automatically. You need to call DynamicGI.UpdateEnvironment() to let the engine know you want to update the ambient lighting. 
        /// Warning: This is a relatively expensive operation, which is why it’s not done automatically while the game is running.
        /// (https://forum.unity.com/threads/changing-skybox-materials-via-script.544854/#:~:text=Changing%20the%20skybox%20at%20runtime%20will%20not%20update,not%20done%20automatically%20while%20the%20game%20is%20running.)
        /// 
        /// PS:
        /// -适用于Skybox变化时调用
        /// -因为刷新GI会有损耗，因此只有当Skybox材质有变化时才调用该方法（注意不会更新Reflection，需要用户自行使用Reflection Probe实现）
        /// </summary>
        [ContextMenu("DynamicGIUpdateEnvironment")]
        public virtual void DynamicGIUpdateEnvironment()
        {
            RuntimeTool.ExecuteOnceInCurFrameAsync(DynamicGI.UpdateEnvironment);//Update environment cubemap
            RefreshReflectionProbe();
        }

        protected float lastUpdateReflectionProbeTime = 0;
        /// <summary>
        /// Update ReflectionProbe to refresh reflection
        /// 
        /// ToUpdate：
        /// -需要先判断当前场景是否需要刷新（即有无影响ReflectionProbe的物体），然后才执行下面的步骤
        /// -刷新场景存在的所有反射探头，只要有一个正常刷新就返回true
        /// 
        /// PS：
        /// -适用于Skybox、主灯光发生变化时调用
        /// -Unity includes a dedicated manager—the SkyManager—to ensure that environment lighting affects your scene by default. The SkyManager automatically generates an ambient probe and default reflection probe to capture environment lighting.【场景默认有一个default Reflection Probe，用于捕捉Lighting窗口-Environment中的Environment lighting。当点击“Generate Lighting”后，就停止自动捕捉】（https://docs.unity3d.com/Manual/UsingReflectionProbes.html）
        /// </summary>
        /// <returns>是否正常刷新</returns>
        [ContextMenu("RefreshReflectionProbe")]
        public override bool RefreshReflectionProbe()
        {
            if (!reflectionProbe)
                return false;
            if (!lastReflectionProbeUsed)//PS:未激活时调用无效
                return false;

            //public enum ReflectionProbeMode
            //{
            //    Baked,//Reflection probe is baked in the Editor.
            //    Realtime,//Reflection probe is updating in real-time.
            //    Custom//Reflection probe uses a custom texture specified by the user.
            //}
            //仅当反射探头的属性为Realtime及ViaScripting时，才能调用方法更新
            if (reflectionProbe.mode == ReflectionProbeMode.Realtime && reflectionProbe.refreshMode == ReflectionProbeRefreshMode.ViaScripting)
            {
                //确保同一帧只调用一次
                RuntimeTool.ExecuteOnceInCurFrameAsync(() =>
                {
                    reflectionProbe.RenderProbe(/*reflectionProbe.realtimeTexture*/);//PS:RenderProbe会返回ID，可用于后续检查Render完成时间
                });
                lastUpdateReflectionProbeTime = Time.time;//记录渲染时间
                return true;
            }
            return false;
        }
        #endregion

        #region Editor Method
#if UNITY_EDITOR
        protected virtual void EditorUpdateConfigUsingComponentDataFunc()
        {
            //# Init
            var targetConfig = soOverrideConfig ? soOverrideConfig.config : defaultConfig;//不能直接通过Config获取！因为其会导致config被初始化，从而无法检测对config的修改

            //# Light
            if (targetConfig.isUseLights)
            {
                if (!sunSourceLight)
                {
                    Debug.LogError($"{nameof(sunSourceLight)} can't be null!");
                }
                else
                {
                    targetConfig.sunLightRotation = sunSourceLight.transform.eulerAngles;//更新值 
                    targetConfig.sunLightIntensity = sunSourceLight.intensity;
                    targetConfig.sunLightColor = sunSourceLight.color;
                    targetConfig.lightShadowType = sunSourceLight.shadows;
                }
            }

            //# Skybox
            Material defaultSkyboxMaterial = targetConfig.defaultSkyboxMaterial;
            if (defaultSkyboxMaterial)
            {
                if (defaultSkyboxMaterial.HasFloat(ShaderID_SunSize))
                    targetConfig.defaultSkyboxSunSize = defaultSkyboxMaterial.GetFloat(ShaderID_SunSize);
            }
            Material panoramaSkyboxMaterial = targetConfig.panoramaSkyboxMaterial;
            if (panoramaSkyboxMaterial)
            {
                if (panoramaSkyboxMaterial.HasTexture(ShaderID_MainTex))
                    targetConfig.defaultPanoramaTexture = panoramaSkyboxMaterial.GetTexture(ShaderID_MainTex);
                if (panoramaSkyboxMaterial.HasFloat(ShaderID_Rotation))
                    targetConfig.panoramaSkyboxRotation = panoramaSkyboxMaterial.GetFloat(ShaderID_Rotation);
            }


            //# Save
            //Debug.Log("Environment Config has changed");
            if (soOverrideConfig)//SO：需要持久化存储SOAsset
            {
                UnityEditor.EditorUtility.SetDirty(soOverrideConfig);//PS:需要调用该方法保存更改
            }
            else//defaultConfig：通知存储组件
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif
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
        [EnableIf(nameof(isUseLights))] [AllowNesting] [Range(0, 8)] public float sunLightIntensity = 0.3f;//Warning：只有当其值>0时，Procedural的Skybox中的灯光才会正常更新，否则会停留在之前的位置（如Ocean中海平面中的光源）（sunEntityColorOverTime也要注意各发光时颜色的Emission不为0）
        [EnableIf(nameof(isUseLights))] [AllowNesting] public Color sunLightColor = Color.white;
        [EnableIf(nameof(isUseLights))] public LightShadows lightShadowType = LightShadows.None;

        [Header("ReflectionProbe")]
        [PersistentValueChanged(nameof(OnPersistentValueChanged_IsUseReflection))] public bool isUseReflection = true;

        [Header("Skybox")]//Skybox的材质参数不一致，仅提供最通用字段，其他后续通过SkyboxController+MaterialController进行自定义
        [DefaultValue(true)] [Newtonsoft.Json.JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)] [PersistentValueChanged(nameof(OnPersistentValueChanged_IsUseSkybox))] public bool isUseSkybox = true;//控制全局是否使用Skybox（包括Custom）
        [EnableIf(nameof(isUseSkybox))] public SkyboxType skyboxType = SkyboxType.Default;
        //Default
        [ValidateInput(nameof(ValidateDefaultSkyboxMaterial), "The defaultSkyboxMaterial's shader should be the one in \"Skybox/...\" catelogy")] [EnableIf(nameof(isUseDefaultSkybox))] [AllowNesting] [JsonIgnore] public Material defaultSkyboxMaterial;
        [EnableIf(nameof(isUseDefaultSkybox))] [AllowNesting] [Range(0, 1)] public float defaultSkyboxSunSize = 0.04f;
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


        ///ToAdd：
        /// -Fog
        /// -通过RE按键或右键菜单，让用户手动更新反射探头
        [RuntimeEditorButton]//ToUpdate:改为RuntimeEditorContextMenu，在初始化UIInspectorElementBase时，调用AddContextMenuInfo
        void RefreshReflectionProbe()
        {
            ManagerHolder.EnvironmentManager.BaseActiveController.RefreshReflectionProbe();
        }
    }
    #endregion
}