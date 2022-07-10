using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
/// <summary>
/// 实时更新LineRenderer的属性, 针对两个点的片段
/// </summary>
[ExecuteInEditMode]
public class LineRendererHelper : ComponentHelperBase<LineRenderer>
{
    public static string defaultName = "Point";

    public Transform TargetPoint { get { return targetPoint; } set { targetPoint = value; } }
    public Transform targetPoint;
    public bool useWorldSpace = true;

    /// <summary>
    /// 运行时计算并更新 LineRenderer
    /// </summary>
    public bool isRunTimeUpdatePointPos = true;
    private void Start()
    {
        Comp.positionCount = 2;
        Comp.useWorldSpace = useWorldSpace;

        UpdateLinePoint();

        //不需要实时更新，直接禁用
        if (!isRunTimeUpdatePointPos && Application.isPlaying)
            this.enabled = false;
    }

    public void Clear()
    {
        TargetPoint = null;
        Comp.positionCount = 0;
    }
    private void LateUpdate()
    {

#if UNITY_EDITOR
        UpdateLineRenderer();
#endif

        UpdateLinePoint();
    }

    void UpdateLinePoint()
    {
        if (!TargetPoint)
            return;

        Comp.SetPosition(0, useWorldSpace ? transform.position : transform.localPosition);
        Comp.SetPosition(1, useWorldSpace ? TargetPoint.position : TargetPoint.localPosition);
    }
    [Header("Editor Setting")]
    /// <summary>
    /// 在编辑器中计算并更新  LineRenderer 的各个点的坐标
    /// </summary>
    public bool isEditorUpdatePointPos = false;
    public Vector3 space = new Vector3(0, 0, 0);

    [ContextMenu("UpdateLineRenderer")]
    void UpdateLineRenderer()
    {
        if (Application.isPlaying)
            return;

        if (!TargetPoint)
            return;


        Comp.positionCount = 2;
        if (isEditorUpdatePointPos)
        {
            TargetPoint.localPosition = space;
        }
    }
}
