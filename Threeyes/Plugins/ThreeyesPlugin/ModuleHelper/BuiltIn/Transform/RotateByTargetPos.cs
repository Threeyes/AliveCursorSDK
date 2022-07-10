using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class RotateByTargetPos : GameObjectBase
{
    public Transform target;
    public float rotateSpeed = 120f;

    public Vector3 lastTargetPos;
    protected Vector3 movedVectorLastFrame;
    public Vector3 localForwardAxis = Vector3.forward;//局部正向的方向(基于target）
    public Vector3 localRotateAxis = new Vector3(1, 0, 0);//旋转轴（基于自身）
    private void Update()
    {
        if (!target)
            return;

        movedVectorLastFrame = target.position - lastTargetPos;
        RotateThis();
        lastTargetPos = target.position;
    }

    protected virtual void RotateThis()
    {
        float vLength = movedVectorLastFrame.magnitude;
        float dot = Vector3.Dot(movedVectorLastFrame, target.TransformVector(localForwardAxis));
        float sign = dot >= 0 ? 1 : -1;//符号决定旋转的正反
        tfThis.Rotate(localRotateAxis, sign * vLength * rotateSpeed / target.localScale.x, Space.Self);//乘以缩放的倍数
    }
}

