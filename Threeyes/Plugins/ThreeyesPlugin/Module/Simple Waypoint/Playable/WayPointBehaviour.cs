#if UNITY_2020_1_OR_NEWER
#define NewVersion
#else//旧版本直接激活
#define Active
#endif
#if NewVersion && USE_Timeline
#define Active
#endif
#if Active
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class WayPointBehaviour : BehaviourBase<Transform>
{
    public SimpleWaypointGroup wayPointGroup;
    [Tooltip("True：Use Curve in everyLocalPath")]
    public bool isUseLocalCurve = true;
    public bool isLookAt;
    public float startRotPercent = 0.2f;
    public float rotateSpeed;

    //Todo：改为整体的曲线
    public AnimationCurve posTweenCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public AnimationCurve rotTweenCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        float totalPercent = (float)(playable.GetTime() / playable.GetDuration());
        Calculate(totalPercent);
    }

    /// <summary>
    /// 计算指定百分比时的位置
    /// </summary>
    /// <param name="totalPercent"></param>
    public void Calculate(float totalPercent)
    {
        if (!trackBinding)
            return;

        if (!wayPointGroup)
            return;

        if (wayPointGroup.IsUseBezierSpline)//BezierSpline
        {
            trackBinding.position = wayPointGroup.GetPosition(totalPercent);
            trackBinding.rotation = wayPointGroup.GetRotation(totalPercent);
            return;
        }
        else if (wayPointGroup && wayPointGroup.IsAvaliable)//SimpleWayPoint
        {
            if (!isUseLocalCurve)
            {
                totalPercent = posTweenCurve.Evaluate(totalPercent);//ReCalculate the totalPercent, Because the curve represent full path
            }
            WayPointPathInfo pathInfo = wayPointGroup.GetWayPointPairInfo(totalPercent);
            Transform originTf = pathInfo.lastWayPoint.tfThis;

            float curPathPercent = pathInfo.percent;

            float posPercent = 0;
            //ReCalculate 
            if (isUseLocalCurve)
            {
                posPercent = pathInfo.lastWayPoint.animCurvePos.Evaluate(curPathPercent);
            }
            else
            {
                posPercent = posTweenCurve.Evaluate(curPathPercent);
            }

            //  Slerp is best for directions, Lerp is best for positions.You should probably be using Lerp. (https://stackoverflow.com/questions/19821514/using-vector3-slerp )
            trackBinding.position = Vector3.LerpUnclamped(originTf.position, pathInfo.nextWayPoint.tfThis.position, posPercent);//模锟斤拷锟斤拷锟阶拷锟绞憋拷募蛹锟斤拷锟?


            if (isLookAt)
            {
                float leftDistance = (pathInfo.nextWayPoint.tfThis.position - trackBinding.position).magnitude;//剩余的距离
                float startRotLength = pathInfo.Length * startRotPercent;
                //curPathPercent应该变为开始旋转的百分比
                if (leftDistance < startRotLength)//锟脚筹拷target锟接斤拷目锟斤拷愕硷拷锟斤拷锟阶拷锟斤拷锟?
                {
                    float curRotPathPercent = 1 - leftDistance / startRotLength;//开始进行旋转的路程
                    float rotPercent = 0;
                    if (isUseLocalCurve)
                    {
                        rotPercent = pathInfo.lastWayPoint.animCurveRot.Evaluate(curRotPathPercent);
                    }
                    else
                    {
                        rotPercent = rotTweenCurve.Evaluate(curRotPathPercent);
                    }

                    //Change Rotation while target nextWayPoint 

                    //到达旋转的位置
                    SimpleWaypoint wayPointToLook = pathInfo.nextWayPoint;
                    if (pathInfo.nextWayPoint.nextWayPoint)
                        wayPointToLook = pathInfo.nextWayPoint.nextWayPoint;

                    Vector3 dirToLook = wayPointToLook.tfThis.position - trackBinding.position;
                    trackBinding.rotation = Quaternion.Lerp(trackBinding.rotation, Quaternion.LookRotation(dirToLook), rotPercent * rotateSpeed);
                }
            }
        }
    }
}
#endif