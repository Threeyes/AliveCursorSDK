using UnityEngine;
using UnityEngine.Events;

public class PhysicsRaycastRecevier : MonoBehaviour
{
    public bool IsActive { get { return isActive; } set { isActive = value; } }
    public bool isActive = true;

    public BoolEvent onHoverUnHover;
    public UnityEvent onHover;
    public UnityEvent onUse;

    public Vector3Event onVector3Use;

    public bool isHover = false;

    public string targetId = "";

    public virtual void Hover(bool isOn)
    {
        if (!IsActive)
            return;

        if (isOn != isHover)
        {
            onHoverUnHover.Invoke(isOn);
            if (isOn)
                onHover.Invoke();
            isHover = isOn;
        }
    }

    public virtual void Use(RaycastHit raycastHit)
    {
        onVector3Use.Invoke(raycastHit.point);
        Use();
    }

    public virtual void Use()
    {
        if (!IsActive)
            return;

        if (isHover)//需要HoverToUse
        {
            onUse.Invoke();
        }
    }
}
