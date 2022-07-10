#if USE_VRTK
using UnityEngine;
using VRTK;
/// <summary>
/// 接触到传送点的碰撞体后，隐藏该传送点
/// </summary>
public class TriggerDestinationPoint : RemoteDestinationPoint
{
    private void OnTriggerEnter(Collider other)
    {
        if (VRInterface.IsVRPlayerWholeBody(other))
        {
            currentDestinationPoint = this;
            DisablePoint();
        }
    }
    protected override void ResetPoint()
    {
        if (snapToPoint && currentDestinationPoint == this)
        {
            return;
        }

        ToggleObject(hoverCursorObject, false);
        if (enableTeleport)
        {
            pointCollider.enabled = true;
            ToggleObject(defaultCursorObject, true);
            ToggleObject(lockedCursorObject, false);
            OnDestinationPointUnlocked();
        }
        else
        {
            //pointCollider.enabled = false;//保持碰撞体激活状态（PS： Teleporter只会根据碰撞体来判断是否能传送，因此还要把该物体的Layer改为Ignore Raycast）
            ToggleObject(lockedCursorObject, true);
            ToggleObject(defaultCursorObject, false);
            OnDestinationPointLocked();
        }
        OnDestinationPointReset();
    }
}
#endif