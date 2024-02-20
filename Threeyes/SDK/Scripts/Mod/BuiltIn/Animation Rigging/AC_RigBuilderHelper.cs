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
/// PS：当光标大小更改后（如更改尺寸，或状态更新导致缩放），RigBuilder的joints会出现错位（因为性能优化，更改缩放不会更新Joint： https://forum.unity.com/threads/how-can-i-override-scale-using-animation-rigging.770219/#post-7277947）
/// </summary>
public class AC_RigBuilderHelper : RigBuilderHelper
       , IAC_CommonSetting_CursorSizeHandler
    , IAC_CursorState_ChangedHandler
{
    #region CallBack
    public void OnCursorSizeChanged(float value)
    {
        RebuildJoint();
    }
    public void OnCursorStateChanged(AC_CursorStateInfo cursorStateInfo)
    {
        bool shouldRebuild = false;
        //从任意状态进入Working/Bored，都需要重建Joint
        if (cursorStateInfo.stateChange == AC_CursorStateInfo.StateChange.Enter)
        {
            switch (cursorStateInfo.cursorState)
            {
                case AC_CursorState.Working:
                case AC_CursorState.Bored:
                    shouldRebuild = true;
                    break;
            }
        }

        if (shouldRebuild)
        {
            cacheEnum_RebuildJointOnStateCompleted = CoroutineManager.StartCoroutineEx(IERebuildJointOnStateCompleted());
        }
        else
        {
            TryStopCoroutine_RebuildJointOnStateCompleted();
        }
    }

    protected UnityEngine.Coroutine cacheEnum_RebuildJointOnStateCompleted;
    IEnumerator IERebuildJointOnStateCompleted()
    {
        //等待当前状态完成后才调用RebuildJoint（因为状态进入时可能涉及缩放）
        while (!AC_ManagerHolder.StateManager.IsCurStateActionComplete(Threeyes.Action.ActionState.Enter))
            yield return null;

        RebuildJoint();
    }
    protected virtual void TryStopCoroutine_RebuildJointOnStateCompleted()
    {
        if (cacheEnum_RebuildJointOnStateCompleted != null)
        {
            CoroutineManager.StopCoroutineEx(cacheEnum_RebuildJointOnStateCompleted);
            cacheEnum_RebuildJointOnStateCompleted = null;
        }
    }

    #endregion
}
