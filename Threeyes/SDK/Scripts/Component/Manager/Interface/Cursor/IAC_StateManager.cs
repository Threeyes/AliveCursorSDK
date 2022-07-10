using Threeyes.Action;

public interface IAC_StateManager : IAC_Manager_ModInitHandler, IManagerWithController<IAC_StateController>
{
	/// <summary>
	/// Current StateMachineBehaviour info
	/// </summary>
	AC_CursorStateInfo CurCursorStateInfo { get; }
	AC_CursorState CurCursorState { get; }
	AC_CursorState LastCursorState { get; }

	/// <summary>
	/// Check if target ActionState Completed
	/// </summary>
	/// <param name="actionState"></param>
	/// <returns></returns>
	bool IsCurStateActionComplete(ActionState actionState);
}

public interface IAC_CursorState_ChangedHandler
{
	void OnCursorStateChanged(AC_CursorStateInfo cursorStateInfo);
}