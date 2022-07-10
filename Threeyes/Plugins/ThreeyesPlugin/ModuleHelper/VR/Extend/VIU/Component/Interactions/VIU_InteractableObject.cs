#if USE_VIU
using HTC.UnityPlugin.ColliderEvent;
using HTC.UnityPlugin.Utility;
using HTC.UnityPlugin.Vive;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using GrabberPool = HTC.UnityPlugin.Utility.ObjectPool<VIU_InteractableObject.Grabber>;
using Threeyes.Coroutine;

public class VIU_InteractableObject : GrabbableBase<VIU_InteractableObject.Grabber>
    , IColliderEventPressUpHandler
    , IColliderEventPressDownHandler
    , IColliderEventHoverEnterHandler
    , IColliderEventHoverExitHandler
   , IPointerClickHandler
    , IPointerEnterHandler
    , IPointerExitHandler
{
#region StickyGrabbable

    public class Grabber : IGrabber
    {
        private static GrabberPool m_pool;

        public ColliderButtonEventData eventData { get; private set; }

        public RigidPose grabberOrigin
        {
            get
            {
                return new RigidPose(eventData.eventCaster.transform);
            }
        }

        public RigidPose grabOffset { get; set; }

        // NOTE:
        // We can't make sure the excution order of OnColliderEventPressDown() and Update()
        // Hence log grabFrame to avoid redundant release in Update()
        // and redeayForRelease flag(remove grabber from m_eventGrabberSet one frame later) to avoid redundant grabbing in OnColliderEventPressDown()
        public int grabFrame { get; set; }
        public bool redeayForRelease { get; set; }

        public static Grabber Get(ColliderButtonEventData eventData)
        {
            if (m_pool == null)
            {
                m_pool = new GrabberPool(() => new Grabber());
            }

            var grabber = m_pool.Get();
            grabber.eventData = eventData;
            grabber.redeayForRelease = false;
            return grabber;
        }

        public static void Release(Grabber grabber)
        {
            grabber.eventData = null;
            m_pool.Release(grabber);
        }
    }

    [Serializable]
    public class UnityEventGrabbable : UnityEvent<VIU_InteractableObject> { }

    private IndexedTable<ColliderButtonEventData, Grabber> m_eventGrabberSet;

    //自定义
    [Header("自定义")]
    public bool isFollowPos = true;//跟随位置
    public bool isFollowRot = true;//跟随旋转
    public bool isUseLocalRotation = false;//是否使用局部旋转，适用于Wheel、Knob等物体（针对Rot）
    public Vector3 allowPosAxis = new Vector3(1, 1, 1);//允许更改的位置
    public Vector3 allowRotAxis = new Vector3(1, 1, 1);//允许更改的旋转
    public Vector2 angleRange = new Vector2();


    public bool alignPosition;
    public bool alignRotation;
    public Vector3 alignPositionOffset;
    public Vector3 alignRotationOffset;
    [Range(MIN_FOLLOWING_DURATION, MAX_FOLLOWING_DURATION)]
    [FormerlySerializedAs("followingDuration")]
    [SerializeField]
    private float m_followingDuration = DEFAULT_FOLLOWING_DURATION;
    [FormerlySerializedAs("overrideMaxAngularVelocity")]
    [SerializeField]
    private bool m_overrideMaxAngularVelocity = true;
    [FormerlySerializedAs("unblockableGrab")]
    [SerializeField]
    private bool m_unblockableGrab = true;
    [SerializeField]
    private ColliderButtonEventData.InputButton m_grabButton = ColliderButtonEventData.InputButton.Trigger;
    [SerializeField]
    private bool m_toggleToRelease = true;
    [FormerlySerializedAs("m_multipleGrabbers")]
    [SerializeField]
    private bool m_allowMultipleGrabbers = false;
    [FormerlySerializedAs("afterGrabbed")]
    [SerializeField]
    private UnityEventGrabbable m_afterGrabbed = new UnityEventGrabbable();
    [FormerlySerializedAs("beforeRelease")]
    [SerializeField]
    private UnityEventGrabbable m_beforeRelease = new UnityEventGrabbable();
    [FormerlySerializedAs("onDrop")]
    [SerializeField]
    private UnityEventGrabbable m_onDrop = new UnityEventGrabbable(); // change rigidbody drop velocity here

    public override float followingDuration { get { return m_followingDuration; } set { m_followingDuration = Mathf.Clamp(value, MIN_FOLLOWING_DURATION, MAX_FOLLOWING_DURATION); } }

    public override bool overrideMaxAngularVelocity { get { return m_overrideMaxAngularVelocity; } set { overrideMaxAngularVelocity = value; } }

    public bool unblockableGrab { get { return m_unblockableGrab; } set { m_unblockableGrab = value; } }

    //（抓起物体后）是否可以通过再次按下扳机，释放物体（false：适用于一直抓着物体）
    public bool toggleToRelease { get { return m_toggleToRelease; } set { m_toggleToRelease = value; } }

    public UnityEventGrabbable afterGrabbed { get { return m_afterGrabbed; } }

    public UnityEventGrabbable beforeRelease { get { return m_beforeRelease; } }

    public UnityEventGrabbable onDrop { get { return m_onDrop; } }

    public ColliderButtonEventData grabbedEvent { get { return isGrabbed ? currentGrabber.eventData : null; } }

    public ColliderButtonEventData.InputButton grabButton
    {
        get
        {
            return m_grabButton;
        }
        set
        {
            m_grabButton = value;
            MaterialChanger.SetAllChildrenHeighlightButton(gameObject, value);
        }
    }

    private bool moveByVelocity { get { return !unblockableGrab && grabRigidbody != null && !grabRigidbody.isKinematic; } }

    [Obsolete("Use grabRigidbody instead")]
    public Rigidbody rigid { get { return grabRigidbody; } set { grabRigidbody = value; } }

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        MaterialChanger.SetAllChildrenHeighlightButton(gameObject, m_grabButton);
    }
#endif

    protected override void Awake()
    {
        base.Awake();
        MaterialChanger.SetAllChildrenHeighlightButton(gameObject, m_grabButton);

        afterGrabberGrabbed += () => m_afterGrabbed.Invoke(this);
        beforeGrabberReleased += () => m_beforeRelease.Invoke(this);
        onGrabberDrop += () => m_onDrop.Invoke(this);
    }

    protected virtual void OnDisable()
    {
        ClearGrabbers(true);
        ClearEventGrabberSet();
    }

    private void ClearEventGrabberSet()
    {
        if (m_eventGrabberSet == null) { return; }

        for (int i = m_eventGrabberSet.Count - 1; i >= 0; --i)
        {
            Grabber.Release(m_eventGrabberSet.GetValueByIndex(i));
        }

        m_eventGrabberSet.Clear();

        //清理记录
        dictCurPool.Clear();
    }

    public void OnColliderEventPressUp(ColliderButtonEventData eventData)
    {
        if (isUsable)
        {
            if (eventData.button == m_useButton)
            {
                StopUse();
            }
        }
    }

    public virtual void OnColliderEventPressDown(ColliderButtonEventData eventData)
    {
        if (isUsable)
        {
            if (eventData.button == m_useButton)
            {
                Use();
            }
        }

        if (isGrabbable)
        {
            OnColliderEventPressDown_Origin(eventData);
        }
    }

    void Use()
    {
        if(InteractableObjectUsed != null)
            InteractableObjectUsed.Invoke(this);
    }
    void StopUse()
    {
        if (InteractableObjectUsed != null)
            InteractableObjectUnused.Invoke(this);
    }

    public virtual void OnColliderEventPressDown_Origin(ColliderButtonEventData eventData)
    {
        //PS:以下是原版
        if (eventData.button != m_grabButton) { return; }

        Grabber grabber;
        if (m_eventGrabberSet == null || !m_eventGrabberSet.TryGetValue(eventData, out grabber))//抓取
        {
            if (!m_allowMultipleGrabbers)
            {
                ClearGrabbers(false);
                ClearEventGrabberSet();
            }

            grabber = Grabber.Get(eventData);
            var offset = RigidPose.FromToPose(grabber.grabberOrigin, new RigidPose(transform));
            if (alignPosition) { offset.pos = alignPositionOffset; }
            if (alignRotation) { offset.rot = Quaternion.Euler(alignRotationOffset); }
            grabber.grabOffset = offset;
            grabber.grabFrame = Time.frameCount;

            if (m_eventGrabberSet == null)
            { m_eventGrabberSet = new IndexedTable<ColliderButtonEventData, Grabber>(); }
            m_eventGrabberSet.Add(eventData, grabber);

            AddGrabber(grabber);

            //记录事件
            if (!dictCurPool.ContainsKey(grabber))
                dictCurPool.Add(grabber, eventData);
        }
        else if (toggleToRelease)//释放
        {
            ReleaseGrabber(grabber, eventData);
        }
    }

    private void ReleaseGrabber(Grabber grabber, ColliderButtonEventData eventData)
    {
        RemoveGrabber(grabber);
        m_eventGrabberSet.Remove(eventData);
        Grabber.Release(grabber);

        ColliderButtonEventData eventDataOut;
        if (dictCurPool.TryGetValue(grabber, out eventDataOut))
            dictCurPool.Remove(grabber);
    }

    [ContextMenu("ForceStopInteracting")]
    public void ForceStopInteracting()
    {
        StopUse();
        CoroutineManager.StartCoroutineEx(IEForceStopInteracting());
    }

    IEnumerator IEForceStopInteracting()
    {
        RemoveUselessEvent();
        yield return new WaitForFixedUpdate();
        this.enabled = false;//调用相应的OnDisabled
        yield return new WaitForFixedUpdate();
        this.enabled = true;
        yield return null;
    }

    Dictionary<Grabber, ColliderButtonEventData> dictCurPool = new Dictionary<Grabber, ColliderButtonEventData>();
    protected virtual void FixedUpdate()
    {
        if (isGrabbed && moveByVelocity)
        {
            OnGrabRigidbody();
        }
    }

    ColliderButtonEventData tempEventDataOut;
    protected virtual void Update()
    {
        if (!isGrabbed) { return; }

        //Todo:监听对应事件
        //Bug
        //if (dictCurPool.Count > 0)
        //{
        //    foreach (var pair in dictCurPool)
        //    {
        //        //参考ControllerManagerSample
        //        ColliderButtonEventData colliderEventData = pair.Value;
        //        ViveColliderButtonEventData viveEventData;
        //        if (colliderEventData.TryGetViveButtonEventData(out viveEventData))
        //        {
        //            if (ViveInput.GetPressDownEx(viveEventData.viveRole.roleType, viveEventData.viveButton))
        //            {
        //                if (isUsable)
        //                    Use();
        //            }
        //            else if(ViveInput.GetPressUpEx(viveEventData.viveRole.roleType, viveEventData.viveButton))
        //            {
        //                if (isUsable)
        //                    StopUse();
        //            }
        //        }
        //    }
        //}

        if (!moveByVelocity)
        {
            RecordLatestPosesForDrop(Time.time, 0.05f);
            OnGrabTransformEx();
        }

        // check toggle release
        if (toggleToRelease)
        {
            //移除所有符合条件的Grabber
            RemoveUselessEvent();
        }
    }

    //修改
    protected void OnGrabTransformEx()
    {
        var targetPose = currentGrabber.grabberOrigin * currentGrabber.grabOffset;
        ModifyPose(ref targetPose, null);

        if (grabRigidbody != null)
        {
            grabRigidbody.velocity = Vector3.zero;
            grabRigidbody.angularVelocity = Vector3.zero;
        }

        if (isFollowPos)
        {
            transform.position = targetPose.pos;
        }
        if (isFollowRot)
        {
            Transform targetGrabber = grabbedEvent.eventCaster.transform.parent;

            if (isUseLocalRotation)
            {
                transform.localEulerAngles = transform.localEulerAngles.Lerp(targetGrabber.localRotation.eulerAngles, allowRotAxis);
            }
            else
            {
                transform.eulerAngles = transform.eulerAngles.Lerp(targetGrabber.rotation.eulerAngles, allowRotAxis);
            }
        }
    }

    void RemoveUselessEvent()
    {
        m_eventGrabberSet.RemoveAll(
    (pair) =>
    {
        var grabber = pair.Value;
        if (!grabber.eventData.GetPressDown())
        { return false; }

        if (grabber.grabFrame == Time.frameCount)
        { return false; }

        if (!grabber.redeayForRelease)
        {
            RemoveGrabber(grabber);
            grabber.redeayForRelease = true;
            return false;
        }

        Grabber.Release(grabber);
        return true;
    });
    }

#endregion

#region Defines

    //事件说明：
    //参考VRTK
    [Header("自定义设置")]
    public bool isGrabbable = true;
    public bool holdButtonToGrab = true;
    public bool isUsable = false;
    public bool pointerActivatesUseAction = false;//通过Laser调用
    [SerializeField]
    private ColliderButtonEventData.InputButton m_useButton = ColliderButtonEventData.InputButton.Trigger;

    /// <summary>
    /// Event Payload
    /// </summary>
    /// <param name="sender">this object</param>
    /// <param name="e"><see cref="InteractableObjectEventArgs"/></param>
    public delegate void InteractableObjectEventHandler(object sender);

    /// <summary>
    /// Emitted when another object touches the current object.
    /// </summary>
    public event InteractableObjectEventHandler InteractableObjectTouched;
    /// <summary>
    /// Emitted when the other object stops touching the current object.
    /// </summary>
    public event InteractableObjectEventHandler InteractableObjectUntouched;

    //用VIU父类脚本自带的事件代替
    /// <summary>
    /// Emitted when another object grabs the current object (e.g. a controller).
    /// </summary>
    public event InteractableObjectEventHandler InteractableObjectGrabbed
    {
        add { afterGrabberGrabbed += () => value(this); }
        remove { afterGrabberGrabbed -= () => value(this); }
    }
    /// <summary>
    /// Emitted when the other object stops grabbing the current object.
    /// </summary>
    public event InteractableObjectEventHandler InteractableObjectUngrabbed
    {
        add { onGrabberDrop += () => value(this); }
        remove { onGrabberDrop -= () => value(this); }
    }

    public event InteractableObjectEventHandler InteractableObjectHover;
    public event InteractableObjectEventHandler InteractableObjectUnHover;


    /// <summary>
    /// Emitted when another object uses the current object (e.g. a controller).
    /// </summary>
    public event InteractableObjectEventHandler InteractableObjectUsed;
    /// <summary>
    /// Emitted when the other object stops using the current object.
    /// </summary>
    public event InteractableObjectEventHandler InteractableObjectUnused;
    /// <summary>
    /// Emitted when the object enters a snap drop zone.
    /// </summary>
    public event InteractableObjectEventHandler InteractableObjectEnteredSnapDropZone;
    /// <summary>
    /// Emitted when the object exists a snap drop zone.
    /// </summary>
    public event InteractableObjectEventHandler InteractableObjectExitedSnapDropZone;
    /// <summary>
    /// Emitted when the object gets snapped to a drop zone.
    /// </summary>
    public event InteractableObjectEventHandler InteractableObjectSnappedToDropZone;
    /// <summary>
    /// Emitted when the object gets unsnapped from a drop zone.
    /// </summary>
    public event InteractableObjectEventHandler InteractableObjectUnsnappedFromDropZone;

#endregion

#region Func

    static ControllerButton GetReleateButton(ColliderButtonEventData.InputButton inputButton)
    {
        switch (inputButton)
        {
            case ColliderButtonEventData.InputButton.Trigger:
                return ControllerButton.Trigger;
            case ColliderButtonEventData.InputButton.PadOrStick:
                return ControllerButton.Pad;
            case ColliderButtonEventData.InputButton.GripOrHandTrigger:
                return ControllerButton.Grip;
            default:
                Debug.LogError("No Defination!");
                return ControllerButton.Trigger;
        }
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isUsable)
            return;
        if (!pointerActivatesUseAction)
            return;

        if (eventData.IsViveButton(ControllerButton.Trigger))
            Use();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!pointerActivatesUseAction)
            return;

        if (InteractableObjectHover != null)
            InteractableObjectHover.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!pointerActivatesUseAction)
            return;
        if (InteractableObjectUnHover != null)
        InteractableObjectUnHover.Invoke(this);

    }

    public void OnColliderEventHoverEnter(ColliderHoverEventData eventData)
    {
        if (InteractableObjectTouched != null)
            InteractableObjectTouched.Invoke(this);
    }

    public void OnColliderEventHoverExit(ColliderHoverEventData eventData)
    {
        if (InteractableObjectUntouched != null)
            InteractableObjectUntouched.Invoke(this);
    }

#endregion
}
#endif