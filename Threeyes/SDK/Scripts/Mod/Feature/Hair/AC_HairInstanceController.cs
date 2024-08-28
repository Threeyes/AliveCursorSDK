#if USE_UnityHair
using Threeyes.GameFramework;

public class AC_HairInstanceController : HairInstanceController
    , IAC_CursorState_ChangedHandler
{
    #region Callback
    bool isLastHidingState;
    public void OnCursorStateChanged(AC_CursorStateInfo cursorStateInfo)
    {
        bool isCurHidingState = AC_ManagerHolder.StateManager.IsVanishState(cursorStateInfo.cursorState);
        if (isLastHidingState && !isCurHidingState)
            ReBuild();
        isLastHidingState = isCurHidingState;
    }
    #endregion
}
#endif