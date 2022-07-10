using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if USE_BezierSolution
using BezierSolution;
#endif
#if USE_NaughtyAttributes
using NaughtyAttributes;
#endif
public class SimpleWaypointGroup : GameObjectBase
{
#if USE_NaughtyAttributes
    [ReorderableList]
#endif
    public List<SimpleWaypoint> listWayPoint = new List<SimpleWaypoint>();

    public bool IsAvaliable
    {
        get
        { return listWayPoint.Count > 1; }
    }

#if USE_NaughtyAttributes
    [Button("RefreshData")]
#endif
    public void RefreshData()
    {
        listWayPoint.Clear();
        tfThis.ForEachChild((tf) =>
        {
            SimpleWaypoint simpleWaypoint = tf.GetComponent<SimpleWaypoint>();
            if (simpleWaypoint)
            {
                simpleWaypoint.name = "WP " + listWayPoint.Count;
                listWayPoint.Add(simpleWaypoint);
            }

        }, false);

        //Consider Close Path
        SimpleWaypoint simpleWaypointLast = tfThis.GetChild(tfThis.childCount - 1).GetComponent<SimpleWaypoint>();
        if (simpleWaypointLast)
        {
            if (simpleWaypointLast.nextWayPoint)
                listWayPoint.Add(simpleWaypointLast.nextWayPoint);
        }
    }

    /// <summary>
    /// 根据总百分比，获取分段的首尾节点
    /// </summary>
    /// <param name="totalPercent"></param>
    /// <returns></returns>
    public WayPointPathInfo GetWayPointPairInfo(float totalPercent)
    {
        totalPercent = Mathf.Clamp(totalPercent, 0, 1);

        //Todo:根据枚举，计算旋转值，有朝向的，也有两个变量插值的
        WayPointPathInfo wayPointPairInfo = new WayPointPathInfo();
        for (int i = 0; i != listWayPoint.Count - 1; i++)
        {
            SimpleWaypoint simpleWaypoint = listWayPoint[i];
            if (totalPercent >= GetWayPointPercent(i) && totalPercent <= GetWayPointPercent(i + 1))
            {
                wayPointPairInfo.lastWayPointIndex = i;
                wayPointPairInfo.lastWayPoint = listWayPoint[i];
                wayPointPairInfo.nextWayPoint = listWayPoint[i + 1];
                //Todo:优化该计算方法
                wayPointPairInfo.percent = (totalPercent - GetWayPointPercent(i)) * (listWayPoint.Count - 1);//计算在此段的百分比
            }
        }
        debugWayPointPairInfo = wayPointPairInfo;
        return wayPointPairInfo;
    }

    #region Bezier Curve (Optional)

    public bool IsUseBezierSpline
    {
        get
        {
#if USE_BezierSolution
            return BezierSpline != null; 
#else
            return false;
#endif
        }
    }
    public Vector3 GetPosition(float totalPercent)
    {
#if USE_BezierSolution
      totalPercent = tweenCurve.Evaluate(totalPercent);
        return BezierSpline.GetPoint(totalPercent);
        //Vector3.Lerp(transform.position, Comp.GetPoint(m_normalizedT), movementLerpModifier * deltaTime);
#else
        return Vector3.zero;
#endif
    }

    public Quaternion GetRotation(float totalPercent)
    {
#if USE_BezierSolution
     totalPercent = tweenCurve.Evaluate(totalPercent);

        Quaternion targetRotation = default(Quaternion);
        if (lookAt == LookAtMode.Forward)
        {
            targetRotation = Quaternion.LookRotation(BezierSpline.GetTangent(totalPercent));
            //transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationLerpModifier * deltaTime);
        }
        else if (lookAt == LookAtMode.SplineExtraData)
        {
            targetRotation = BezierSpline.GetExtraData(m_normalizedT, extraDataLerpAsQuaternionFunction);
            //transform.rotation = Quaternion.Lerp(transform.rotation, Comp.GetExtraData(m_normalizedT, extraDataLerpAsQuaternionFunction), rotationLerpModifier * deltaTime);
        }
        return targetRotation;
#else
        return Quaternion.identity;
#endif
    }

#if USE_BezierSolution
    public BezierSpline BezierSpline
    {
        get
        {
            if (!bezierSpline)
                bezierSpline = GetComponent<BezierSpline>();
            return bezierSpline;
        }
        set
        {
            bezierSpline = value;
        }
    }
    public float NormalizedT { get { return m_normalizedT; } set { m_normalizedT = value; } }

    [Header("Bezier Curve")]
    public BezierSpline bezierSpline;//可空，如果设置为BezierSpline则使用其作为替代方案
    public LookAtMode lookAt = LookAtMode.Forward;
    public AnimationCurve tweenCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);//位移及旋转需要一致的曲线，否则会出现滞后



    [SerializeField]
    [Range(0f, 1f)]
    float m_normalizedT = 0f;
    static readonly ExtraDataLerpFunction extraDataLerpAsQuaternionFunction = InterpolateExtraDataAsQuaternion;
    static BezierPoint.ExtraData InterpolateExtraDataAsQuaternion(BezierPoint.ExtraData data1, BezierPoint.ExtraData data2, float normalizedT)
    {
        return Quaternion.LerpUnclamped(data1, data2, normalizedT);
    }

    [ContextMenu("ConvertToBezierCurve")]
    public void ConvertToBezierCurve()
    {
        foreach (var wp in listWayPoint)
        {
            wp.AddComponentOnce<BezierPoint>();
        }
        var bs = this.AddComponentOnce<BezierSpline>();
        bs.ConstructLinearPath();
    }



#endif
    #endregion

    #region Utility

    /// <summary>
    /// 获取路标点所占的百分比
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    float GetWayPointPercent(int index)
    {
        return (float)index / (listWayPoint.Count - 1);
    }

#endregion

#region Debug

    [ContextMenu("Test")]
    public void Test(float totalPercent)
    {
        WayPointPathInfo wayPointPairInfo = GetWayPointPairInfo(totalPercent);
        Debug.Log(wayPointPairInfo.lastWayPoint + "   " + wayPointPairInfo.nextWayPoint + "  " + wayPointPairInfo.percent);
    }

    public WayPointPathInfo debugWayPointPairInfo;

#endregion

}

/// <summary>
/// Info about path between two wayPoint
/// </summary>
[System.Serializable]
public class WayPointPathInfo
{
    public int lastWayPointIndex;
    public SimpleWaypoint lastWayPoint;
    public SimpleWaypoint nextWayPoint;
    public float percent;

    public float Length { get { return (nextWayPoint.tfThis.position - lastWayPoint.tfThis.position).magnitude; } }//该段的长度
}
