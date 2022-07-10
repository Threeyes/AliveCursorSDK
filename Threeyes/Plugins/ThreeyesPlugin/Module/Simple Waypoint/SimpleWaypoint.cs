using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif
/// <summary>
/// 简单的路标
/// </summary>
public class SimpleWaypoint : GameObjectBase
{
    public static string defaultName = "WayPoint";
    public SimpleWaypoint nextWayPoint;
    public bool isImmediatelyReach = false;//是否立即到达该路标点，常用于起始点
    public UnityEvent onReach;//到达路标后的事件，可用于触发特定事情，或者是更改Follower的速度

    public AnimationCurve animCurvePos = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    public AnimationCurve animCurveRot = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    public Transform GetNextTarget()
    {
        return nextWayPoint ? nextWayPoint.tfThis : null;
    }

#if UNITY_EDITOR

    SimpleWaypointGroup simpleWaypointGroup { get { return transform.FindFirstComponentInParent<SimpleWaypointGroup>(); } }


    private void OnValidate()
    {
        //通知组更新
        if (simpleWaypointGroup)
            simpleWaypointGroup.RefreshData();
    }


    static Vector3 deltaGizmo = new Vector3(0, 0.1f, 0);
    [Header("Editor Setting")]
    public float spehreSize = 0.1f;
    [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NonSelected | GizmoType.Pickable)]
    static void MyGizmo(SimpleWaypoint wayPoint, GizmoType gizmoType)  //参数1为“XX”组件，可以随意选，参数2 必须写，不用赋值  
    {
        if (wayPoint.simpleWaypointGroup && wayPoint.simpleWaypointGroup.IsUseBezierSpline)
            return;

        if (wayPoint.nextWayPoint)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(wayPoint.transform.position, wayPoint.spehreSize);

            Gizmos.color = Color.cyan;   //绘制时颜色  
            Gizmos.DrawLine(wayPoint.transform.position + deltaGizmo, wayPoint.nextWayPoint.transform.position + deltaGizmo);
        }
        else//End
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(wayPoint.transform.position, wayPoint.spehreSize);
        }
    }
#endif


}
