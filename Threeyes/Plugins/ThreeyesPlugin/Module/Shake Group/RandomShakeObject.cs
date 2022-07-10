using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
/// <summary>
/// 摇晃平台
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class RandomShakeObject : ShakeListenerBase
{
    Rigidbody rig;
    public Vector3 startPos;
    public Vector3 avaliAxis = new Vector3(1, 0, 1);//允许的方向
    public Vector3 outPower;
    private void Awake()
    {
        startPos = transform.position;
        rig = GetComponent<Rigidbody>();
    }
    protected override void OnShaking(float percent)
    {

        PulseShake(percent);
        return;

        //rig.isKinematic = false;

        //outPower = UnityEngine.Random.insideUnitSphere;
        //outPower.Scale(avaliAxis);
        //outPower *= percent;


        ////rig.velocity = outPower;

        //Vector3 targetPos = outPower;
        //Vector3 targetVelocity = targetPos - transform.localPosition;
        //if (targetVelocity.Avaliable())
        //{
        //    //rig.velocity = targetVelocity;
        //    rig.AddForce(targetVelocity);
        //    //rig.position = startPos + outPower;
        //}
    }

    //不建议
    public float durationTime = 0.2f;
    public float lastTime = 0;
    public ForceMode forceMode = ForceMode.Force;
    void PulseShake(float percent)
    {
        if (Time.time < lastTime + durationTime)
            return;
        lastTime = Time.time;

        rig.isKinematic = false;

        outPower = UnityEngine.Random.insideUnitSphere;
        outPower.Scale(avaliAxis);
        outPower *= percent;

        //rig.velocity = outPower;

        Vector3 targetPos = startPos + outPower;
        Vector3 targetVelocity = targetPos - transform.localPosition;
        if (targetVelocity.Avaliable())
        {
            //rig.AddForce(targetVelocity, forceMode);
            rig.AddTorque(targetVelocity, forceMode);
            //rig.velocity = targetVelocity;
        }
        //rig.position = outPower;

    }

    protected override void OnShakeStop()
    {
        //rig.position = startPos;
        rig.velocity = default(Vector3);//复位
        rig.isKinematic = true;
    }
}
