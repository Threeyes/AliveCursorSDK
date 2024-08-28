using Threeyes.Action;
using Threeyes.GameFramework;

public interface IAC_StateController : IModControllerHandler
{
	void SetState(AC_CursorStateInfoEx cursorStateInfo);
	bool IsCurStateActionComplete(ActionState actionState);
}
