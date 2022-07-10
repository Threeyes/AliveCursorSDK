
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
/// <summary>
/// 显示特定的Gizmo
/// </summary>
public class ShowGizmos : MonoBehaviour
{
#if UNITY_EDITOR
    //static Vector3 deltaGizmo = new Vector3(0, 0.1f, 0);
    [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NonSelected)]
    public bool allowScaling = true;
    static void MyGizmo(ShowGizmos showGizmos, GizmoType gizmoType)  //参数1为“XX”组件，可以随意选，参数2 必须写，不用赋值  
    {
        Gizmos.color = Color.green;   //绘制时颜色  
        Gizmos.DrawIcon(showGizmos.transform.position, showGizmos.name, showGizmos.allowScaling);
    }
#endif
}