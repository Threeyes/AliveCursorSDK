using System.Collections;
using System.Collections.Generic;
using Threeyes.Coroutine;
using UnityEngine;
/// <summary>
/// Control creeper's model
/// </summary>
public class AC_CreeperModelController : MonoBehaviour
	, IAC_ModHandler
	, IAC_CommonSetting_IsAliveCursorActiveHandler
	, IAC_CursorState_ChangedHandler
	, IAC_CommonSetting_CursorSizeHandler
	, IAC_SystemWindow_ChangedHandler
{
	public AC_CreeperTransformController creeperTransformController;

	#region Callback
	public void OnModInit()
	{
		Resize();
	}
	public void OnModDeinit() { }

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
		bool isCurHidingState = IsHidingState(cursorStateInfo.cursorState);
		if (isCurHidingState)
		{
			TryStopResizeCoroutine();
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
		TryStopResizeCoroutine();
		cacheEnumResize = CoroutineManager.StartCoroutineEx(IEResize());
	}
	protected virtual void TryStopResizeCoroutine()
	{
		if (cacheEnumResize != null)
			CoroutineManager.StopCoroutineEx(cacheEnumResize);
	}
	IEnumerator IEResize()
	{
		//让Rig相关组件强制更新(缩放后需要重新显隐，否则RigBuilder不会更新)
		gameObject.SetActive(false);
		Vector3 targetScale =  Vector3.one * AC_ManagerHolder.CommonSettingManager.CursorSize;//同步缩放Leg组
		transform.localScale = targetScale;
		creeperTransformController.SetLocalScale(targetScale);

		//更新关节
		creeperTransformController.MoveAllLeg();
		//yield return new WaitForSeconds(0.1f);//等待缩放不为0才能激活，否则会报错
		yield return null;
		gameObject.SetActive(true);
	}

	static bool IsHidingState(AC_CursorState cursorState)//（ToUpdate：改为通用方法）
	{
		return cursorState == AC_CursorState.Exit || cursorState == AC_CursorState.Hide || cursorState == AC_CursorState.StandBy;
	}
}
