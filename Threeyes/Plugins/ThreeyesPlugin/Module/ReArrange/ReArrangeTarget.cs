using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReArrangeTarget : MonoBehaviour
{

    public bool isReParent = false;//是否作为TargetPoint的父物体

    public void SetPos(ReArrangePoint reArrangePoint)
    {
        transform.position = reArrangePoint.transform.position;
    }

}
