using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 通过射线传送到指定目标点（可选择是否挂在RemoteDestinationPoint上）
/// </summary>
[RequireComponent(typeof(RemoteDestinationPoint))]
public class PhysicsRaycastRecevier_DP : PhysicsRaycastRecevier
{
    private void Awake()
    {
        onHoverUnHover.AddListener(Teleport);


        //Teleport on start
        if (RemoteDestinationPoint.isTeleportOnGameStart)
            SetPos(RemoteDestinationPoint);
    }


    public RemoteDestinationPoint RemoteDestinationPoint
    {
        get
        {
            if (!remoteDestinationPoint)
                remoteDestinationPoint = GetComponent<RemoteDestinationPoint>();
            return remoteDestinationPoint;
        }
    }
    RemoteDestinationPoint remoteDestinationPoint;

    public void SetPos()
    {
        SetPos(RemoteDestinationPoint);
    }
    private void Teleport(bool isHover)
    {
        if (isHover)
        {
            SetPos(RemoteDestinationPoint);
        }
    }

    static void SetPos(RemoteDestinationPoint remoteDestinationPoint)
    {
        if (VRPlayerSimulatorHelper.Instance)
        {
            //ToUpdate:直接调用RemoteDestinationPoint的一个方法实现
            //ToUpdate:后期将VRPlayerSimulatorHelper改为与VRTK同级的模拟器，然后使用DP的传送方法，然后尽量不要涉及到VRInterface
            VRPlayerSimulatorHelper.Instance.SetPosition(remoteDestinationPoint.transform.position);
            if (remoteDestinationPoint.isAlignToDP)
                VRPlayerSimulatorHelper.Instance.SetRotation(remoteDestinationPoint.transform.rotation);


            //通知事件更新
            if (remoteDestinationPoint)
                RemoteDestinationPoint.actionTeleportFinished.Execute(remoteDestinationPoint);
        }
        else
        {
            Debug.LogError("找不到 VRPlayerSimulatorHelper 单例！");
        }
    }
}
