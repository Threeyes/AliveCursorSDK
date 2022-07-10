#if USE_OpenXR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public partial class RemoteDestinationPoint : BaseTeleportationInteractable
{
    public static event UnityAction<Transform, BaseInteractionEventArgs> DestinationMarkerSet;

    #region  Partial Method

    protected void AddListenerFunc()
    {
        DestinationMarkerSet += DoDestinationMarkerSet;
    }
    protected void RemoveListenerFunc()
    {
        DestinationMarkerSet -= DoDestinationMarkerSet;
    }

    private void DoDestinationMarkerSet(Transform tfTeleportTo, BaseInteractionEventArgs arg)
    {

    }

    #endregion

    protected override void OnHoverEntered(HoverEnterEventArgs args)
    {
        base.OnHoverEntered(args);
        EnablePoint();
    }
    protected override void OnHoverExited(HoverExitEventArgs args)
    {
        base.OnHoverExited(args);
        ResetPoint();
    }

    protected override bool GenerateTeleportRequest(XRBaseInteractor interactor, RaycastHit raycastHit, ref TeleportRequest teleportRequest)
    {
        //Debug.Log(name + " GenerateTeleportRequest");//顺序2
        DoDestinationMarkerSetFunc(transform, raycastHit.point);
        return false;//PS:return true就会使用XR的传送系统

        //teleportRequest.destinationPosition = raycastHit.point;
        //teleportRequest.destinationRotation = transform.rotation;
        //return true;
    }


    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        //BeginTeleport
        DestinationMarkerSet.Invoke(transform, args);
        //Debug.Log(name + " GenerateTeleportRequest");//顺序1
    }
}
#endif
