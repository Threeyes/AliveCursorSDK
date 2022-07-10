#if !(USE_OpenXR || USE_VRTK || USE_VIU || USE_RhinoX)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 当所有VR插件都找不到时激活
/// </summary>
public partial class VRInterface : InstanceBase<VRInterface>
{
#region  partial Method

    static Transform GetTFCameraRigParentFunc()
    {
        return Instance.tfCameraRigParentOverride;
    }
    static Transform GetTFCameraRigFunc()
    {
        return Instance.tfCameraRigOverride;
    }

    static Transform GetTFCameraEyeFunc()
    {
        return Instance.tfCameraEyeOverride;
    }
    static Camera GetVRCameraFunc()
    {
        return Instance.vrCameraOverride ? Instance.vrCameraOverride : Camera.main;
    }

#endregion

}
#endif