using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
/// <summary>
///
/// PS:
/// 1.整体流程参考AC_EnvironmentManagerBase
/// </summary>
/// <typeparam name="T"></typeparam>
public class AC_PostProcessingManagerBase<T> : AC_ManagerWithControllerBase<T, IAC_PostProcessingController, AC_DefaultPostProcessingController>, IAC_PostProcessingManager
	where T : AC_PostProcessingManagerBase<T>
{
	#region Unity Method
	private void Awake()
	{
		//开始时监听一次
		defaultController.IsUsePostProcessingChanged += OnIsUsePostProcessingChanged;
	}
	private void OnDestroy()
	{
		defaultController.IsUsePostProcessingChanged -= OnIsUsePostProcessingChanged;
	}
	#endregion

	#region Callback
	public void OnModInit(Scene scene, AC_AliveCursor aliveCursor)
	{
		modController = scene.GetComponents<IAC_PostProcessingController>().FirstOrDefault();
		defaultController.gameObject.SetActive(modController == null);//两者互斥
		if (modController != null)//Mod有自定义EnvironmentController：更新Environment
		{
			//监听ModController是否OverrideDefaultController的设置
			modController.IsUsePostProcessingChanged += OnIsUsePostProcessingChanged;
		}

		ActiveController.OnModControllerInit();//初始化
	}
	public void OnModDeinit(Scene scene, AC_AliveCursor aliveCursor)
	{
		modController?.OnModControllerDeinit();
		modController = null;//重置，否则会有引用残留
	}
	#endregion

	#region Controller Callback
	void OnIsUsePostProcessingChanged(bool isUse)
	{
		var uacData = AC_ManagerHolder.EnvironmentManager.MainCamera.GetComponent<UniversalAdditionalCameraData>();
		uacData.renderPostProcessing = isUse;
		uacData.antialiasing = isUse ? AntialiasingMode.None : AntialiasingMode.FastApproximateAntialiasing;//PS: FXAA会导致PP的透明度失效，因此两者互斥
	}
	#endregion
}
