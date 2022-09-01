using NaughtyAttributes;
using Newtonsoft.Json;
using System;
using Threeyes.Persistent;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
/// <summary>
///Control PostProcessing Setting
///
/// Note:
/// 1.只提供最常见且URP可用的Effect，如有特定需求，请重新定义一个Controller
/// </summary>

[AddComponentMenu(AC_EditorDefinition.ComponentMenuPrefix_AC_Cursor_Controller + "AC_DefaultPostProcessingController")]
public class AC_DefaultPostProcessingController : AC_PostProcessingControllerBase<AC_SODefaultPostProcessingControllerConfig, AC_DefaultPostProcessingController.ConfigInfo>
{
	public override bool IsUsePostProcessing { get { return Config.isUsePostProcessing; } }

	[Header("PostProcessing")]
	[Tooltip("The PostProcessing volume")] [Required] [SerializeField] protected Volume volume;

	#region Unity Method
	private void Awake()
	{
		Config.actionPersistentChanged += OnPersistentChanged;//Get called at last
	}
	private void OnDestroy()
	{
		Config.actionPersistentChanged -= OnPersistentChanged;
	}
	#endregion

	#region Config Callback
	void OnPersistentChanged(PersistentChangeState persistentChangeState)
	{
		SetPostProcessing(Config.isUsePostProcessing);
	}
	#endregion


	#region Override
	Bloom bloom = null;
	public override void SetPostProcessing(bool isUse)
	{
		if (!volume)
			return;
		if (!volume.profile)
			return;

		volume.gameObject.SetActive(isUse);
		//Change Volume Layer
		volume.profile.TryGet(out bloom);
		if (bloom)
		{
			bloom.active = Config.bloom_IsActive;
			bloom.threshold.value = Config.bloom_Threshold;
			bloom.intensity.value = Config.bloom_Intensity;
			bloom.scatter.value = Config.bloom_Scatter;
			bloom.clamp.value = Config.bloom_Clamp;
			bloom.tint.value = Config.bloom_Tint;
		}

		base.SetPostProcessing(isUse);//Notify Hub Manager to update
	}
	#endregion

	#region Define
	[Serializable]
	[PersistentChanged(nameof(ConfigInfo.OnPersistentChanged))]
	public class ConfigInfo : AC_SerializableDataBase
	{
		[JsonIgnore] public UnityAction<PersistentChangeState> actionIsUsePostProcessingChanged;
		[JsonIgnore] public UnityAction<PersistentChangeState> actionPersistentChanged;

		[PersistentValueChanged(nameof(OnPersistentValueChanged_IsUsePostProcessing))] public bool isUsePostProcessing = false;

		//Note:
		//1. 命名参考AC_CommonSettingConfigInfo，以类型开头
		//2.部分字段没有最大值，那就不使用Range，因为系统会自动裁剪
		//3.如果模块下的子字段没有被勾选，那更改其值就不会影响效果（需要Modder提前勾选想要的选项）
		[Header("Bloom")]//https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@12.1/manual/post-processing-bloom.html
		[EnableIf(nameof(isUsePostProcessing))] [AllowNesting] public bool bloom_IsActive = false;
		[Tooltip("Filters out pixels under this level of brightness. Value is in gamma-space.")] [EnableIf(nameof(isBloomValid))] [AllowNesting] public float bloom_Threshold = 0.9f;
		[Tooltip("Strength of the bloom filter.")] [EnableIf(nameof(isBloomValid))] [AllowNesting] public float bloom_Intensity = 0f;
		[Tooltip("Set the radius of the bloom effect")] [EnableIf(nameof(isBloomValid))] [AllowNesting] [Range(0, 1)] public float bloom_Scatter = 0.7f;
		[Tooltip("Use the color picker to select a color for the Bloom effect to tint to.")] [EnableIf(nameof(isBloomValid))] [AllowNesting] public Color bloom_Tint = Color.white;
		[Tooltip("Set the maximum intensity that Unity uses to calculate Bloom. If pixels in your Scene are more intense than this, URP renders them at their current intensity, but uses this intensity value for the purposes of Bloom calculations.")] [EnableIf(nameof(isBloomValid))] [AllowNesting] public float bloom_Clamp = 65472f;

		#region Callback
		void OnPersistentValueChanged_IsUsePostProcessing(PersistentChangeState persistentChangeState)
		{
			actionIsUsePostProcessingChanged.Execute(persistentChangeState);
		}
		void OnPersistentChanged(PersistentChangeState persistentChangeState)
		{
			actionPersistentChanged.Execute(persistentChangeState);
		}
		#endregion

		#region NaughtAttribute
		bool isBloomValid { get { return isUsePostProcessing && bloom_IsActive; } }


		#endregion
	}
	#endregion


	#region Editor Method
#if UNITY_EDITOR
	//——MenuItem——
	static string instName = "DefaultPostProcessingController";
	[UnityEditor.MenuItem(AC_EditorDefinition.HierarchyMenuPrefix_Cursor_Controller_PostProcessing + "Default", false)]
	public static void CreateInst()
	{
		Threeyes.Editor.EditorTool.CreateGameObjectAsChild<AC_DefaultPostProcessingController>(instName);
	}
#endif
	#endregion
}
