using UnityEngine;

public class AC_SystemInputManagerBase<T> : AC_ManagerBase<T>, IAC_SystemInputManager
	where T : AC_SystemInputManagerBase<T>
{
	#region Interface

	public AC_Point CurCursorLocation { get { return curCursorLocation; } }// 鼠标的Raw屏幕坐标（基于主屏，结果可能为负数）
	public float LastMouseInputEventTime { get { return lastMouseInputEventTime; } set { lastMouseInputEventTime = value; } }// 鼠标移动或点击事件
	public float LastMouseWheelEventTime { get { return lastMouseWheelEventTime; } set { lastMouseWheelEventTime = value; } }// 鼠标滚轮事件
	public float LastKeyInputEventTime { get { return lastKeyInputEventTime; } set { lastKeyInputEventTime = value; } }// 上次点击按键的事件
	public bool IsLAltKeyPressed { get { return isLAltKeyPressed; } }
	public bool IsRAltKeyPressed { get { return isRAltKeyPressed; } }
	public bool IsLControlKeyPressed { get { return isLControlKeyPressed; } }
	public bool IsRControlKeyPressed { get { return isRControlKeyPressed; } }
	public bool IsLShiftKeyPressed { get { return isLShiftKeyPressed; } }
	public bool IsRShiftKeyPressed { get { return isRShiftKeyPressed; } }


	public bool IsModifyKeysPressed(AC_ModifierKeys modifierKeys, AC_ModifierKeySide modifierKeySide = AC_ModifierKeySide.Both)//ModifierKeys,支持多按键组合
	{
		if (modifierKeys == AC_ModifierKeys.None)//PS:None代表不需要按下任何ModifierKey，返回true以便后续执行之后的代码
			return true;

		//因为可能是组合键，因此只要任意条件不满足则返回false
		if (modifierKeys.HasFlag(AC_ModifierKeys.Alt) && !IsModifyKeyPressed(modifierKeySide, isLAltKeyPressed, isRAltKeyPressed))
		{
			return false;
		}
		if (modifierKeys.HasFlag(AC_ModifierKeys.Control) && !IsModifyKeyPressed(modifierKeySide, isLControlKeyPressed, isRControlKeyPressed))
			return false;
		if (modifierKeys.HasFlag(AC_ModifierKeys.Shift) && !IsModifyKeyPressed(modifierKeySide, isLShiftKeyPressed, isRShiftKeyPressed))
			return false;
		return true;
	}
	bool IsModifyKeyPressed(AC_ModifierKeySide modifierKeySide, bool isLeftKeyPressed, bool isRightKeyPressed)
	{
		switch (modifierKeySide)
		{
			case AC_ModifierKeySide.Both:
				return isLeftKeyPressed || isRightKeyPressed;
			case AC_ModifierKeySide.Left:
				return isLeftKeyPressed;
			case AC_ModifierKeySide.Right:
				return isRightKeyPressed;
			default:
				return false;
		}
	}
	#endregion

	#region Property & Field
	[SerializeField] protected AC_Point curCursorLocation;
	[SerializeField] AC_Point lastMouseLocation = default;
	[SerializeField] protected float lastMouseInputEventTime = 0;//没有获得鼠标事件的时长
	[SerializeField] protected float lastMouseWheelEventTime = 0;//没有获得鼠标事件的时长
	[SerializeField] protected float lastKeyInputEventTime = 0;//没有获得鼠标事件的时长
	[SerializeField] protected bool isLAltKeyPressed = false;
	[SerializeField] protected bool isRAltKeyPressed = false;
	[SerializeField] protected bool isLControlKeyPressed = false;
	[SerializeField] protected bool isRControlKeyPressed = false;
	[SerializeField] protected bool isLShiftKeyPressed = false;
	[SerializeField] protected bool isRShiftKeyPressed = false;
	#endregion

	#region Inner Method
	//Mouse
	protected virtual void OnMouseMove(AC_MouseEventExtArgs e)
	{
		UpdateMouseInputInfo();

		curCursorLocation = e.Location;
		//计算DeltaLocation
		//（PS：Location是基于主屏幕的左上角，单位是像素，因此如果有多屏幕，其坐标基准不变，位于主屏幕左上侧的坐标为负数）
		if (lastMouseLocation != default(AC_Point))//Init
		{
			e.DeltaLocation = e.Location - lastMouseLocation;
		}
		lastMouseLocation = e.Location;
		AC_EventCommunication.SendMessage<IAC_SystemInput_MouseMoveHandler>(inst => inst.OnMouseMove(e));
	}

	protected virtual void OnMouseDownUp(AC_MouseEventExtArgs e)
	{
		UpdateMouseInputInfo();
		AC_EventCommunication.SendMessage<IAC_SystemInput_MouseButtonHandler>(inst => inst.OnMouseButton(e));
	}
	protected virtual void OnMouseDragStartFinish(AC_MouseEventExtArgs e, bool isStart)
	{
		//PS：IsDragStart/IsDragFinish是自行加上，所以需要在这里赋值
		if (isStart)
			e.IsDragStart = true;
		else
			e.IsDragFinish = true;
		AC_EventCommunication.SendMessage<IAC_SystemInput_MouseDragHandler>(inst => inst.OnMouseDrag(e));
	}
	protected virtual void OnMouseWheel(AC_MouseEventExtArgs e)
	{
		UpdateMouseWheelInfo();
		AC_EventCommunication.SendMessage<IAC_SystemInput_MouseWheelHandler>(inst => inst.OnMouseWheel(e));
	}

	/// <summary>
	/// 更新鼠标输入的时间
	/// </summary>
	protected virtual void UpdateMouseInputInfo()
	{
		lastMouseInputEventTime = Time.time;
	}
	protected virtual void UpdateMouseWheelInfo()
	{
		lastMouseWheelEventTime = Time.time;
	}

	//Key
	protected virtual void OnKeyDownUp(KeyCode keyCode, AC_KeyState keyState)
	{
		UpdateKeyInputInfo(keyCode, keyState);
		//Debug.LogError(keyCode + " " + keyState);
	}

	void UpdateKeyInputInfo(KeyCode keyCode, AC_KeyState keyState)
	{
		lastKeyInputEventTime = Time.time;

		UpdateModifyKeysInputInfo(keyCode, keyState, KeyCode.LeftAlt, ref isLAltKeyPressed);
		UpdateModifyKeysInputInfo(keyCode, keyState, KeyCode.RightAlt, ref isRAltKeyPressed);
		UpdateModifyKeysInputInfo(keyCode, keyState, KeyCode.LeftControl, ref isLControlKeyPressed);
		UpdateModifyKeysInputInfo(keyCode, keyState, KeyCode.RightControl, ref isRControlKeyPressed);
		UpdateModifyKeysInputInfo(keyCode, keyState, KeyCode.LeftShift, ref isLShiftKeyPressed);
		UpdateModifyKeysInputInfo(keyCode, keyState, KeyCode.RightShift, ref isRShiftKeyPressed);
	}
	void UpdateModifyKeysInputInfo(KeyCode keyCode, AC_KeyState keyState, KeyCode keyCodeRequire, ref bool cacheKeyState)
	{
		if (keyState == AC_KeyState.Down || keyState == AC_KeyState.Up)
			if (keyCode == keyCodeRequire)
				cacheKeyState = keyState == AC_KeyState.Down ? true : false;
	}
	#endregion
}
