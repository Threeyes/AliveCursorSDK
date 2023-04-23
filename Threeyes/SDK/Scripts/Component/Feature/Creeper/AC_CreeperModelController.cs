using System.Collections;
using System.Collections.Generic;
using Threeyes.Coroutine;
using UnityEngine;
/// <summary>
/// Control Creeper's state
/// </summary>
public class AC_CreeperModelController : MonoBehaviour
	, IAC_CommonSetting_IsAliveCursorActiveHandler
	, IAC_CursorState_ChangedHandler
	, IAC_CommonSetting_CursorSizeHandler
	, IAC_SystemWindow_ChangedHandler
{
	public Transform tfParent;//Parent of this gameobject, Control Model Scale (Default scale must be one)
	public AC_CreeperTransformController creeperTransformController;

	#region Callback
	public void OnIsAliveCursorActiveChanged(bool isActive)
	{
		if (isActive)
			Resize();
		else
			gameObject.SetActive(false);
	}

	bool isLastHidingState;
	public void OnCursorStateChanged(AC_CursorStateInfo cursorStateInfo)
	{
		//在相关隐藏State时，临时隐藏该物体
		bool isCurHidingState =AC_ManagerHolder.StateManager.IsVanishState(cursorStateInfo.cursorState);
		if (isCurHidingState)
		{
			TryStopCoroutine_Resize();
			gameObject.SetActive(false);
		}
		else
		{
			if (isLastHidingState)//只有从隐藏切换到显示，才需要更新
				Resize();
		}
		isLastHidingState = isCurHidingState;
	}

	public void OnCursorSizeChanged(float value)
	{
		Resize();
	}

	public void OnWindowChanged(AC_WindowEventExtArgs e)
	{
		if(e.stateChange== AC_WindowEventExtArgs.StateChange.After)
		{
			creeperTransformController.Teleport();
		}
	}
	#endregion

	protected Coroutine cacheEnumResize;
	public void Resize()
	{
		TryStopCoroutine_Resize();
		cacheEnumResize = CoroutineManager.StartCoroutineEx(IEResize());
	}
	protected virtual void TryStopCoroutine_Resize()
	{
		if (cacheEnumResize != null)
			CoroutineManager.StopCoroutineEx(cacheEnumResize);
	}
	IEnumerator IEResize()
	{
		//让Rig相关组件强制更新(缩放后需要重新显隐，否则RigBuilder不会更新)
		gameObject.SetActive(false);
		Vector3 targetScale =  Vector3.one * AC_ManagerHolder.CommonSettingManager.CursorSize;//同步缩放Leg组

		//直接缩放父物体
		tfParent.localScale = targetScale;

		//更新关节
		creeperTransformController.MoveAllLeg();
		yield return null;//等待缩放不为0才能激活，否则会报错
		gameObject.SetActive(true);
	}
}
