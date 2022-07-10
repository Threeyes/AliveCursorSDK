using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 输出Joint的各项属性
/// </summary>
public class LogRigidbodyProperty : MonoBehaviour
{
    Joint joint;
    Rigidbody rig;
    public float curForce;
    public Vector3 curVelocity;

    public Vector3 maxCollisionRelativeVelocity;
    public Vector3 collisionRelativeVelocity;
    public bool isPrint = false;
    private void Awake()
    {
        joint = GetComponent<Joint>();
        rig = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        collisionRelativeVelocity = collision.relativeVelocity;
        if (collisionRelativeVelocity.magnitude > maxCollisionRelativeVelocity.magnitude)
            maxCollisionRelativeVelocity = collisionRelativeVelocity;
        if (isPrint)
        {
            print("OnCollisionEnter:" + collision.gameObject.name + "  " + collisionRelativeVelocity);
        }
    }

    private void FixedUpdate()
    {
        if (rig)
        {
            curVelocity = rig.velocity;
            if (curVelocity.magnitude > 0)
                if (isPrint)
                    print("Velocity: " + curVelocity);
        }
        if (joint)
        {
            curForce = joint.currentForce.magnitude;
            print("Force:" + joint.currentForce + "(" + joint.currentForce.magnitude + ")" + "   Torque:" + joint.currentTorque);
        }
    }
}
