﻿
using System;
using UnityEngine;
using UnityEngine.Events;
#if USE_VRTK
using VRTK;
using VRTK.UnityEventHelper;
#endif

public class VRTK_InteractableObject_UnityEventsEx :
#if USE_VRTK
    VRTK_UnityEvents<VRTK_InteractableObject>
#else
    MonoBehaviour
#endif
{
    public BoolEvent onUseUnUse;
    public UnityEvent onSelect;

#if USE_VRTK
    [Serializable]
    public sealed class InteractableObjectEvent : UnityEvent<object, InteractableObjectEventArgs> { }

    public InteractableObjectEvent OnTouch = new InteractableObjectEvent();
    public InteractableObjectEvent OnUntouch = new InteractableObjectEvent();

    public InteractableObjectEvent OnGrab = new InteractableObjectEvent();
    public InteractableObjectEvent OnUngrab = new InteractableObjectEvent();


    public InteractableObjectEvent OnUse = new InteractableObjectEvent();
    public InteractableObjectEvent OnUnuse = new InteractableObjectEvent();

    public InteractableObjectEvent OnEnterSnapDropZone = new InteractableObjectEvent();
    public InteractableObjectEvent OnExitSnapDropZone = new InteractableObjectEvent();
    public InteractableObjectEvent OnSnapToDropZone = new InteractableObjectEvent();
    public InteractableObjectEvent OnUnsnapFromDropZone = new InteractableObjectEvent();

    public bool isSelectOnTriggerPress = true;

    protected override void AddListeners(VRTK_InteractableObject component)
    {
        component.InteractableObjectTouched += Touch;
        component.InteractableObjectUntouched += UnTouch;

        component.InteractableObjectGrabbed += Grab;
        component.InteractableObjectUngrabbed += UnGrab;

        component.InteractableObjectUsed += Use;
        component.InteractableObjectUnused += Unuse;

        component.InteractableObjectEnteredSnapDropZone += EnterSnapDropZone;
        component.InteractableObjectExitedSnapDropZone += ExitSnapDropZone;
        component.InteractableObjectSnappedToDropZone += SnapToDropZone;
        component.InteractableObjectUnsnappedFromDropZone += UnsnapFromDropZone;
    }

    protected override void RemoveListeners(VRTK_InteractableObject component)
    {
        component.InteractableObjectTouched -= Touch;
        component.InteractableObjectUntouched -= UnTouch;

        component.InteractableObjectGrabbed -= Grab;
        component.InteractableObjectUngrabbed -= UnGrab;

        component.InteractableObjectUsed -= Use;
        component.InteractableObjectUnused -= Unuse;

        component.InteractableObjectEnteredSnapDropZone -= EnterSnapDropZone;
        component.InteractableObjectExitedSnapDropZone -= ExitSnapDropZone;
        component.InteractableObjectSnappedToDropZone -= SnapToDropZone;
        component.InteractableObjectUnsnappedFromDropZone -= UnsnapFromDropZone;
    }

    private void Touch(object o, InteractableObjectEventArgs e)
    {
        OnTouch.Invoke(o, e);
    }

    private void UnTouch(object o, InteractableObjectEventArgs e)
    {
        OnUntouch.Invoke(o, e);
    }

    private void Grab(object o, InteractableObjectEventArgs e)
    {
        OnGrab.Invoke(o, e);
    }

    private void UnGrab(object o, InteractableObjectEventArgs e)
    {
        OnUngrab.Invoke(o, e);
    }

    private void Use(object o, InteractableObjectEventArgs e)
    {
        onUseUnUse.Invoke(true);
        OnUse.Invoke(o, e);
    }

    private void Unuse(object o, InteractableObjectEventArgs e)
    {
        onUseUnUse.Invoke(false);
        OnUnuse.Invoke(o, e);
    }

    private void EnterSnapDropZone(object o, InteractableObjectEventArgs e)
    {
        OnEnterSnapDropZone.Invoke(o, e);
    }

    private void ExitSnapDropZone(object o, InteractableObjectEventArgs e)
    {
        OnExitSnapDropZone.Invoke(o, e);
    }

    private void SnapToDropZone(object o, InteractableObjectEventArgs e)
    {
        OnSnapToDropZone.Invoke(o, e);
    }

    private void UnsnapFromDropZone(object o, InteractableObjectEventArgs e)
    {
        OnUnsnapFromDropZone.Invoke(o, e);
    }
#endif

}