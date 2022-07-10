using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 随机设置 joint的breakForce 
/// </summary>
[RequireComponent(typeof(Joint))]
public class RandomJointValue : RandomValueComponentBase<Random_Float, float>
{
    public Joint joint;

    public override void OnSet()
    {
        if (!joint)
            joint = GetComponent<Joint>();
        joint.breakForce = ResultValue;
        base.OnSet();
    }
}
