using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 监听DP的全局事件
/// </summary>
public class RemoteDestinationPointListener : ComponentHelperBase<RemoteDestinationPoint>
{
    public UnityEvent onTeleportFinished;
    public BoolEvent onEnterExit;//是否进入此DP

    private void OnEnable()
    {
        RemoteDestinationPoint.actionTeleportFinished += OnTeleportFinished;
    }
    private void OnDisable()
    {
        RemoteDestinationPoint.actionTeleportFinished -= OnTeleportFinished;
    }

    protected virtual void OnTeleportFinished(RemoteDestinationPoint remoteDestinationPoint)
    {
        if (remoteDestinationPoint)
        {
            onTeleportFinished.Invoke();
        }

#if UNITY_EDITOR
        if(remoteDestinationPoint == Comp)
        {
            Debug.Log("Enter DP: " + remoteDestinationPoint.name);
        }
#endif

        onEnterExit.Invoke(remoteDestinationPoint == Comp);
    }
}
