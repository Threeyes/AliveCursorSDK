using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 针对多个点，实时更新LineRenderer的属性
/// </summary>
[ExecuteInEditMode]
public class LineRendererGroupHelper : ComponentHelperBase<LineRenderer>
{
    public List<Transform> listTfPoint = new List<Transform>();
    public bool useWorldSpace = true;

    public bool isAutoInitPoints = true;
    /// <summary>
    /// 运行时计算并更新 LineRenderer
    /// </summary>
    public bool isRunTimeUpdatePointPos = true;
    private void Start()
    {
        Comp.positionCount = listTfPoint.Count;
        Comp.useWorldSpace = useWorldSpace;

        UpdateLinePoint();

        //不需要实时更新，直接禁用
        if (!isRunTimeUpdatePointPos)
            this.enabled = false;
    }

    [ContextMenu("InitPoints")]
    void InitPoints()
    {
        listTfPoint.Clear();
        foreach (Transform tfChild in transform)
        {
            listTfPoint.Add(tfChild);
        }
    }
    private void LateUpdate()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            UpdateLineRenderer();
#endif

        UpdateLinePoint();
    }

    void UpdateLinePoint()
    {
        for (int i = 0; i != listTfPoint.Count; i++)
        {
            Comp.SetPosition(i, useWorldSpace ? listTfPoint[i].position : listTfPoint[i].localPosition);
        }
    }
    [Header("Editor Setting")]
    /// <summary>
    /// 在编辑器中计算并更新  LineRenderer 的各个点的坐标
    /// </summary>
    public bool isEditorUpdatePointPos = false;
    public Vector3 space = new Vector3(0, -0.5f, 0);
    public bool isSetupCollider = false;//上一个节点朝向下一个节点，用于设置碰撞体
    [ContextMenu("UpdateLineRenderer")]
    void UpdateLineRenderer()
    {
        InitPoints();

        Comp.positionCount = listTfPoint.Count;
        //print("In");
        if (isSetupCollider)
        {
            for (int i = 0; i != listTfPoint.Count; i++)
            {
                if (i < listTfPoint.Count - 1)
                {
                    Transform curTf = listTfPoint[i];
                    Transform nextTf = listTfPoint[i + 1];
                    curTf.LookAt(nextTf);
                    //BoxCollider boxCollider = curTf.GetComponent<BoxCollider>();
                    //if (boxCollider)
                    //{
                    //    float distance = (curTf.localPosition + nextTf.localPosition).magnitude;
                    //    boxCollider.center = (curTf.localPosition + nextTf.localPosition) / 2;
                    //    boxCollider.size = new Vector3(boxCollider.size.x, boxCollider.size.y, distance);
                    //}
                }
            }
        }

        if (isEditorUpdatePointPos)
        {
            for (int i = 0; i != listTfPoint.Count; i++)
            {
                listTfPoint[i].localPosition = space * i;
            }
        }
    }


#if UNITY_EDITOR

    [DrawGizmo(GizmoType.InSelectionHierarchy)]
    static void MyGizmo(LineRendererGroupHelper lineRendererGroupHelper, GizmoType gizmoType)
    {
        lineRendererGroupHelper.UpdateLineRenderer();
    }
#endif
}
