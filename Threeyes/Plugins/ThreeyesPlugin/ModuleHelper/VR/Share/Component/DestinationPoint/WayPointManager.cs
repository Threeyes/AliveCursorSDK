using System.Collections;
using System.Collections.Generic;
using Threeyes.Coroutine;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 管理场景的所有WayPoint
/// 注意：
/// DP要不同名
/// </summary>
public class WayPointManager : InstanceBase<WayPointManager>
{
    public static WayPoint CurrentWayPoint { get { return WayPoint.CurrentWayPoint; } }//上一个传送的路标点

    public void MovetoNextWayPoint()
    {
        if (!CurrentWayPoint)
            return;

        CurrentWayPoint.MovetoNextWayPoint();
    }

    [ContextMenu("TryMovetoNextWayPoint")]
    public void TryMovetoNextWayPoint()
    {
        if (!CurrentWayPoint)
            return;
        CurrentWayPoint.TryMovetoNextWayPoint();
    }

}
