using System;
using UnityEngine;
#if USE_VIU
[RequireComponent(typeof(VIU_InteractableObject))]
#elif USE_VRTK
using VRTK;
#endif
/// <summary>
/// VR通用的按键事件监听
/// </summary>
public class VR_InteractableObject_UnityEvents :
#if USE_VIU
    ComponentHelperBase<VIU_InteractableObject>
#elif USE_VRTK
        ComponentHelperBase<VRTK_InteractableObject>
#else
     MonoBehaviour
#endif
{

    public ObjectEvent OnTouch = new ObjectEvent();
    public ObjectEvent OnUntouch = new ObjectEvent();

    public ObjectEvent OnGrab = new ObjectEvent();
    public ObjectEvent OnUngrab = new ObjectEvent();

    public ObjectEvent OnUse = new ObjectEvent();
    public ObjectEvent OnUnuse = new ObjectEvent();

    public BoolEvent OnHoverUnHover = new BoolEvent();

    public ObjectEvent OnHover = new ObjectEvent();
    public ObjectEvent OnUnHover = new ObjectEvent();

#if USE_VIU

    private void Start()
    {
        AddListeners();
    }

    protected virtual void AddListeners()
    {
        Comp.InteractableObjectTouched += Touch;
        Comp.InteractableObjectUntouched += UnTouch;

        Comp.InteractableObjectGrabbed += (Grab);
        //PS:beforeRelease可能只是按键抬起，不一定是放下
        Comp.InteractableObjectUngrabbed -= (UnGrab);

        Comp.InteractableObjectUsed += Use;
        Comp.InteractableObjectUnused += Unuse;

        Comp.InteractableObjectHover += Hover;
        Comp.InteractableObjectUnHover += UnHover;
    }

#if UNITY_EDITOR

    [ContextMenu("SyncSetting")]
    void SyncSetting()
    {
        //Todo：从VRTK的相关组件中复制其UnityEvent
    }

#endif

#elif USE_VRTK

    public bool isUseOnTriggerPress = true;//按下扳机代表使用

    private void Start()
    {
        AddListeners();
    }

    protected virtual void AddListeners()
    {
        Comp.InteractableObjectUsed += Hover_VRTK;
        Comp.InteractableObjectUnused += UnHover_VRTK;
    }

    public GameObject interactingObject;//当前交互的物体(如Controller）

    private void Hover_VRTK(object sender, InteractableObjectEventArgs e)
    {
        interactingObject = e.interactingObject;
        VRTK_ControllerEvents ce = interactingObject.GetComponent<VRTK_ControllerEvents>();
        if (ce)
        {
            ce.TriggerPressed += DoTriggerPressed;
        }
        Hover(sender);
    }

    private void DoTriggerPressed(object sender, ControllerInteractionEventArgs e)
    {
        if (isUseOnTriggerPress)
        {
            Use(sender);
        }
    }

    private void UnHover_VRTK(object sender, InteractableObjectEventArgs e)
    {
        if (interactingObject != null && e.interactingObject == interactingObject)
        {
            VRTK_ControllerEvents ce = interactingObject.GetComponent<VRTK_ControllerEvents>();
            if (ce)
            {
                ce.TriggerPressed -= DoTriggerPressed;
            }
        }

        UnHover(sender);
        interactingObject = null;
    }


#endif

    #region Common

    private void Hover(object sender)
    {
        OnHoverUnHover.Invoke(true);
        OnHover.Invoke(sender);
    }
    private void UnHover(object sender)
    {
        OnHoverUnHover.Invoke(false);
        OnUnHover.Invoke(sender);
    }


    private void Touch(object o)
    {
        OnTouch.Invoke(o);
    }

    private void UnTouch(object o)
    {
        OnUntouch.Invoke(o);
    }

    private void Grab(object o)
    {
        OnGrab.Invoke(o);
    }

    private void UnGrab(object o)
    {
        OnUngrab.Invoke(o);
    }

    private void Use(object o)
    {
        OnUse.Invoke(o);
    }

    private void Unuse(object o)
    {
        OnUnuse.Invoke(o);
    }

    #endregion

}