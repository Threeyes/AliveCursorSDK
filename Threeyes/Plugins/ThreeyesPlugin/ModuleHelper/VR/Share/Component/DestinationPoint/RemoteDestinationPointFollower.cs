using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 一直跟随玩家所在路标的位置
/// Todo:Face HeadSet
/// </summary>
public class RemoteDestinationPointFollower : AutoFollow
{
    private void Reset()
    {
        isBaseOnTarget = true;
    }

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
            SetTarget(remoteDestinationPoint.transform);
    }
}
