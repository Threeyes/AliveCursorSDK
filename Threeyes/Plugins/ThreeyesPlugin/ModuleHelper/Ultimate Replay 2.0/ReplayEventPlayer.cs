using System;
using Threeyes.EventPlayer;
using UnityEngine;
#if USE_UltimateReplayV2
using UltimateReplay;
#endif

//注意：
//1. [ReplayMethod] 需要其父类为ReplayBehaviour，而且是public（因为是通过反射获取），具体看ReplayObject.CheckMethodRecordable #485
//2.RecordMethodCall的作用是录制并调用，所以针对封装的回调方法，在录制时不要调用，而是在回放时调用
[DisallowMultipleComponent]
public class ReplayEventPlayer : ReplayComponentHelperBase<EventPlayer>
{
#if USE_UltimateReplayV2
 public override void Awake()
    {
        base.Awake();
        Comp.actionRealPlay += (value) => RecordCallbackMethod(() => RecordMethodCall(Play, value));
        Comp.actionRealSetIsActive += (value) => RecordCallbackMethod(() => RecordMethodCall(SetIsActive, value));
    }

    [ReplayMethod]
    public void Play(bool value)
    {
        if (IsReplaying)
        {
            Comp.Play(value);
        }
    }

    [ReplayMethod]
    public void SetIsActive(bool value)
    {
        if (IsReplaying)
        {
            Comp.IsActive = value;
        }
    }
#endif
}