using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Threeyes.Action;
using Threeyes.Coroutine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class AC_StateManagerBase<T> : AC_ManagerWithControllerBase<T, IAC_StateController, AC_DefaultStateController>
	, IAC_StateManager
	, IAC_SystemCursor_AppearanceChangedHandler
	, IAC_CommonSetting_IsStandByActiveHandler
	, IAC_CommonSetting_StandByDelayTimeHandler
	, IAC_CommonSetting_IsBoredActiveHandler
	, IAC_CommonSetting_BoredDelayTimeHandler
	where T : AC_StateManagerBase<T>
{
	#region Interface
	public AC_CursorStateInfo CurCursorStateInfo { get { return curCursorStateInfo; } }
	public AC_CursorState CurCursorState { get { return curCursorState; } }//单独以字段保存，而不是从curCursorStateInfo中获取，避免空引用或引用被修改
	public AC_CursorState LastCursorState { get { return lastCursorState; } }
	public bool IsCurStateActionComplete(ActionState actionState)
	{
		return ActiveController.IsCurStateActionComplete(actionState);
	}
	#endregion

	#region Property & Field
	[SerializeField] protected Animator animator;//统一放在Manager进行管理

	[Header("Run time")]
	[SerializeField] protected AC_CursorStateInfo curCursorStateInfo;
	[SerializeField] protected AC_CursorState curCursorState = AC_CursorState.None;
	[SerializeField] protected AC_CursorState lastCursorState = AC_CursorState.None;
	#endregion

	#region Unity Method
	protected virtual void Awake()
	{
		AC_StateMachineBehaviour.CursorStateChanged += OnStateMachineStateChange;
		AC_StateMachineBehaviour.StateManager_SetCommonStateCompleted += SetAnimatorTrigger_Common_StateCompleted;
	}
	protected virtual void OnDestroy()
	{
		AC_StateMachineBehaviour.CursorStateChanged -= OnStateMachineStateChange;
		AC_StateMachineBehaviour.StateManager_SetCommonStateCompleted -= SetAnimatorTrigger_Common_StateCompleted;
	}
	#endregion

	#region Callback
	public void OnModInit(Scene scene, AC_AliveCursor aliveCursor)
	{
		modController = aliveCursor.GetComponent<IAC_StateController>();//尝试获取
																		//ToAdd：调用Controller.SetState以便同步调用SOAction到最新状态
	}

	public void OnModDeinit(Scene scene, AC_AliveCursor aliveCursor)
	{
		//ToAdd
	}

	//Setting
	protected bool commonSetting_IsStandByActive;
	protected float commonSetting_StandByDelayTime;
	protected bool commonSetting_IsBoredActive;
	protected float commonSetting_BoredDelayTime;
	public void OnIsStandByActiveChanged(bool isActive)
	{
		commonSetting_IsStandByActive = isActive;
	}
	public void OnStandByDelayTimeChanged(float value)
	{
		commonSetting_StandByDelayTime = value;
	}
	public void OnIsBoredActiveChanged(bool isActive)
	{
		commonSetting_IsBoredActive = isActive;
	}
	public void OnBoredDelayTimeChanged(float value)
	{
		commonSetting_BoredDelayTime = value;
	}

	//Appearance
	protected bool isSystemCursorShown = false;//缓存系统光标的激活状态
	public void OnSystemCursorAppearanceChanged(bool isSystemCursorShowing, AC_SystemCursorAppearanceInfo systemCursorAppearanceInfo)
	{
		isSystemCursorShown = isSystemCursorShowing;

		if (Application.isEditor && isDebugStayActive)
			return;

		SetAnimatorBool(isSystemCursorShowing ? AC_CursorState.Show : AC_CursorState.Hide);
	}

	//State
	protected static readonly List<AC_CursorState> listShouldFollowSystemCursorState = new List<AC_CursorState>() { AC_CursorState.Working, AC_CursorState.StandBy, AC_CursorState.Bored };//代表鼠标需要跟随系统光标的状态
	protected Coroutine cacheEnWorking;
	void OnStateMachineStateChange(AC_CursorStateInfoEx cursorStateInfo)//Control Animator's State changed
	{
		curCursorStateInfo = cursorStateInfo;
		if (cursorStateInfo.cursorState != curCursorState)//缓存变更前的状态
		{
			lastCursorState = curCursorState;
		}
		curCursorState = cursorStateInfo.cursorState;

		if (cursorStateInfo.cursorState == AC_CursorState.Working)//当Working Enter后，启动线程计时
		{
			TryStopCoroutine(cacheEnWorking);
			cacheEnWorking = CoroutineManager.StartCoroutineEx(IEDetectMouseNotMove());
		}
		else if (!listShouldFollowSystemCursorState.Contains(curCursorState))//结束计时：重置
		{
			TryStopCoroutine(cacheEnWorking, () => SetAnimatorBool(false, false));
		}
		AC_EventCommunication.SendMessage<IAC_CursorState_ChangedHandler>((inst) => inst.OnCursorStateChanged(cursorStateInfo));
		ActiveController.SetState(cursorStateInfo);
	}
	protected void TryStopCoroutine(Coroutine en, UnityAction action = null)
	{
		if (en != null)
		{
			CoroutineManager.StopCoroutineEx(en);
			action.Execute();
		}
	}
	IEnumerator IEDetectMouseNotMove()
	{
		float lastMouseInputTime = LastMouseInputEventTime;
		float lastMouseOrKeyInputStartTime = LastAnyInputEventTime;

		while (true)
		{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
			if (AC_EditorSetting.GetDebugMode(isDebugIgnoreInput))
				yield break;
#endif

			if (!isAliveCursorActived)//等待AC被激活
				yield return null;

			if (!isSystemCursorShown)//等待系统光标被激活
				yield return null;

			if (!this)
				yield break;

			//——StandBy——
			//PS:以下状态的时间都是基于Work。响应鼠标移动，忽略鼠标滚轮和按键点击，给用户一个沉浸式体验
			lastMouseInputTime = Mathf.Max(lastMouseInputTime, LastMouseInputEventTime);
			noMouseInputTimeSinceWorking = Time.time - lastMouseInputTime;

			//——Bored——
			//PS:以下状态的时间都是基于先前的状态(Work or Work+StandBy)，如果激活StandBy，则需要减去该状态占用的时间。响应任何鼠标和键盘输入
			lastMouseOrKeyInputStartTime = Mathf.Max(lastMouseOrKeyInputStartTime, LastAnyInputEventTime);
			notMouseOrKeyInputTimeSinceStandBy = Time.time - (commonSetting_IsStandByActive ? lastMouseOrKeyInputStartTime + commonSetting_StandByDelayTime : lastMouseOrKeyInputStartTime);//该值可能会小于0，但不影响后续判断

			//Bug ToFix:打开TaskManager时，除非以管理员运行，按键事件不更新（https://social.msdn.microsoft.com/Forums/en-US/88fbf594-6d71-4fa5-9435-7bb2cef1c7e1/how-to-hook-mouse-and-keyboard-message-when-task-manager-is-active-window）（https://docs.microsoft.com/zh-cn/windows/powertoys/administrator）
			// Debug.LogError(noMouseInputTimeSinceWorking + " : " + notMouseOrKeyInputTimeSinceStandBy);
			SetAnimatorBool(noMouseInputTimeSinceWorking > commonSetting_StandByDelayTime, notMouseOrKeyInputTimeSinceStandBy > commonSetting_BoredDelayTime);//更新对应Animator
			yield return null;
		}
	}
	#endregion

	#region Public Method
	/// <summary>
	/// 强制切换AnimatorState，忽略跳转State Transition
	///
	/// 用途：
	/// 1.调用Enter/Exit
	/// 2.Debug
	/// </summary>
	/// <param name="targetCursorState"></param>
	public void ForceSetAnimatorState(AC_CursorState targetCursorState)
	{
		switch (targetCursorState)
		{
			case AC_CursorState.Show:
				SetAnimatorBool(false, false);
				SetAnimatorBool(AC_CursorState.Show);
				break;
			case AC_CursorState.Hide:
				SetAnimatorBool(false, false);
				SetAnimatorBool(AC_CursorState.Hide);
				break;

			case AC_CursorState.Working:
				SetAnimatorBool(false, false);
				SetAnimatorBool(AC_CursorState.Show);
				break;
			case AC_CursorState.StandBy:
				SetAnimatorBool(true, false);
				break;
			case AC_CursorState.Bored:
				SetAnimatorBool(false, true);
				break;
		}
		animator.CrossFade(targetCursorState.ToString(), 0);//强制跳转
	}
	#endregion

	#region Inner Method
	[ReadOnly] [SerializeField] float noMouseInputTimeSinceWorking = 0;
	[ReadOnly] [SerializeField] float notMouseOrKeyInputTimeSinceStandBy = 0;

	protected float LastMouseInputEventTime { get { return AC_ManagerHolder.SystemInputManager.LastMouseInputEventTime; } }
	protected float LastMouseWheelEventTime { get { return AC_ManagerHolder.SystemInputManager.LastMouseWheelEventTime; } }
	protected float LastKeyInputEventTime { get { return AC_ManagerHolder.SystemInputManager.LastKeyInputEventTime; } }
	protected float LastAnyInputEventTime { get { return Mathf.Max(LastMouseInputEventTime, LastMouseWheelEventTime, LastKeyInputEventTime); } }

	//Set Working/StandBy/Bored Param
	protected virtual void SetAnimatorBool(bool isStandBy, bool isBored)
	{
		SetAnimatorBool(AC_CursorState.StandBy, commonSetting_IsStandByActive ? isStandBy : false);
		SetAnimatorBool(AC_CursorState.Bored, commonSetting_IsBoredActive ? isBored : false);
	}
	//Set Bool Param
	protected virtual void SetAnimatorBool(AC_CursorState targetCursorState, bool? param = null)
	{
		string animName = "";
		bool? animValue = param;
		switch (targetCursorState)
		{
			case AC_CursorState.Show:
				animName = "Any_ShowCustomCursor";
				animValue = true;
				break;
			case AC_CursorState.Hide:
				animName = "Any_ShowCustomCursor";
				animValue = false;
				break;
			case AC_CursorState.StandBy:
				animName = "StandBy";
				break;
			case AC_CursorState.Bored:
				animName = "Bored";
				break;
		}

		if (animName.NotNullOrEmpty())
		{
			if (animValue.HasValue)
			{
				animator.SetBool(animName, animValue.Value);
			}
		}
	}
	protected virtual void SetAnimatorTrigger_Common_StateCompleted()
	{
		animator.SetTrigger("Common_StateCompleted");
	}
	#endregion

	#region Debug
	public bool isDebugNumberKeysChangeState { get { return isDebugTopNumberKeysChangeState || isDebugPadNumberKeysChangeState; } }
	[Foldout(foldoutName_Debug)] public bool isDebugStayActive = false;
	[Foldout(foldoutName_Debug)] public bool isDebugTopNumberKeysChangeState = false;//Number keys on the top of the alphanumeric keyboard (Active PadNumberKeys instead incase you need to change Inspector field via these keys)
	[Foldout(foldoutName_Debug)] public bool isDebugPadNumberKeysChangeState = false;//Keys on NumberPad
	[Header("Stop auto state switching via input (eg: from working to bored)!")] [Foldout(foldoutName_Debug)] [EnableIf(EConditionOperator.Or, nameof(isDebugTopNumberKeysChangeState), nameof(isDebugPadNumberKeysChangeState))] public bool isDebugIgnoreInput = false;//The Input won't affects State (Toggle via Alpha0 or Keypad0)

	readonly protected Dictionary<KeyCode, AC_CursorState> debugDicTopNumberKey2State = new Dictionary<KeyCode, AC_CursorState>()
	{
		{KeyCode.Alpha0,AC_CursorState.None},//特殊用途
		{KeyCode.Alpha1,AC_CursorState.Enter},
		{KeyCode.Alpha2,AC_CursorState.Exit},
		{KeyCode.Alpha3,AC_CursorState.Show},
		{KeyCode.Alpha4,AC_CursorState.Hide},
		{KeyCode.Alpha5,AC_CursorState.Working},
		{KeyCode.Alpha6,AC_CursorState.StandBy},
		{KeyCode.Alpha7,AC_CursorState.Bored},
	};
	readonly protected Dictionary<KeyCode, AC_CursorState> debugDicPadNumberKey2State = new Dictionary<KeyCode, AC_CursorState>()
	{
		{KeyCode.Keypad0,AC_CursorState.None},//特殊用途
		{KeyCode.Keypad1,AC_CursorState.Enter},
		{KeyCode.Keypad2,AC_CursorState.Exit},
		{KeyCode.Keypad3,AC_CursorState.Show},
		{KeyCode.Keypad4,AC_CursorState.Hide},
		{KeyCode.Keypad5,AC_CursorState.Working},
		{KeyCode.Keypad6,AC_CursorState.StandBy},
		{KeyCode.Keypad7,AC_CursorState.Bored},
	};
	protected virtual void DebugInputChangeStateFunc(KeyCode keyCode, AC_KeyState mouseKeyState, Dictionary<KeyCode, AC_CursorState> dicKey2State)
	{
		//ToUpdate:针对按下、抬起可以有不同效果

		if (mouseKeyState == AC_KeyState.Down)
		{
			if (!dicKey2State.ContainsKey(keyCode))
				return;

			if (keyCode == dicKey2State.Keys.First())//Alpha0 or Keypad0: Switch isDebugIgnoreInput's value
			{
				isDebugIgnoreInput = !isDebugIgnoreInput;
				Debug.Log($"[Debug] StateManager {(isDebugIgnoreInput ? "response to" : "ignore")}  Input!");
				return;
			}

			//按下键盘对应数字，切换模式
			//键位名称就是对应位置：https://docs.unity3d.com/2018.4/Documentation/Manual/ConventionalGameInput.html?_ga=2.204127448.93138317.1615045368-1235783780.1524321557
			AC_CursorState aC_CursorState = dicKey2State[keyCode];
			ForceSetAnimatorState(aC_CursorState);
			Debug.Log($"[Debug] StateManager change state to {aC_CursorState}");
		}
	}

	#endregion
}