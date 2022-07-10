using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if USE_UltimateReplayV2
using UltimateReplay;
#endif

[DisallowMultipleComponent]
public class ReplayRemoteDestinationPointManager : ReplayComponentHelperBase<RemoteDestinationPointManager>
{
#if USE_UltimateReplayV2
    public override void Awake()
    {
        base.Awake();
        Comp.actRealSetPos += OnSetPos;
    }

    //ToUpdate：通过SceneID获取对应的DP
    public void OnSetPos(RemoteDestinationPoint rdp, Vector3    ? value, bool isInvokeEvent)
    {
        if (IsRecording)
        {
            string sceneID = SceneUniqueTool.GetSceneID(rdp);
            RecordMethodCall(SetPos, sceneID, value.HasValue ? value.Value : Vector3.negativeInfinity, isInvokeEvent);
        }
    }


    [ReplayMethod]
    public void SetPos(string sceneID, Vector3 value, bool isInvokeEvent)//PS：ReplayMethod不支持可空方法
    {
        if (IsReplaying)
        {
            var rdp = SceneUniqueTool.FindBySceneID<RemoteDestinationPoint>(sceneID, Comp.tfRoot);
            if (rdp)
            {
                Vector3? realValue = null;
                if (value != Vector3.negativeInfinity)
                    realValue = value;
                rdp.SetPos(realValue, isInvokeEvent);
            }
        }
    }
#endif
}
