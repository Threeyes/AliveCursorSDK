using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if USE_UltimateReplayV2
using UltimateReplay;
#endif
/// <summary>
/// 监听Replay事件
/// </summary>
public class ReplayBehaviourEventListener :
#if USE_UltimateReplayV2
    ReplayBehaviour
#else
        MonoBehaviour
#endif
{
    public BoolEvent onReplayPlayStop;

#if USE_UltimateReplayV2
    public override void OnReplayStart()
    {
        onReplayPlayStop.Invoke(true);
    }
    public override void OnReplayEnd()
    {
        onReplayPlayStop.Invoke(false);
    }
#endif
}
