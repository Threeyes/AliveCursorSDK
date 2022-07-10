#if USE_VRTK
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VRTK;
using VRTK.SecondaryControllerGrabActions;
/// <summary>
/// 抓取物品
/// </summary>
public class SecondHandGrab : VRTK_BaseGrabAction
{
    public UnityEvent onGrab;
    public UnityEvent onUnGrab;
    public override void Initialise(VRTK_InteractableObject currentGrabbdObject, VRTK_InteractGrab currentPrimaryGrabbingObject, VRTK_InteractGrab currentSecondaryGrabbingObject, Transform primaryGrabPoint, Transform secondaryGrabPoint)
    {
        base.Initialise(currentGrabbdObject, currentPrimaryGrabbingObject, currentSecondaryGrabbingObject, primaryGrabPoint, secondaryGrabPoint);

        Grab();
    }

    public override void OnDropAction()
    {
        base.OnDropAction();
        print("UnGrab!");
        onUnGrab.Invoke();
    }

    void Grab()
    {
        print("Grab!");
        onGrab.Invoke();
    }

    //Debug
#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            Grab();
        }
    }
#endif
}
#endif