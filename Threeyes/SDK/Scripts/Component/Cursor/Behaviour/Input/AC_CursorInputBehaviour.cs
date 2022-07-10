using System.Text;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using Threeyes.Editor;
#endif
/// <summary>
/// Custom Behaviour when target system input event occured
/// 
/// Use case: Different Input that sharing the same SOAction
/// </summary>
[AddComponentMenu(AC_EditorDefinition.ComponentMenuPrefix_AC_Cursor_Behaviour_Input + "AC_CursorInputBehaviour", 0)]
public class AC_CursorInputBehaviour : AC_BehaviourBase,
	IAC_SystemInput_MouseButtonHandler,
	IAC_SystemInput_MouseWheelHandler,
	IAC_SystemInput_MouseMoveHandler,
	IAC_SystemInput_MouseDragHandler
{
	#region Property & Field

	public AC_ModifierKeys ModifierKeys { get { return modifierKeys; } set { modifierKeys = value; } }
	[SerializeField] protected AC_ModifierKeys modifierKeys = AC_ModifierKeys.None;
	public AC_CursorInputType CursorInputType { get { return cursorInputType; } set { cursorInputType = value; } }//Desire type to listen
	[SerializeField] protected AC_CursorInputType cursorInputType = AC_CursorInputType.None;

	///PS:
	///1.虽然通过onPlay/onStop可以获知对应事件回调，但不够直观，因此提供特定事件并隐藏默认事件
	///2.只提供通用的事件，如果需要分别对不同按键处理，请创建对应的CIB实例而不是在同一个CIB中完成所有操作
	public BoolEvent onButtonDownUp;//Triggered when target button get pressed or released
	public FloatEvent onWheelScrollDownUp;//Triggered when wheel get scrolled (>0 is Up, <0 is Down)
	public UnityEvent onMove;//Triggered when the mouse is moved
	public BoolEvent onDragStartFinish;//Triggered when the mouse drag start/finish
									   //ToAdd: onMouseMoveStartStop

	#endregion

	#region Callback

	//PS: Use these fields to make sure the desire UnityEvent get called
	bool isWheelScroll = false;
	bool isButtonDownUp = false;
	bool isMove = false;
	bool isDrag = false;

	public virtual void OnMouseButton(AC_MouseEventExtArgs e)
	{
		if (!AC_ManagerHolder.SystemInputManager.IsModifyKeysPressed(modifierKeys))
			return;
		if (!CursorInputType.Has(AC_InputTool.ConvertToInputType(e.Button), true))
			return;

		isButtonDownUp = true;
		if (e.IsMouseButtonDown)
			Play();
		else if (e.IsMouseButtonUp)
			Stop();
		isButtonDownUp = false;
	}

	public virtual void OnMouseWheel(AC_MouseEventExtArgs mouseEventArgs)
	{
		if (!AC_ManagerHolder.SystemInputManager.IsModifyKeysPressed(modifierKeys))
			return;
		if (!CursorInputType.Has(AC_CursorInputType.WheelScroll))
			return;

		isWheelScroll = true;
		float param = mouseEventArgs.DeltaScroll;
		PlayWithParam(param > 0 ? true : false, param);
		isWheelScroll = false;
	}

	public virtual void OnMouseMove(AC_MouseEventExtArgs mouseEventArgs)
	{
		if (!AC_ManagerHolder.SystemInputManager.IsModifyKeysPressed(modifierKeys))
			return;
		if (!CursorInputType.Has(AC_CursorInputType.Move))
			return;
		isMove = true;
		Play();
		isMove = false;
	}

	public virtual void OnMouseDrag(AC_MouseEventExtArgs e)
	{
		if (!AC_ManagerHolder.SystemInputManager.IsModifyKeysPressed(modifierKeys))
			return;
		if (!CursorInputType.Has(AC_CursorInputType.Drag))
			return;
		isDrag = true;
		if (e.IsDragStart)
			Play();
		else if (e.IsDragFinish)
			Stop();
		isDrag = false;
	}

	#endregion

	#region Inner Method

	protected override void PlayFunc()
	{
		base.PlayFunc();
		if (isButtonDownUp)
			onButtonDownUp.Invoke(true);
		else if (isMove)
			onMove.Invoke();
		else if (isDrag)
			onDragStartFinish.Invoke(true);
	}
	protected override void StopFunc()
	{
		base.StopFunc();
		if (isButtonDownUp)
			onButtonDownUp.Invoke(false);
		else if (isDrag)
			onDragStartFinish.Invoke(false);
	}

	protected override void PlayWithParamFunc(object value)
	{
		base.PlayWithParamFunc(value);
		if (isWheelScroll)
			onWheelScrollDownUp.Invoke((float)value);//确保事件在合适的方法体被调用
	}
	protected override void StopWithParamFunc(object value)
	{
		base.StopWithParamFunc(value);
		if (isWheelScroll)
			onWheelScrollDownUp.Invoke((float)value);
	}

	#endregion

	#region Utility

	static bool IsButtonInput(AC_CursorInputType input)
	{
		return (input & (AC_CursorInputType.LeftButtonDownUp | AC_CursorInputType.RightButtonDownUp | AC_CursorInputType.MiddleButtonDownUp | AC_CursorInputType.XButton1DownUp | AC_CursorInputType.XButton2DownUp)) != 0;
	}

	#endregion

	#region Editor Method
#if UNITY_EDITOR

	//——MenuItem——

	static string instName = "CIB ";
	[UnityEditor.MenuItem(AC_EditorDefinition.HierarchyMenuPrefix_Cursor_Behaviour_Input + "CursorInputBehaviour", false, 0)]
	public static void CreateActionEventPlayer_CursorInputBehaviour()
	{
		EditorTool.CreateGameObjectAsChild<AC_CursorInputBehaviour>(instName);
	}

	//——Hierarchy GUI——
	public override string ShortTypeName { get { return "CIB"; } }


	public override void SetHierarchyGUIProperty(StringBuilder sB)
	{
		base.SetHierarchyGUIProperty(sB);
		sbCache.Length = 0;
		if (ModifierKeys == AC_ModifierKeys.All)
		{
			sbCache.Append("All");
		}
		else if (ModifierKeys != AC_ModifierKeys.None)
		{
			sbCache.Append(ModifierKeys);
		}
		AddSplit(sB, sbCache);

		sbCache.Length = 0;
		if (CursorInputType == AC_CursorInputType.All)
		{
			sbCache.Append("All");
		}
		else if (CursorInputType != AC_CursorInputType.None)//显示缩写
		{
			listHierarchyParamCache.Clear();
			if (CursorInputType.HasFlag(AC_CursorInputType.LeftButtonDownUp))
				listHierarchyParamCache.Add("L");
			if (CursorInputType.HasFlag(AC_CursorInputType.RightButtonDownUp))
				listHierarchyParamCache.Add("R");
			if (CursorInputType.HasFlag(AC_CursorInputType.MiddleButtonDownUp))
				listHierarchyParamCache.Add("M");
			if (CursorInputType.HasFlag(AC_CursorInputType.XButton1DownUp))
				listHierarchyParamCache.Add("X1");
			if (CursorInputType.HasFlag(AC_CursorInputType.XButton2DownUp))
				listHierarchyParamCache.Add("X2");
			if (CursorInputType.HasFlag(AC_CursorInputType.WheelScroll))
				listHierarchyParamCache.Add("Scroll");
			if (CursorInputType.HasFlag(AC_CursorInputType.Move))
				listHierarchyParamCache.Add("Move");
			if (CursorInputType.HasFlag(AC_CursorInputType.Drag))
				listHierarchyParamCache.Add("Drag");

			if (listHierarchyParamCache.Count > 0)
				sbCache.Append(listHierarchyParamCache.ConnectToString(", "));
		}
		AddSplit(sB, sbCache);
	}

	//——Inspector GUI——
	public override void SetInspectorGUIUnityEventProperty(GUIPropertyGroup group)
	{
		group.title = "Unity Event";
		group.listProperty.Add(new GUIProperty(nameof(onActiveDeactive)));

		group.listProperty.Add(new GUIProperty(nameof(onButtonDownUp), isEnable: IsButtonInput(cursorInputType)));
		group.listProperty.Add(new GUIProperty(nameof(onWheelScrollDownUp), isEnable: cursorInputType.Has(AC_CursorInputType.WheelScroll)));
		group.listProperty.Add(new GUIProperty(nameof(onMove), isEnable: cursorInputType.Has(AC_CursorInputType.Move)));
		group.listProperty.Add(new GUIProperty(nameof(onDragStartFinish), isEnable: cursorInputType.Has(AC_CursorInputType.Drag)));

	}
	public override void SetInspectorGUISubProperty(GUIPropertyGroup group)
	{
		base.SetInspectorGUISubProperty(group);
		group.listProperty.Add(new GUIProperty(nameof(modifierKeys)));
		group.listProperty.Add(new GUIProperty(nameof(cursorInputType)));
	}

#endif
	#endregion
}
