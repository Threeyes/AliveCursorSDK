using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class AC_SystemCursorManagerBase<T> : AC_ManagerWithLifeCycleBase<T>
	, IAC_SystemCursorManager
	, IAC_CommonSetting_IsHideOnUnknownCursorHandler
	, IAC_CommonSetting_IsHideOnTextInputHandler,
	IAC_CommonSetting_BoredDepthHandler
	where T : AC_SystemCursorManagerBase<T>
{
	#region Interface
	public bool IsSystemCursorShowing { get { return isSystemCursorShowing; } protected set { isSystemCursorShowing = value; } }
	public virtual AC_SystemCursorAppearanceInfo CurSystemCursorAppearanceInfo { get { return curSystemCursorAppearanceInfo; } }
	public virtual AC_SystemCursorAppearanceInfo LastSystemCursorAppearanceInfo { get { return lastSystemCursorAppearanceInfo; } }

	public AC_SystemCursorAppearanceType CurSystemCursorAppearanceType { get { return curSystemCursorAppearanceType; } }
	public AC_SystemCursorAppearanceType LastSystemCursorAppearanceType { get { return lastSystemCursorAppearanceType; } }

	public virtual Vector3 WorldPosition
	{
		get
		{
#if UNITY_EDITOR
			if (IsDebugStayAtCenter)
			{
				return DebugGetFixPoint(curDepth);
			}
#endif
			return GetWorldPosition(curDepth);
		}
	}
	public virtual Vector3 MousePosition { get { return Input.mousePosition; } }////与new InputSystem Mouse.current.position.ReadValue结果相同
	public float CurDepth { get { return curDepth; } set { curDepth = value; } }
	public float WorkingStateCameraDepth { get { return 10f; } }//光标在Woring状态时相对相机的深度
	public Vector2 BoredStateCameraDepthRange { get { return new Vector2(WorkingStateCameraDepth, WorkingStateCameraDepth + commonSetting_BoredDepth); } }//光标在Bored状态时相对相机的深度范围
	public Vector2 BoredStateWorldZRange { get { return new Vector2(0, commonSetting_BoredDepth); } }
	public Vector3 GetRandomPointInsideBoredBounds()
	{
		//Ref：https://answers.unity.com/questions/752253/spawn-object-at-random-points-within-camera-view.html
		Vector3 viewPoint = new Vector3(Random.Range(0, 1), Random.Range(0, 1), Random.Range(BoredStateCameraDepthRange.x, BoredStateCameraDepthRange.y));//ViewPoint中X、Y的的范围为[0,1]，Z代表与相机的距离
		return MainCamera.ViewportToWorldPoint(viewPoint);
	}
	public bool IsInsideBoredBounds(Vector3 worldPosition)
	{
		Vector3 screenPoint = MainCamera.WorldToViewportPoint(worldPosition);
		if (screenPoint.z >= BoredStateCameraDepthRange.x && screenPoint.z <= BoredStateCameraDepthRange.y)//是否在深度范围内
		{
			return screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;//是否在屏幕边界内
		}
		return false;
	}

	/// <summary>
	/// Get mouse position on desire depth
	/// </summary>
	/// <param name="depth"></param>
	/// <returns></returns>
	public Vector3 GetWorldPosition(float depth)
	{
		Vector3 worldPos = MousePosition;
		worldPos.z = depth;
		return MainCamera.ScreenToWorldPoint(worldPos);
	}
	/// <summary>
	/// 检查指定体积的物体是否在相机视野内
	/// Ref：https://forum.unity.com/threads/solved-check-if-gameobject-without-renderer-is-within-camera-view.756506/#:~:text=You%20could%20always%20use%20https%3A%2F%2Fdocs.unity3d.com%2FScriptReference%2FCamera.WorldToViewportPoint.html%20and%20then%20check,positive%20Z%20value.%20That%20would%20indicate%20it%27s%20visible.
	/// </summary>
	/// <param name="pos"></param>
	/// <param name="boundSize"></param>
	/// <param name="camera"></param>
	/// <returns></returns>
	public bool IsVisible(Vector3 pos, Vector3 boundSize)
	{
		var bounds = new Bounds(pos, boundSize);
		var planes = GeometryUtility.CalculateFrustumPlanes(MainCamera);
		return GeometryUtility.TestPlanesAABB(planes, bounds);
	}
	#endregion

	#region Property & Field
	protected Camera MainCamera { get { if (!mainCamera) mainCamera = AC_ManagerHolder.EnvironmentManager.MainCamera; return mainCamera; } }
	Camera mainCamera;

	protected bool commonSetting_IsHideOnTextInput;
	protected bool commonSetting_IsHideOnUnknownCursor;
	protected float commonSetting_BoredDepth;
	protected float curDepth = 10;//需要设置为默认值，否则运行时会进行位置变换

	[Header("Appearance")]
	protected bool isSystemCursorShowing;
	protected AC_SystemCursorAppearanceType curSystemCursorAppearanceType;
	protected AC_SystemCursorAppearanceType lastSystemCursorAppearanceType;

	//Debug
	public bool IsDebugStayAtCenter { get => isDebugStayAtCenter; set => isDebugStayAtCenter = value; }//固定在屏幕中央
	[Foldout(foldoutName_Debug)] public bool isDebugStayAtCenter;
	#endregion

	#region Callback
	public void OnModInit(Scene scene, AC_AliveCursor aliveCursor)
	{
		SendMessage(CurSystemCursorAppearanceInfo, lastSystemCursorAppearanceInfo);//通知更新
	}
	public void OnModDeinit(Scene scene, AC_AliveCursor aliveCursor)
	{

	}
	public virtual void OnIsHideOnTextInputChanged(bool isActiveSystemCursor_IBeam)
	{
		commonSetting_IsHideOnTextInput = isActiveSystemCursor_IBeam;
		SetSystemCursorActive(AC_SystemCursorAppearanceType.IBeam, isActiveSystemCursor_IBeam);//单独更新：是否显示系统IBeam光标
	}

	public virtual void OnIsHideOnUnknownCursorChanged(bool isActive)
	{
		commonSetting_IsHideOnUnknownCursor = isActive;
		//ToAdd：调用SetSystemCursorActive
	}

	public virtual void OnBoredDepthChanged(float value)
	{
		commonSetting_BoredDepth = value;
	}

	protected override void OnIsAliveCursorActiveChangedFunc(bool isActive)
	{
		base.OnIsAliveCursorActiveChangedFunc(isActive);
		SetAllSystemCursorActive(!isActive);//反向显隐系统光标
	}
	#endregion

	#region Appearance
	bool isLastCursorShowing = false;
	bool hasChanged = false;

	protected AC_SystemCursorAppearanceInfo curSystemCursorAppearanceInfo;
	protected AC_SystemCursorAppearanceInfo lastSystemCursorAppearanceInfo;//缓存上次的值，便于发送事件
	protected virtual void UpdateAppearance(bool isTempSystemCursorShowing, AC_SystemCursorAppearanceInfo systemCursorAppearanceInfo)
	{
		curSystemCursorAppearanceInfo = systemCursorAppearanceInfo;

		isSystemCursorShowing = isTempSystemCursorShowing;

		//#1 更新 isCursorShowing、isLastCursorShowing
		if (systemCursorAppearanceInfo.systemCursorAppearanceType == AC_SystemCursorAppearanceType.None)//当前光标为未知（有可能是空，或者是程序/Game等非系统默认或自定义的光标）：隐藏
		{
			if (commonSetting_IsHideOnUnknownCursor)
				isSystemCursorShowing = false;
		}
		else//当前光标为已知
		{
			if (commonSetting_IsHideOnTextInput && systemCursorAppearanceInfo.systemCursorAppearanceType == AC_SystemCursorAppearanceType.IBeam)//当前为输入光标且设置符合：隐藏
			{
				isSystemCursorShowing = false;
			}
		}

		//#2 更新 curCursorAppearanceType	
		curSystemCursorAppearanceType = systemCursorAppearanceInfo.systemCursorAppearanceType;

		//#3 更新 hasChanged
		if (isLastCursorShowing != isSystemCursorShowing)
		{
			isLastCursorShowing = isSystemCursorShowing;
			hasChanged = true;
		}
		if (lastSystemCursorAppearanceType != curSystemCursorAppearanceType)
		{
			lastSystemCursorAppearanceType = curSystemCursorAppearanceType;
			hasChanged = true;
		}

		if (hasChanged)//PS：只有信息更改后才会发送消息
		{
			SendMessage(curSystemCursorAppearanceInfo, lastSystemCursorAppearanceInfo);
			hasChanged = false;
			lastSystemCursorAppearanceInfo = new AC_SystemCursorAppearanceInfo(curSystemCursorAppearanceInfo);//注意！因为是class，因此缓存的必须是Clone值！
		}
	}
	protected virtual void SendMessage(AC_SystemCursorAppearanceInfo curSystemCursorAppearanceInfo, AC_SystemCursorAppearanceInfo lastSystemCursorAppearanceInfo)
	{
		///PS:
		///1.在这里更新State，先发送旧的退出事件，再发新的进入事件，参考AC_CursorStateInfo
		//2.发送的应该是一个Clone，而不是原值，能够避免修改原值
		if (lastSystemCursorAppearanceInfo != null)
		{
			lastSystemCursorAppearanceInfo.stateChange = AC_SystemCursorAppearanceInfo.StateChange.Exit;
			AC_EventCommunication.SendMessage<IAC_SystemCursor_AppearanceChangedHandler>((inst) => inst.OnSystemCursorAppearanceChanged(isSystemCursorShowing, lastSystemCursorAppearanceInfo), includeHubScene: true);//包括AC_StateManager
		}
		curSystemCursorAppearanceInfo.stateChange = AC_SystemCursorAppearanceInfo.StateChange.Enter;
		AC_EventCommunication.SendMessage<IAC_SystemCursor_AppearanceChangedHandler>((inst) => inst.OnSystemCursorAppearanceChanged(isSystemCursorShowing, curSystemCursorAppearanceInfo), includeHubScene: true);//包括AC_StateManager
	}
	/// <summary>
	/// 启用/禁用 系统光标
	/// </summary>
	protected virtual void SetAllSystemCursorActive(bool isActive)
	{
		foreach (AC_SystemCursorAppearanceType cAT in System.Enum.GetValues(typeof(AC_SystemCursorAppearanceType)))
		{
			if (!isActive)//禁用时：
			{
				if (commonSetting_IsHideOnTextInput && cAT == AC_SystemCursorAppearanceType.IBeam)//根据设置，决定是否启用系统光标IBeam
					continue;
			}
			SetSystemCursorActive(cAT, isActive);
		}
	}
	protected virtual void SetSystemCursorActive(AC_SystemCursorAppearanceType cursorAppearanceType, bool isActive) { }
	#endregion

	#region Debug
	/// <summary>
	/// 获取Debug时的固定坐标
	/// </summary>
	/// <param name="depth"></param>
	/// <returns></returns>
	protected Vector3 DebugGetFixPoint(float depth)
	{
		return MainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 4 * 3, depth));//稍微居上，便于查看光标全貌
	}
	#endregion
}