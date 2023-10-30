using Threeyes.Action;
using Threeyes.Steamworks;

public interface IAC_StateController : IModControllerHandler
{
	void SetState(AC_CursorStateInfoEx cursorStateInfo);
	bool IsCurStateActionComplete(ActionState actionState);
}
