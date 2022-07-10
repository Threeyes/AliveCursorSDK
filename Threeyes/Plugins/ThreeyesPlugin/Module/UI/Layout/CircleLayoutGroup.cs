using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleLayoutGroup : ComponentGroupBase<Transform>
{
    private void Reset()
    {
        isRecursive = false;
    }

    public Transform tfPivotOverride;//重置中点
    bool isLookAtPivot = true;

    public float radius = 1f;

    public Transform tfPivot { get { return tfPivotOverride ? tfPivotOverride : transform; } }

    [ContextMenu("ReArrange")]
    public void ReArrange()
    {
        for (int i = 0; i != ListComp.Count; i++)
        {
            float anglePerUnit = 360 / ListComp.Count;
            Transform tfEle = ListComp[i];
            tfEle.position = tfPivot.TransformPoint(new Vector3(0, 0, radius));
            tfEle.localEulerAngles = default(Vector3);
            tfEle.RotateAround(tfPivot.position, Vector3.up, i * anglePerUnit);
        }
    }


}
