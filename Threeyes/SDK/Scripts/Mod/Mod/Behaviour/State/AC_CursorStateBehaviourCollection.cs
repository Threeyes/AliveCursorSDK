using NaughtyAttributes;
using Threeyes.Core;
using Threeyes.Core.Editor;
using UnityEngine;
using StateChange = AC_CursorStateInfo.StateChange;

[AddComponentMenu(AC_EditorDefinition.ComponentMenuPrefix_Root_Mod_Behaviour_State + "AC_CursorStateBehaviourCollection", 1)]
public class AC_CursorStateBehaviourCollection : AC_BehaviourCollectionBase<AC_SOCursorStateActionCollection>
	, IAC_CursorState_ChangedHandler
{
	#region Property & Field
	//Action target for related CursorState (can be null)
	[Foldout(foldoutName_ActionTarget)] public GameObject actionTargetEnterState;
	[Foldout(foldoutName_ActionTarget)] public GameObject actionTargetExitState;
	[Foldout(foldoutName_ActionTarget)] public GameObject actionTargetShowState;
	[Foldout(foldoutName_ActionTarget)] public GameObject actionTargetHideState;
	[Foldout(foldoutName_ActionTarget)] public GameObject actionTargetWorkingState;
	[Foldout(foldoutName_ActionTarget)] public GameObject actionTargetStandByState;
	[Foldout(foldoutName_ActionTarget)] public GameObject actionTargetBoredState;
	[Foldout(foldoutName_ActionTarget)] public GameObject actionTargetVanishState;

	//Triggered when the target CursorState enter/exit
	[Foldout(foldoutName_UnityEvent)] public BoolEvent onEnterStateEnterExit;
	[Foldout(foldoutName_UnityEvent)] public BoolEvent onExitStateEnterExit;
	[Foldout(foldoutName_UnityEvent)] public BoolEvent onShowStateEnterExit;
	[Foldout(foldoutName_UnityEvent)] public BoolEvent onHideStateEnterExit;
	[Foldout(foldoutName_UnityEvent)] public BoolEvent onWorkingStateEnterExit;
	[Foldout(foldoutName_UnityEvent)] public BoolEvent onStandByStateEnterExit;
	[Foldout(foldoutName_UnityEvent)] public BoolEvent onBoredStateEnterExit;
	
	//Mixed
	[Foldout(foldoutName_UnityEvent)] public BoolEvent onCursorAppearDisappear;//Cursor disappeared on state[Exit, Hide, StandBy]

	#endregion

	#region Callback
	public void OnCursorStateChanged(AC_CursorStateInfo cursorStateInfo)
	{
		AC_CursorState curCursorState = cursorStateInfo.cursorState;

		switch (curCursorState)
		{
			case AC_CursorState.Enter: InvokeBehaviour(actionTargetEnterState, cursorStateInfo, onEnterStateEnterExit); break;
			case AC_CursorState.Exit: InvokeBehaviour(actionTargetExitState, cursorStateInfo, onExitStateEnterExit); break;
			case AC_CursorState.Show: InvokeBehaviour(actionTargetShowState, cursorStateInfo, onShowStateEnterExit); break;
			case AC_CursorState.Hide: InvokeBehaviour(actionTargetHideState, cursorStateInfo, onHideStateEnterExit); break;
			case AC_CursorState.Working: InvokeBehaviour(actionTargetWorkingState, cursorStateInfo, onWorkingStateEnterExit); break;
			case AC_CursorState.StandBy: InvokeBehaviour(actionTargetStandByState, cursorStateInfo, onStandByStateEnterExit); break;
			case AC_CursorState.Bored: InvokeBehaviour(actionTargetBoredState, cursorStateInfo, onBoredStateEnterExit); break;
		}

		bool isVanishState = AC_ManagerHolder.StateManager.IsVanishState(curCursorState);
		onCursorAppearDisappear.Invoke(!isVanishState);
	}
	#endregion

	#region Virtual
	protected virtual void InvokeBehaviour(GameObject goTarget, AC_CursorStateInfo cursorStateInfo, BoolEvent boolEvent)
	{
		AC_CursorState curCursorState = cursorStateInfo.cursorState;
		StateChange stateChange = cursorStateInfo.stateChange;

		if (soActionCollection && goTarget)
		{
			var soAction = soActionCollection[curCursorState];
			if (soAction)
			{
				if (stateChange == StateChange.Enter)
					soAction.Enter(true, goTarget);
				else if (stateChange == StateChange.Exit)
					soAction.Enter(false, goTarget);
			}
		}

		//Still invoke event even if the soActionCollection is not valid
		if (stateChange == StateChange.Enter)
			boolEvent.Invoke(true);
		else if (stateChange == StateChange.Exit)
			boolEvent.Invoke(false);
	}

	#endregion

	#region Editor Method
#if UNITY_EDITOR
	//——MenuItem——
	static string instName = "CSB_Collection ";
	[UnityEditor.MenuItem(AC_EditorDefinition.HierarchyMenuPrefix_Root_Mod_Behaviour_State + "CursorStateBehaviourCollection", false, 1)]
	public static void CreateInst()
	{
        EditorTool.CreateGameObjectAsChild<AC_CursorStateBehaviourCollection>(instName);
	}
#endif
	#endregion
}