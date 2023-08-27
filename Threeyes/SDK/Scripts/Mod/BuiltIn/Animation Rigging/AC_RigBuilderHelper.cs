using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Threeyes.Coroutine;
using NaughtyAttributes;
using Threeyes.Steamworks;
/// <summary>
/// Make sure joints relation don't break when cursor size changed (Mainly for DampTransform or ChainIK)
/// 
/// How to save joints' Initialization info:
/// 1. Setup joints
/// 2. Open ContextMenu and invoke SaveJointInfo before game played 
/// 
/// PS：更改光标大小/光标显隐后，RigBuilder的joints会出现错位（因为性能优化，更改缩放不会更新Joint： https://forum.unity.com/threads/how-can-i-override-scale-using-animation-rigging.770219/#post-7277947）
/// </summary>
public class AC_RigBuilderHelper : RigBuilderHelper
       , IAC_CommonSetting_CursorSizeHandler
    ,IAC_CursorState_ChangedHandler
{
    #region CallBack
    public void OnCursorSizeChanged(float value)
    {
        RebuildJoint();
    }
    bool isLastHidingState;
    public void OnCursorStateChanged(AC_CursorStateInfo cursorStateInfo)
    {
        ///避免：光标通过更改尺寸重新显示时，Joint会错位的问题
        bool isCurHidingState = AC_ManagerHolder.StateManager.IsVanishState(cursorStateInfo.cursorState);
        if (isCurHidingState)
        {
            TryStopCoroutine();
        }
        else
        {
            if (isLastHidingState)//只有从隐藏切换到显示，才需要更新
                RebuildJoint();
        }
        isLastHidingState = isCurHidingState;
    }
    #endregion
}
