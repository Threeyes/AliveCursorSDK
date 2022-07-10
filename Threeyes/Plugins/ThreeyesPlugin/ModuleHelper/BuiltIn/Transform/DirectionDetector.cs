using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
//到达指定朝向时，执行事件
public class DirectionDetector : ComponentHelperBase<Transform>
{
    public BoolEvent onReachUnReach;
    public Vector3 vt3TargetRot = new Vector3(0, 0, 120f);

    CheckValueType checkValueType = CheckValueType.Greater;
    bool isReach = false;
    private void Update()
    {
        bool isCurReach = CheckReach;
        if(isReach!=isCurReach)
        {
            isReach = isCurReach;
            onReachUnReach.Invoke(isReach);

        }
    }

    bool CheckReach
    {
        get
        {
            Vector3 vt3LocalEA = Comp.localEulerAngles;
            return vt3LocalEA.z > vt3TargetRot.z && vt3LocalEA.z < 360 - vt3TargetRot.z;
        }
    }

    public enum CheckValueType
    {
        Equal,
        Greater,
        Less
    }


}
