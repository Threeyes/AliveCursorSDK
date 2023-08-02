using Threeyes.Action;
using Threeyes.Steamworks;

public interface IAC_StateManager :
    IHubManagerModInitHandler,
    IHubManagerWithController<IAC_StateController>
{
    /// <summary>
    /// Current StateMachineBehaviour info
    /// </summary>
    AC_CursorStateInfo CurCursorStateInfo { get; }
    AC_CursorState CurCursorState { get; }
    AC_CursorState LastCursorState { get; }

    /// <summary>
    /// Check if AC is vanished in specify state (Exit、Hide、StandBy)
    /// </summary>
    /// <param name="cursorState"></param>
    /// <returns></returns>
    bool IsVanishState(AC_CursorState cursorState);
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