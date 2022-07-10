using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ���͵�ָ����RDP����һ����ͬ�����£�
/// </summary>
public class RemoteDestinationPointHelper : ComponentHelperBase<RemoteDestinationPoint>
{
    public bool keepAngle = true;//��֤���ͺ�DP�븸����ļнǵ����������нǣ������û��ڴ���ʱŤͷ���´���Ҫ��ѡAlignToDP��

    Vector3 extraAngle;//������������ƫ��ֵ
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
