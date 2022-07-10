using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if USE_UltimateReplayV2
using UltimateReplay;
#endif
[DisallowMultipleComponent]
public class ReplaySequence : ReplayComponentHelperBase<SequenceAbstract>
{
#if USE_UltimateReplayV2
    public override void Awake()
    {
        base.Awake();
        Comp.actRealSet += (value) => RecordCallbackMethod(() => RecordMethodCall(Set, value));
        Comp.actRealReset += (value) => RecordCallbackMethod(() => RecordMethodCall(Reset, value));
        Comp.actRealSetDelta += (value) => RecordCallbackMethod(() => RecordMethodCall(SetDelta, value));
    }

    [ReplayMethod]
    public void Set(int value)
    {
        if (IsReplaying)
        {
            Comp.Set(value);
        }
    }
    [ReplayMethod]
    public void Reset(int value)
    {
        if (IsReplaying)
        {
            Comp.Reset(value);
        }
    }
    [ReplayMethod]
    public void SetDelta(int value)
    {
        if (IsReplaying)
        {
            Comp.SetDelta(value);
        }
    }
#endif
}
