using Newtonsoft.Json;
using Threeyes.Config;
using Threeyes.Steamworks;
using UnityEngine;

/// <summary>
/// Tilt rotate around the axis
/// </summary>
public class AC_TiltByTargetMovement : TiltByTargetMovement
{
    protected override void UpdateFunc()
    {
        if (AC_ManagerHolder.TransformManager.ActiveController is AC_DefaultTransformController defaultTransformController)
        {
            if (!defaultTransformController.Config.isFixedAngle)//Only valid on FixedAngle
                return;
        }
        base.UpdateFunc();
    }
}
