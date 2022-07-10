#if USE_RhinoX
using Ximmerse.RhinoX;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class VRInterface : InstanceBase<VRInterface>
{
    Transform tfCameraRigParentCache;
    Transform tfCameraRigCache;
    Transform tfCameraCache;
    Camera cameraCache;

    static Transform GetTFCameraRigParentFunc()
    {
        if (!Instance.tfCameraRigParentCache)
            Instance.tfCameraRigParentCache = tfCameraRig.parent;
        return Instance.tfCameraRigParentCache;
    }
    static Transform GetTFCameraRigFunc()
    {
        if (!Instance.tfCameraRigCache)
            Instance.tfCameraRigCache = tfCameraEye.parent;
        return Instance.tfCameraRigCache;
    }

    static Transform GetTFCameraEyeFunc()
    {
        if (!Instance.tfCameraCache)
            Instance.tfCameraCache = GetVRCameraFunc() ? GetVRCameraFunc().transform : null;
        return Instance.tfCameraCache;
    }
    static Camera GetVRCameraFunc()
    {
        if (!Instance.cameraCache)
            Instance.cameraCache = ARCamera.Instance.GetComponent<Camera>();
        return Instance.cameraCache;
    }
    #region Completed

    #endregion
}
#endif
