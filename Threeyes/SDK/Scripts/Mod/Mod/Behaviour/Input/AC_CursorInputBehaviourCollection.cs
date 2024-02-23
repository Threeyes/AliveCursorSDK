using NaughtyAttributes;
using Threeyes.Action;
using Threeyes.Core;
using Threeyes.Core.Editor;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// Custom Behaviour when any system input event occured
/// 
/// PS：
/// 1.该脚本无法通过管理多个子CursorInputListener的Group代替，因为他们可能在不同地方，而且分配不同的监听值，会增加复杂度
/// </summary>
[AddComponentMenu(AC_EditorDefinition.ComponentMenuPrefix_Root_Mod_Behaviour_Input + "AC_CursorInputBehaviourCollection", 1)]
public class AC_CursorInputBehaviourCollection : AC_BehaviourCollectionBase<AC_SOCursorInputActionCollection>,
	IAC_SystemInput_MouseButtonHandler,
	IAC_SystemInput_MouseWheelHandler,
	IAC_SystemInput_MouseMoveHandler,
	IAC_SystemInput_MouseDragHandler
{
	#region Property & Field
	//Action target for related Input (can be null)
	[Foldout(foldoutName_ActionTarget)] public GameObject actionTargetLeftButton;
	[Foldout(foldoutName_ActionTarget)] public GameObject actionTargetRightButton;
	[Foldout(foldoutName_ActionTarget)] public GameObject actionTargetMiddleButton;
	[Foldout(foldoutName_ActionTarget)] public GameObject actionTargetXButton1;
	[Foldout(foldoutName_ActionTarget)] public GameObject actionTargetXButton2;
	[Foldout(foldoutName_ActionTarget)] public GameObject actionTargetWheelScroll;
	[Foldout(foldoutName_ActionTarget)] public GameObject actionTargetMove;
	[Foldout(foldoutName_ActionTarget)] public GameObject actionTargetDrag;

	//Events
	[Foldout(foldoutName_UnityEvent)] public BoolEvent onLeftButtonDownUp;
	[Foldout(foldoutName_UnityEvent)] public BoolEvent onRightButtonDownUp;
	[Foldout(foldoutName_UnityEvent)] public BoolEvent onMiddleButtonDownUp;
	[Foldout(foldoutName_UnityEvent)] public BoolEvent onXButton1DownUp;
	[Foldout(foldoutName_UnityEvent)] public BoolEvent onXButton2DownUp;
	[Foldout(foldoutName_UnityEvent)] public FloatEvent onWheelScrollDownUp;
	[Foldout(foldoutName_UnityEvent)] public UnityEvent onMove;
	[Foldout(foldoutName_UnityEvent)] public BoolEvent onDragStartFinish;

	public AC_ModifierKeys ModifierKeys { get { return modifierKeys; } set { modifierKeys = value; } }
	[SerializeField] protected AC_ModifierKeys modifierKeys = AC_ModifierKeys.None;
	#endregion

	#region Simulate Input

	public void SimulateLeftButtonDownUp(bool isDown) { SimulateButtonDownUp(AC_MouseButtons.Left, isDown); }
	public void SimulateMiddleButtonDownUp(bool isDown) { SimulateButtonDownUp(AC_MouseButtons.Middle, isDown); }
	public void SimulateRightButtonDownUp(bool isDown) { SimulateButtonDownUp(AC_MouseButtons.Right, isDown); }
	public void SimulateXButton1ButtonDownUp(bool isDown) { SimulateButtonDownUp(AC_MouseButtons.XButton1, isDown); }
	public void SimulateXButton2ButtonDownUp(bool isDown) { SimulateButtonDownUp(AC_MouseButtons.XButton2, isDown); }
	public void SimulateWheelScrollDownUp(bool isDown)
	{
		AC_MouseEventExtArgs e = new AC_MouseEventExtArgs(AC_MouseButtons.None, 0, new AC_Point(0, 0), (isDown ? -1 : 1) * AC_MouseEventArgs.MouseWheelScrollDelta);
		OnMouseWheel(e);
	}
	public void SimulateMove(Vector2 point)
	{
		AC_MouseEventExtArgs e = new AC_MouseEventExtArgs(AC_MouseButtons.None, 0, new AC_Point((int)point.x, (int)point.y), 0);
		OnMouseMove(e);
	}

	/// <summary>
	/// 
	/// Use cases: Simulate input on Bored state
	/// </summary>
	/// <param name="mouseButtons"></param>
	/// <param name="mouseKeyState"></param>
	public void SimulateButtonDownUp(AC_MouseButtons mouseButtons, bool isDown)
	{
		AC_MouseEventExtArgs e = new AC_MouseEventExtArgs(mouseButtons, 0, new AC_Point(0, 0), 0, isMouseButtonDown: isDown == true, isMouseButtonUp: isDown == false);
		OnMouseButton(e);
	}

	#endregion

	#region Callback
	public virtual void OnMouseButton(AC_MouseEventExtArgs e)
	{
		if (!AC_ManagerHolder.SystemInputManager.IsModifyKeysPressed(modifierKeys))
			return;

		switch (e.Button)
		{
			case AC_MouseButtons.Left: InvokeBehaviour(actionTargetLeftButton, e, onLeftButtonDownUp); break;
			case AC_MouseButtons.Right: InvokeBehaviour(actionTargetRightButton, e, onRightButtonDownUp); break;
			case AC_MouseButtons.Middle: InvokeBehaviour(actionTargetMiddleButton, e, onMiddleButtonDownUp); break;
			case AC_MouseButtons.XButton1: InvokeBehaviour(actionTargetXButton1, e, onXButton1DownUp); break;
			case AC_MouseButtons.XButton2: InvokeBehaviour(actionTargetXButton2, e, onXButton2DownUp); break;
		}
	}
	public virtual void OnMouseWheel(AC_MouseEventExtArgs mouseEventArgs)
	{
		if (!AC_ManagerHolder.SystemInputManager.IsModifyKeysPressed(modifierKeys))
			return;

		float param = mouseEventArgs.DeltaScroll;
		if (soActionCollection && actionTargetWheelScroll)
			soActionCollection.soActionWheelScroll?.Enter(param > 0 ? true : false, actionTargetWheelScroll, param);
		onWheelScrollDownUp.Invoke(param);
	}
	public virtual void OnMouseMove(AC_MouseEventExtArgs mouseEventArgs)
	{
		if (!AC_ManagerHolder.SystemInputManager.IsModifyKeysPressed(modifierKeys))
			return;

		if (soActionCollection && actionTargetMove)
			soActionCollection.soActionMove?.Enter(true, actionTargetMove);
		onMove.Invoke();
	}

	public void OnMouseDrag(AC_MouseEventExtArgs e)
	{
		if (!AC_ManagerHolder.SystemInputManager.IsModifyKeysPressed(modifierKeys))
			return;

		if (soActionCollection && actionTargetDrag)
		{
			var soAction = soActionCollection.soActionDrag;
			if (soAction)
			{
				if (e.IsDragStart)
					soAction.Enter(true, actionTargetDrag);
				else if (e.IsDragFinish)
					soAction.Enter(false, actionTargetDrag);
			}
		}
		if (e.IsDragStart)
			onDragStartFinish.Invoke(true);
		else if (e.IsDragFinish)
			onDragStartFinish.Invoke(false);
	}
	#endregion

	#region Virtual
	protected virtual void InvokeBehaviour(GameObject goTarget, AC_MouseEventExtArgs e, BoolEvent boolEvent)
	{
		if (soActionCollection && goTarget)
		{
			var soAction = soActionCollection[AC_InputTool.ConvertToInputType(e.Button)];
			if (soAction)
			{
				if (e.IsMouseButtonDown)
					soAction.Enter(true, goTarget);
				else if (e.IsMouseButtonUp)
					soAction.Enter(false, goTarget);
			}
		}

		//Still invoke event even if the soActionCollection is not valid
		if (e.IsMouseButtonDown)
			boolEvent.Invoke(true);
		else if (e.IsMouseButtonUp)
			boolEvent.Invoke(false);
	}
	#endregion

	#region Editor Method
#if UNITY_EDITOR
	//——MenuItem——
	static string instName = "CIB_Collection ";
	[UnityEditor.MenuItem(AC_EditorDefinition.HierarchyMenuPrefix_Root_Mod_Behaviour_Input + "CursorInputBehaviourCollection", false, 1)]
	public static void CreateInst()
	{
        EditorTool.CreateGameObjectAsChild<AC_CursorInputBehaviourCollection>(instName);
	}
#endif
	#endregion
}
