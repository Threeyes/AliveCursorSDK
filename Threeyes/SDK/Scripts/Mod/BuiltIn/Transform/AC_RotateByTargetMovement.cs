using Threeyes.Config;
using Threeyes.GameFramework;
using UnityEngine;

/// <summary>
/// Rotate base on target's Movement
/// 
/// PS:
/// 1.Will take Cursor Size into account
/// </summary>
public class AC_RotateByTargetMovement : RotateByTargetMovement
{
    protected override float RuntimeRotateSpeed
    {
        get
        {
            return Config.rotateSpeed / AC_ManagerHolder.CommonSettingManager.CursorSize;
        }
    }
}