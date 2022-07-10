using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 自动朝向下一路标
/// </summary>
public class NavagationPointerManager : RemoteDestinationPointFollower
{
    protected override void LateUpdate()
    {
        base.LateUpdate();

        //设置自身的显隐状态
        var curWayPoint = WayPoint.CurrentWayPoint;
        if (curWayPoint)
        {
            var nextWayPoint = curWayPoint.FirstNextWayPoint;
            if(nextWayPoint)
            {
                Show(nextWayPoint.IsShowing);//与下一DP同时显隐

                //朝向目标
                tfThis.LookAt(nextWayPoint.transform);
            }
            else
            {
                Show(false);
            }
        }
    }


    public virtual void Show(bool isShow)
    {
        tfThis.gameObject.SetActive(isShow);
    }
}
