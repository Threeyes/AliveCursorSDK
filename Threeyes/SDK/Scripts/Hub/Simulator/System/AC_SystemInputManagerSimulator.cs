using System;
using System.Collections;
using System.Collections.Generic;
using Threeyes.Core;
using Threeyes.InputSystem;
using UnityEngine;

public class AC_SystemInputManagerSimulator : AC_SystemInputManagerBase<AC_SystemInputManagerSimulator>
{
	Vector3 lastMousePosition;

	//Drag
	Vector3 lastMouseButtonDownPosition;
	bool isDraggingBegin = false;
	private void Update()
	{
		//Simulate Input
		//PS:可改为Vector3Int
		int curTimestamp = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;//注意：该测试数据可能跟系统的不一致！
		Vector3 curMousePosition = InputTool.mousePosition;


		//MouseDown/Up
		if (InputTool.GetMouseButtonDown(0))
		{
			OnMouseDownUp(new AC_MouseEventExtArgs(AC_MouseButtons.Left, 1, curCursorLocation, 0, isMouseButtonDown: true, timestamp: curTimestamp));
			lastMouseButtonDownPosition = curMousePosition;
		}
		if (InputTool.GetMouseButtonDown(1))
			OnMouseDownUp(new AC_MouseEventExtArgs(AC_MouseButtons.Right, 1, curCursorLocation, 0, isMouseButtonDown: true, timestamp: curTimestamp));
		if (InputTool.GetMouseButtonDown(2))
			OnMouseDownUp(new AC_MouseEventExtArgs(AC_MouseButtons.Middle, 1, curCursorLocation, 0, isMouseButtonDown: true, timestamp: curTimestamp));
		if (InputTool.GetMouseButtonDown(3))
			OnMouseDownUp(new AC_MouseEventExtArgs(AC_MouseButtons.XButton1, 1, curCursorLocation, 0, isMouseButtonDown: true, timestamp: curTimestamp));
		if (InputTool.GetMouseButtonDown(4))
			OnMouseDownUp(new AC_MouseEventExtArgs(AC_MouseButtons.XButton2, 1, curCursorLocation, 0, isMouseButtonDown: true, timestamp: curTimestamp));

		if (InputTool.GetMouseButtonUp(0))
			OnMouseDownUp(new AC_MouseEventExtArgs(AC_MouseButtons.Left, 1, curCursorLocation, 0, isMouseButtonUp: true, timestamp: curTimestamp));
		if (InputTool.GetMouseButtonUp(1))
			OnMouseDownUp(new AC_MouseEventExtArgs(AC_MouseButtons.Right, 1, curCursorLocation, 0, isMouseButtonUp: true, timestamp: curTimestamp));
		if (InputTool.GetMouseButtonUp(2))
			OnMouseDownUp(new AC_MouseEventExtArgs(AC_MouseButtons.Middle, 1, curCursorLocation, 0, isMouseButtonUp: true, timestamp: curTimestamp));
		if (InputTool.GetMouseButtonUp(3))
			OnMouseDownUp(new AC_MouseEventExtArgs(AC_MouseButtons.XButton1, 1, curCursorLocation, 0, isMouseButtonUp: true, timestamp: curTimestamp));
		if (InputTool.GetMouseButtonUp(4))
			OnMouseDownUp(new AC_MouseEventExtArgs(AC_MouseButtons.XButton2, 1, curCursorLocation, 0, isMouseButtonUp: true, timestamp: curTimestamp));

		if (InputTool.mouseScrollDelta != Vector2.zero)
		{
			OnMouseWheel(new AC_MouseEventExtArgs(AC_MouseButtons.None, 0, curCursorLocation, (int)(InputTool.mouseScrollDelta.y * 120), timestamp: curTimestamp));
		}

		//MouseMove
		if (lastMousePosition != curMousePosition)
		{
			OnMouseMove(new AC_MouseEventExtArgs(AC_MouseButtons.None, 0, curCursorLocation, 0, timestamp: curTimestamp));
			lastMousePosition = curMousePosition;
		}


		//ToAdd:Mouse Drag
		if (InputTool.GetMouseButton(0))
		{
			if (!isDraggingBegin && (curMousePosition != lastMouseButtonDownPosition))
			{
				OnMouseDragStartFinish(new AC_MouseEventExtArgs(AC_MouseButtons.Left, 1, curCursorLocation, 0, isMouseButtonDown: true, timestamp: curTimestamp), true);
				isDraggingBegin = true;
			}
		}
		else if (InputTool.GetMouseButtonUp(0))
		{
			if (curMousePosition != lastMouseButtonDownPosition)
			{
				OnMouseDragStartFinish(new AC_MouseEventExtArgs(AC_MouseButtons.Left, 1, curCursorLocation, 0, isMouseButtonDown: true, timestamp: curTimestamp), false);
				isDraggingBegin = false;
			}
		}
	}

	//PS：Event只在OnGUI可用
	Event currentEvent { get { return Event.current; } }
	KeyCode curKeyCode;
	EventType curEventType;
	private void OnGUI()
	{
		curKeyCode = currentEvent.keyCode;
		if (curKeyCode == KeyCode.None)
			return;

		curEventType = currentEvent.rawType;
		if (curEventType == EventType.KeyDown || currentEvent.rawType == EventType.KeyUp)
		{
			OnKeyDownUp(curKeyCode, curEventType == EventType.KeyDown ? AC_KeyState.Down : AC_KeyState.Up);
		}
	}
}