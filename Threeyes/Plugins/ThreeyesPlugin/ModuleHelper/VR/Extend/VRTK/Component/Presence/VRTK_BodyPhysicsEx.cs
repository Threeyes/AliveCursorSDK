#if USE_VRTK
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
/// <summary>
/// 
/// </summary>
public class VRTK_BodyPhysicsEx : VRTK_BodyPhysics
{
    public bool IsPlayerStayKinematic = true;//玩家保持不受外力影响,适用于攀爬

    protected override void GenerateRigidbody()
    {
        base.GenerateRigidbody();
        if(IsPlayerStayKinematic)
        {
            bodyRigidbody.isKinematic = true;
        }
    }
    protected override void TogglePhysics(bool state)
    {
        //不修改kinect的状态
        if (IsPlayerStayKinematic)
        {
            return;
        }
        else
        {
            base.TogglePhysics(state);
        }
    }


}

#endif