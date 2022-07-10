using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointBreakShakeExecuteListener : ShakeExecuteListener
{
    new Rigidbody rigidbody;
    public Joint joint;
    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        if (!joint)
            joint = GetComponent<Joint>();
        isExecuteOnce = true;
    }

    protected override void OnShakeIncrease()
    {
        base.OnShakeIncrease();

        if (joint)
        {
            ////避免某些物体Sleep
            //if (rigidbody)
            //    rigidbody.WakeUp();

            joint.breakForce = 0;
            Invoke("WakeUp", 0.1f);
        }
    }

    void WakeUp()
    {
        if (rigidbody)
        {
            rigidbody.WakeUp();
            //rigidbody.mass = rigidbody.mass;
        }
    }
}
