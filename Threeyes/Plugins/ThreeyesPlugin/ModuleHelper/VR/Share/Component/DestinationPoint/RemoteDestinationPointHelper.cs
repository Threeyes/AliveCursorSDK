using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 传送到指定的RDP（不一定是同物体下）
/// </summary>
public class RemoteDestinationPointHelper : ComponentHelperBase<RemoteDestinationPoint>
{
    public bool keepAngle = true;//保证传送后DP与父物体的夹角等于与该物体夹角，避免用户在传送时扭头导致错误（要勾选AlignToDP）

    Vector3 extraAngle;//保存与该物体的偏移值
    public void Teleport()
    {
        if (keepAngle)
        {
            extraAngle = transform.eulerAngles - VRInterface.tfCameraRig.eulerAngles;

            Comp.actionAfterTeleport += OnAfterTeleport;
        }
        Comp.SetPos();
    }

    private void OnAfterTeleport()
    {
        Comp.actionAfterTeleport -= OnAfterTeleport;

        if (keepAngle)
        {
            VRInterface.tfCameraRig.eulerAngles-=extraAngle;
        }
    }
}
