using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AC_EnvironmentManagerBase<T> : AC_ManagerWithControllerBase<T, IAC_EnvironmentController, AC_DefaultEnvironmentController>, IAC_EnvironmentManager
where T : AC_EnvironmentManagerBase<T>
{
	#region Interface
	public Camera MainCamera { get { return mainCamera; } }// 1.为了方便Modder计算可视区域，只能暴露Camera给用户，但是要提醒用户注意还原相机！否则其FOV等属性可能会被篡改（ToUpdate：可以通过将Camera放到一个子场景中，每次加载Mod就重置子场景）

	/// <summary>
	/// Turn on/off Smear Effect
	/// 
	/// PS：
	/// 1.可以保留，但是需要Modder自行处理好，每次打开新Mod时Manager都要重置
	/// 2.使用bool是为了限制用户的操作，不允许用户直接修改该值，否则可能会被设置为其他值导致出错（如被设置为Skybox或Color）
	/// </summary>
	/// <param name="isOn"></param>
	public void SetSmearEffectActive(bool isOn)
	{
		//PS:URP中，Background Type对应的是旧版的ClearFlag
		mainCamera.clearFlags = isOn ? CameraClearFlags.Nothing : CameraClearFlags.SolidColor;
	}

	#endregion

	#region Property & Field
	[SerializeField] protected Camera mainCamera;
	#endregion

	#region Unity Method
	private void Awake()
	{
		//开始时监听一次
		defaultController.IsUseReflectionChanged += OnIsUseReflectionChanged;
		defaultController.IsUseLightsChanged += OnIsUseLightsChanged;
		defaultController.IsUseSkyboxChanged += OnIsUseSkyboxChanged;
	}
	private void OnDestroy()
	{
		defaultController.IsUseReflectionChanged -= OnIsUseReflectionChanged;
		defaultController.IsUseLightsChanged -= OnIsUseLightsChanged;
		defaultController.IsUseSkyboxChanged -= OnIsUseSkyboxChanged;
	}
	#endregion


	#region Callback
	//！！！ToUpdate:应该减少复杂度：如果使用了ModController，那就完全停用DefaultController
	public virtual void OnModInit(Scene scene, AC_AliveCursor aliveCursor)
	{
		//重置
		SetSmearEffectActive(false);//重置设置，以免用户忘记重置导致影响下一Mod

		//设置Mod环境
		modController = scene.GetComponents<IAC_EnvironmentController>().FirstOrDefault();
		defaultController.gameObject.SetActive(modController == null);//两者互斥
		if (modController != null)//Mod有自定义EnvironmentController：更新Environment
		{
			//监听ModController的设置回调
			modController.IsUseReflectionChanged += OnIsUseReflectionChanged;
			modController.IsUseLightsChanged += OnIsUseLightsChanged;
			modController.IsUseSkyboxChanged += OnIsUseSkyboxChanged;
		}

		ActiveController.OnModControllerInit();//初始化
	}
	public virtual void OnModDeinit(Scene scene, AC_AliveCursor aliveCursor)
	{
		modController?.OnModControllerDeinit();
		modController = null;//重置，否则会有引用残留
	}
	#endregion

	#region Controller Callback
	void OnIsUseReflectionChanged(bool isUse)
	{
	}
	void OnIsUseLightsChanged(bool isModFieldActive)
	{
	}
	void OnIsUseSkyboxChanged(bool isModFieldActive)
	{
	}
	#endregion
}
