using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsRayDrawer : ComponentHelperBase<PhysicsRayCaster>
{
    public LineRenderer lineRenderer;

    private void Awake()
    {
        if (!lineRenderer)
            lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = 2;

    }
    private void Update()
    {
        Debug.DrawRay(Comp.ray.origin, Comp.ray.direction);

        if (Comp.isHit)
        {
            lineRenderer.SetPosition(0, Comp.ray.origin);
            lineRenderer.SetPosition(1, Comp.raycastHit.point);
        }
        else
        {
            lineRenderer.SetPosition(0, Comp.ray.origin);
            Vector3 result = Comp.Target.TransformDirection(Vector3.forward*1000);
            lineRenderer.SetPosition(1, result);
        }
    }

}
