/// <summary>
/// Raw Input (Mouse and Key)
/// </summary>
public interface IAC_SystemInputManager
{
	AC_Point CurCursorLocation { get; }
	float LastMouseInputEventTime { get; }
	float LastMouseWheelEventTime { get; }
	float LastKeyInputEventTime { get; }
	bool IsLAltKeyPressed { get; }
	bool IsRAltKeyPressed { get; }
	bool IsLControlKeyPressed { get; }
	bool IsRControlKeyPressed { get; }
	bool IsLShiftKeyPressed { get; }
	bool IsRShiftKeyPressed { get; }

	bool IsModifyKeysPressed(AC_ModifierKeys modifierKeys, AC_ModifierKeySide modifierKeySide = AC_ModifierKeySide.Both);
}

public interface IAC_SystemInput_MouseButtonHandler
{
	/// <summary>
	/// Mouse button down/up event
	/// </summary>
	/// <param name="e"></param>
	/// 
	void OnMouseButton(AC_MouseEventExtArgs e);
}

public interface IAC_SystemInput_MouseWheelHandler
{
	/// <summary>
	/// Mouse scroll event
	/// </summary>
	/// <param name="e"></param>
	void OnMouseWheel(AC_MouseEventExtArgs e);
}
public interface IAC_SystemInput_MouseMoveHandler
{
	/// <summary>
	/// Mouse move event (get invoked every time the mouse move)
	/// </summary>
	/// <param name="e"></param>
	void OnMouseMove(AC_MouseEventExtArgs e);
}

public interface IAC_SystemInput_MouseDragHandler
{
	/// <summary>
	/// Mouse drag start/finish event
	/// </summary>
	/// <param name="e"></param>
	/// 
	void OnMouseDrag(AC_MouseEventExtArgs e);
}