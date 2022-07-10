using NaughtyAttributes;
using Newtonsoft.Json;
using System;
using Threeyes.Persistent;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Control Environment Setting
/// 
/// PS:
/// 1.Default Environment Lighting/Reflections Sources come from Skybox, inheric this class if your want to change them
/// </summary>
[AddComponentMenu(AC_EditorDefinition.ComponentMenuPrefix_AC_Cursor_Controller + "AC_DefaultEnvironmentController")]
public class AC_DefaultEnvironmentController : AC_EnvironmentControllerBase<AC_SODefaultEnvironmentControllerConfig, AC_DefaultEnvironmentController.ConfigInfo>
{
	#region Property & Field
	public override bool IsUseReflection { get { return Config.isUseReflection; } }
	public override bool IsOverrideLights { get { return Config.isOverrideLights; } }
	public override bool IsOverrideSkybox { get { return Config.isOverrideSkybox; } }

	//PS：以下是场景相关的配置，暂不需要通过EnableIf来激活
	[Header("Lights")]
	[Tooltip("The Root gameobject for all lights")] [SerializeField] protected GameObject goLightGroup;
	[Tooltip("When the Skybox Material is a Procedural Skybox, use this setting to specify a GameObject with a directional Light component to indicate the direction of the sun (or whatever large, distant light source is illuminating your Scene). If this is set to None, the brightest directional light in the Scene is assumed to represent the sun. Lights whose Render Mode property is set to Not Important do not affect the Skybox.")] [SerializeField] protected Light sunSourceLight;//(Can be null)
	#endregion

	#region Unity Method
	private void Awake()
	{
		Config.actionIsOverrideLightsChanged += OnIsOverrideLightsChanged;
		Config.actionIsOverrideSkyboxChanged += OnIsOverrideSkyboxChanged;
		Config.actionPersistentChanged += OnPersistentChanged;
	}
	private void OnDestroy()
	{
		Config.actionIsOverrideLightsChanged -= OnIsOverrideLightsChanged;
		Config.actionIsOverrideSkyboxChanged -= OnIsOverrideSkyboxChanged;
		Config.actionPersistentChanged -= OnPersistentChanged;
	}
	#endregion

	#region Callback
	void OnIsOverrideLightsChanged(PersistentChangeState persistentChangeState)
	{
		NotifyIsOverrideLightsChanged(Config.isOverrideLights);
	}
	void OnIsOverrideSkyboxChanged(PersistentChangeState persistentChangeState)
	{
		NotifyIsOverrideSkyboxChanged(Config.isOverrideSkybox);
	}
	void OnPersistentChanged(PersistentChangeState persistentChangeState)
	{
		InitReflectionProbe(Config.isUseReflection);
		InitLights(Config.isOverrideLights);
		InitSkybox(Config.isOverrideSkybox);
	}
	#endregion

	#region Override
	public override void InitReflectionProbe(bool isActive)
	{
		Manager.SetReflectionProbeActive(isActive);
	}
	public override void InitLights(bool isOverride)
	{
		goLightGroup?.SetActive(isOverride);
		if (isOverride)
		{
			RenderSettings.sun = sunSourceLight;
			if (sunSourceLight)
			{
				sunSourceLight.transform.eulerAngles = Config.sunLightRotation;
				sunSourceLight.intensity = Config.sunLightIntensity;
				sunSourceLight.color = Config.sunLightColor;
			}
		}
	}
	/// <summary>
	/// 
	/// </summary>
	/// <param name="isOverride"></param>
	/// <returns>If skybox changed</returns>
	public override bool InitSkybox(bool isOverride)
	{
		bool needRefresh = false;
		if (isOverride)
		{
			needRefresh = UpdatePanoramaSkyboxMaterialTexture();//Update texture first
			needRefresh |= UpdatePanoramaSkyboxMaterialRotation();
			if (RenderSettings.skybox != Config.SkyboxMaterial)//Check if skybox material changed
			{
				RenderSettings.skybox = Config.SkyboxMaterial;
				needRefresh = true;
			}
			if (needRefresh)
				DynamicGIUpdateEnvironment();
		}
		return needRefresh;
	}
	#endregion

	#region Utility

	/// <summary>
	/// 
	/// </summary>
	/// <param name="texture"></param>
	/// <returns>Skybox texture changed</returns>
	bool UpdatePanoramaSkyboxMaterialTexture()
	{
		string panoMatTextureName = "_MainTex";
		if (Config.panoramaSkyboxMaterial && Config.panoramaSkyboxMaterial.HasTexture(panoMatTextureName) && Config.PanoramaSkyboxTexture)
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

	#region Define

	[Serializable]
	[PersistentChanged(nameof(ConfigInfo.OnPersistentChanged))]
	public class ConfigInfo : AC_SerializableDataBase
	{
		[JsonIgnore] public UnityAction<PersistentChangeState> actionIsOverrideLightsChanged;
		[JsonIgnore] public UnityAction<PersistentChangeState> actionIsOverrideSkyboxChanged;
		[JsonIgnore] public UnityAction<PersistentChangeState> actionPersistentChanged;

		public Material SkyboxMaterial { get { return skyboxType == SkyboxType.Default ? defaultSkyboxMaterial : panoramaSkyboxMaterial; } }
		public Texture PanoramaSkyboxTexture { get { return externalPanoramaTexture ? externalPanoramaTexture : defaultPanoramaTexture; } }

		[Header("ReflectionProbe")]
		public bool isUseReflection = false;

		[Header("Lights")]
		[PersistentValueChanged(nameof(OnPersistentValueChanged_IsOverrideLights))] public bool isOverrideLights = false;
		[EnableIf(nameof(isOverrideLights))] [AllowNesting] public Vector3 sunLightRotation = new Vector3(30, 30, 240);
		[EnableIf(nameof(isOverrideLights))] [AllowNesting] [Range(0, 8)] public float sunLightIntensity = 0.3f;
		[EnableIf(nameof(isOverrideLights))] [AllowNesting] public Color sunLightColor = Color.white;

		[Header("Skybox")]
		[PersistentValueChanged(nameof(OnPersistentValueChanged_IsOverrideSkybox))] public bool isOverrideSkybox = false;
		[EnableIf(nameof(isOverrideSkybox))] public SkyboxType skyboxType = SkyboxType.Default;
		//Default
		[ValidateInput(nameof(ValidateDefaultSkyboxMaterial), "The defaultSkyboxMaterial's shader should be the one in \"Skybox/...\" catelogy")] [EnableIf(nameof(isOverrideDefaultSkybox))] [AllowNesting] [JsonIgnore] public Material defaultSkyboxMaterial;
		//Panorama  
		[ValidateInput(nameof(ValidatePanoramaSkyboxMaterial), "The panoramaSkyboxMaterial's shader should be the one in \"Skybox/...\" catelogy")] [EnableIf(nameof(isOverridePanoramicSkybox))] [AllowNesting] [JsonIgnore] public Material panoramaSkyboxMaterial;
		///Skybox/Panoramic Shader中的全景图。（PS：Panorama类型的图片不要选中 "generate mipmaps"，否则会产生缝（外部加载的图片默认都不会生成））
		[EnableIf(nameof(isOverridePanoramicSkybox))] [AllowNesting] [JsonIgnore] public Texture defaultPanoramaTexture;
		[EnableIf(nameof(isOverridePanoramicSkybox))] [ReadOnly] [AllowNesting] [JsonIgnore] public Texture externalPanoramaTexture;
		[EnableIf(nameof(isOverridePanoramicSkybox))] [AllowNesting] [PersistentAssetFilePath(nameof(externalPanoramaTexture), true)] public string externalPanoramaTextureFilePath;
		[EnableIf(nameof(isOverridePanoramicSkybox))] [AllowNesting] [Range(0, 360)] public float panoramaSkyboxRotation = 0;
		[HideInInspector] [JsonIgnore] [PersistentDirPath] public string PersistentDirPath;

		#region Callback
		void OnPersistentValueChanged_IsOverrideLights(PersistentChangeState persistentChangeState)
		{
			actionIsOverrideLightsChanged.Execute(persistentChangeState);
		}
		void OnPersistentValueChanged_IsOverrideSkybox(PersistentChangeState persistentChangeState)
		{
			actionIsOverrideSkyboxChanged.Execute(persistentChangeState);
		}
		void OnPersistentChanged(PersistentChangeState persistentChangeState)
		{
			actionPersistentChanged.Execute(persistentChangeState);
		}
		#endregion

		#region NaughtAttribute
		bool isOverrideDefaultSkybox { get { return isOverrideSkybox && skyboxType == SkyboxType.Default; } }
		bool isOverridePanoramicSkybox { get { return isOverrideSkybox && skyboxType == SkyboxType.Panoramic; } }
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

	#region Editor Method
#if UNITY_EDITOR
	//——MenuItem——
	static string instName = "DefaultEnvironmentController";
	[UnityEditor.MenuItem(AC_EditorDefinition.HierarchyMenuPrefix_Cursor_Controller_Environment + "Default", false)]
	public static void CreateInst()
	{
		Threeyes.Editor.EditorTool.CreateGameObjectAsChild<AC_DefaultEnvironmentController>(instName);
	}
#endif
	#endregion
}