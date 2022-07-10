using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRPhysicsRayCaster : PhysicsRayCaster
{
    protected override Camera CamRaycast
    {
        get
        {
            return VRInterface.vrCamera;
        }
    }
}
