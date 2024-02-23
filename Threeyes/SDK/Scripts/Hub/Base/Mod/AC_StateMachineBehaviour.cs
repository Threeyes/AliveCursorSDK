using System.Collections;
using System.Collections.Generic;
using Threeyes.Core;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 与Action联通
/// 
/// Warning:
/// 1.如果出现多次调用，则可能是因为错误地多次Add这个组件。打开Inspector——Debug模式，找到StateMachineBehaviours，删除多余的组件
/// 2.只有当对应的SoAction激活后，Complete才能正常执行
/// 3.Common_StateCompleted的Trigger适用于一个State完成后会自动跳转到下一个State（如Enter->Working)
/// </summary>
public class AC_StateMachineBehaviour : StateMachineBehaviour
{
	public static event UnityAction<AC_CursorStateInfoEx> CursorStateChanged;
	public static event UnityAction StateManager_SetCommonStateCompleted;

	public bool SetCompleteTriggerOnEnterCompleted = false;
	public AC_CursorState cursorState = AC_CursorState.None;

	/// <summary>
	/// Called on the first Update frame when a state machine evaluate this state.
	/// </summary>
	/// <param name="animator"></param>
	/// <param name="stateInfo"></param>
	/// <param name="layerIndex"></param>
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		DebugLog(cursorState.ToString() + " Enter");

		//ToUpdate:固定每个状态Tween的过渡时长，通过静态List决定哪些是需要SetTrigger
		UnityAction actOnComplete =
		() =>
		{
			//完成后自动过渡到下一步
			if (SetCompleteTriggerOnEnterCompleted)
			{
				StateManager_SetCommonStateCompleted.Execute();
				//AC_ManagerHolder.StateManager.SetCommonStateCompleted();
			}
		};
		CursorStateChanged.Execute(new AC_CursorStateInfoEx(cursorState, AC_CursorStateInfo.StateChange.Enter, actOnComplete));
	}

	/// <summary>
	/// Called on the last Update frame when making a transition out of a StateMachine.
	/// </summary>
	/// <param name="animator"></param>
	/// <param name="stateInfo"></param>
	/// <param name="layerIndex"></param>
	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		DebugLog(cursorState.ToString() + " Exit");
		CursorStateChanged.Execute(new AC_CursorStateInfoEx(cursorState, AC_CursorStateInfo.StateChange.Exit));
	}

#if UNITY_EDITOR
	bool isDebugLog =
	//true;
	false;
#endif
	void DebugLog(string content)
	{
#if UNITY_EDITOR
		if (isDebugLog)
			Debug.Log(content + " " + Time.realtimeSinceStartup);
#endif
	}
}