using System;
using UnityEngine;
#if USE_UltimateReplayV2
using UltimateReplay;
#endif

public class ReplayComponentHelperBase<TComp> :
#if USE_UltimateReplayV2
    ReplayBehaviour
#else
        MonoBehaviour
#endif
    where TComp : Component
{
    public TComp Comp
    {
        get
        {
            if (!comp)
                comp = GetComponent<TComp>();
            return comp;
        }
    }
    [SerializeField] protected TComp comp;


#if USE_UltimateReplayV2
    /// <summary>
    /// 录制指定方法，适用于回调
    /// </summary>
    /// <param name="action"></param>
    protected void RecordCallbackMethod(Action action)
    {
        if (IsRecording)
        {
            action.Execute();
        }
    }
#endif
}
