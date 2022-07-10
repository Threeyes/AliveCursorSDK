using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 汽车沿着路标形式
/// </summary>
public class CarController : SimpleWayPointFollower
{
    public AudioSource audioSourceDriving;

    protected override void MoveStart()
    {
        base.MoveStart();

        if (audioSourceDriving)
            audioSourceDriving.Play();
    }

    protected override void MoveFinish()
    {
        base.MoveFinish();

        if (audioSourceDriving)
            audioSourceDriving.Stop();
    }

    public void ChangeSpeed(float percent)
    {
        moveSpeed *= percent;
    }
}
