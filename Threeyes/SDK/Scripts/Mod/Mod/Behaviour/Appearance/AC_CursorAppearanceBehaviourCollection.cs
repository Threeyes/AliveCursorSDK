using NaughtyAttributes;
using Threeyes.Core;
using Threeyes.Core.Editor;
using UnityEngine;
using StateChange = AC_SystemCursorAppearanceInfo.StateChange;

/// <summary>
/// Custom Behaviour when system cursor type changed
///
/// Ref: AC_CursorStateBehaviourCollection
///
/// PS:
/// 1.因为AC_SystemCursorAppearanceType太多，而且可能有后续自定义的事件，因此改为程序化获取
/// </summary>
[AddComponentMenu(AC_EditorDefinition.ComponentMenuPrefix_Root_Mod_Behaviour_Appearance + "AC_CursorAppearanceBehaviourCollection", 1)]
public class AC_CursorAppearanceBehaviourCollection : AC_BehaviourCollectionBase<AC_SOCursorAppearanceActionCollection>
	, IAC_SystemCursor_AppearanceChangedHandler
{
	#region Property & Field

	[Foldout(foldoutName_ActionTarget)] public GameObject actionTargetContent;//content to display all cursor appearance info

	//PS：将所有事件放到这里不太现实，应只放通用的事件

	#endregion

	#region Callback
	public virtual void OnSystemCursorAppearanceChanged(bool isSystemCursorShowing, AC_SystemCursorAppearanceInfo systemCursorAppearanceInfo)
	{
		InvokeBehaviour(GetActionTarget(systemCursorAppearanceInfo.systemCursorAppearanceType), isSystemCursorShowing, systemCursorAppearanceInfo);
	}
	#endregion

	#region Virtual
	protected virtual void InvokeBehaviour(GameObject goTarget, bool isSystemCursorShowing, AC_SystemCursorAppearanceInfo systemCursorAppearanceInfo, BoolEvent boolEvent = null)
	{
		AC_SystemCursorAppearanceType systemCursorAppearanceType = systemCursorAppearanceInfo.systemCursorAppearanceType;
		StateChange stateChange = systemCursorAppearanceInfo.stateChange;
		if (soActionCollection && goTarget)
		{
			var soAction = soActionCollection[systemCursorAppearanceType];
			if (soAction)
			{
				if (stateChange == StateChange.Enter)
					soAction.Enter(true, goTarget);
				else if (stateChange == StateChange.Exit)
					soAction.Enter(false, goTarget);
			}
		}

		if (boolEvent != null)
		{
			if (stateChange == StateChange.Enter)
				boolEvent.Invoke(true);
			else if (stateChange == StateChange.Exit)
				boolEvent.Invoke(false);
		}
	}
	protected virtual GameObject GetActionTarget(AC_SystemCursorAppearanceType systemCursorAppearanceType) { return actionTargetContent; }

	#endregion

	#region Editor Method
#if UNITY_EDITOR
	//——MenuItem——
	static string instName = "CAB_Collection ";
	[UnityEditor.MenuItem(AC_EditorDefinition.HierarchyMenuPrefix_Root_Mod_Behaviour_Appearance + "CursorAppearanceBehaviourCollection", false, 1)]
	public static void CreateInst()
	{
        EditorTool.CreateGameObjectAsChild<AC_CursorAppearanceBehaviourCollection>(instName);
	}
#endif
	#endregion
}