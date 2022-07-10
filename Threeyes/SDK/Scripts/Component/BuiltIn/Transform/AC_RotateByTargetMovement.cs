using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Rotate base on target's Movement
/// 
/// PS:
/// 1.Will take Cursor Size into account
/// </summary>
public class AC_RotateByTargetMovement : AC_ConfigableUpdateComponentBase<Transform, AC_SORotateByTargetMovementConfig, AC_RotateByTargetMovement.ConfigInfo>
{
    public Transform target;

    //Runtime
    protected Vector3 lastTargetPos;
    protected Vector3 movedVectorLastFrame;

    protected override void UpdateFunc()
    {
        if (!target)
            return;

        movedVectorLastFrame = target.position - lastTargetPos;
        RotateThis();
        lastTargetPos = target.position;
    }

    protected virtual void RotateThis()
    {
        Vector3 VectorRightAxis = Vector3.Cross(-target.forward, movedVectorLastFrame).normalized;//Get the right Axis base on current movement vector
        Comp.Rotate(VectorRightAxis, movedVectorLastFrame.magnitude * Config.rotateSpeed / AC_ManagerHolder.CommonSettingManager.CursorSize, Space.World);//绕移动方向的对应轴旋转（移动单位需要乘以缩放的倍数）
    }

    #region Define

    [System.Serializable]
    public class ConfigInfo : AC_SerializableDataBase
    {
        public float rotateSpeed = 360f;//rotate speed when the cursor size is 1
    }

    #endregion
}

