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
	[SerializeField] protected ReflectionProbe reflectionProbe;//全局的ReflectionProbe（类似RenderSettings.skybox）（Mod可按需自行实现ReflectionProbe）
	#endregion

	#region Callback
	public void OnModInit(Scene scene, AC_AliveCursor aliveCursor)
	{
		SetSmearEffectActive(false);//重置设置，以免用户忘记重置导致影响下一Mod

		//设置Mod环境
		modController = scene.GetComponents<IAC_EnvironmentController>().FirstOrDefault();
		if (modController != null)//Mod有自定义EnvironmentController：更新Environment
		{
			//监听后续用户通过PD修改设置的操作
			modController.IsOverrideLightsChanged += OnModIsOverrideLightsChanged;
			modController.IsOverrideSkyboxChanged += OnModIsOverrideSkyboxChanged;
			modController.OnModControllerInit();
		}
		else//Mod无定义环境：使用defaultEnvironmentController.ModInit的默认设置
		{
			defaultController.OnModControllerInit();
		}
	}
	public void OnModDeinit(Scene scene, AC_AliveCursor aliveCursor)
	{
		modController?.OnModControllerDeinit();
		modController = null;//记得重置，否则会有引用残留
	}
	#endregion

	#region Mod Callback
	void OnModIsOverrideLightsChanged(bool isModFieldActive)
	{
		defaultController.InitLights(!isModFieldActive);//两者互斥
	}
	void OnModIsOverrideSkyboxChanged(bool isModFieldActive)
	{
		defaultController.InitSkybox(!isModFieldActive);
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
	public void DynamicGIUpdateEnvironment()
	{
		RuntimeTool.ExecuteOnceInCurFrameAsync(DynamicGI.UpdateEnvironment);//Update environment cubemap
		RefreshReflectionProbe();
	}

	bool isReflectionProbeActived = false;//Cache state
	public void SetReflectionProbeActive(bool isActive)
	{
		bool activeStateChanged = isReflectionProbeActived != isActive;
		isReflectionProbeActived = isActive;
		reflectionProbe.transform.position = isActive ? Vector3.zero : new Vector3(0, 0, -10000);//Warning:不能直接禁用reflectionProbe，因为其重新激活后无法正常Render。因此将隐藏实现改为移动到不可见区域外

		if (isActive && activeStateChanged)//在重新激活时要重新刷新
			RefreshReflectionProbe();
	}

	/// <summary>
	/// Update ReflectionProbe to refresh reflection
	/// </summary>
	void RefreshReflectionProbe()
	{
		if (!isReflectionProbeActived)//PS:未激活时调用无效
			return;
		RuntimeTool.ExecuteOnceInCurFrameAsync(() => reflectionProbe.RenderProbe());//PS:RenderProbe会返回ID，可用于后续检查Render完成时间
	}

	#endregion

}
