using System.Collections;
using System.Collections.Generic;
using Threeyes.Steamworks;
using UnityEngine;
/// <summary>
/// Control Creeper's state
/// </summary>
public class AC_CreeperModelController : CreeperModelController
    , IAC_CursorState_ChangedHandler
    , IAC_CommonSetting_CursorSizeHandler
{
    #region Callback
    bool isLastHidingState;
    public void OnCursorStateChanged(AC_CursorStateInfo cursorStateInfo)
    {
        //在相关隐藏State时，临时隐藏该物体
        bool isCurHidingState = AC_ManagerHolder.StateManager.IsVanishState(cursorStateInfo.cursorState);
        if (isCurHidingState)
        {
            TryStopCoroutine_Resize();
            gameObject.SetActive(false);
        }
        else
        {
            if (isLastHidingState)//只有从隐藏切换到显示，才需要更新
                Resize();
        }
        isLastHidingState = isCurHidingState;
    }
    public void OnCursorSizeChanged(float value)
    {
        //PS:在ModInit会被调用
        baseScale = value;
        Resize();
    }
    #endregion

}