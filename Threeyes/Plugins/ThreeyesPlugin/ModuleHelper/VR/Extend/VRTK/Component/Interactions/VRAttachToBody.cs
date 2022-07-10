using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
#if USE_VRTK
using VRTK;
using VRTK.GrabAttachMechanics;
using VRTK.Highlighters;
#endif

/// <summary>
/// 将物体附着在VR身体的某个部位，作为相应碰撞器的子物体
/// PS：可以通过其他方式实现，如隐藏当前物体，显示头盔上的同种物体
/// VRTK建议增加的组件：
/// ——VRTK_ChildOfControllerGrabAttach
/// ——VRTK_OutlineObjectCopyHighlighter
/// </summary>

#if USE_VRTK
//[RequireComponent(typeof(VRTK_InteractableObject))]
#endif

public class VRAttachToBody : GameObjectBase
{
    public VRColliderCheckType colliderCheckType = VRColliderCheckType.VRBody_Head;//检测类型

    public UnityEvent onAttach;

    public Vector3 attachLocalPos = new Vector3(0, 0.1f, 0.15f);
    public Vector3 attachLocalRot;

    public bool IsActive { get { return isActive; } set { isActive = value; } }
    [SerializeField]
    protected bool isActive = true;

    #region VRTK
    public bool isRemoveCompAfterAttach = true;

#if USE_VRTK

    VRTK_InteractableObject interactableObject;
    protected void Awake()
    {
        interactableObject = GetComponent<VRTK_InteractableObject>();
        interactableObject.InteractableObjectUsed += InteractablseObject_InteractableObjectUsed;
    }

    private void InteractablseObject_InteractableObjectUsed(object sender, InteractableObjectEventArgs e)
    {
        if (interactableObject.isUsable)
            Attach();
    }
#endif
    #endregion

    bool isAttached = false;
    VRColliderCheckType attachedToType = VRColliderCheckType.VRBody_Head;//当前Attach的目标

    private void OnTriggerEnter(Collider other)
    {
        //避免频繁触发导致程序卡顿
        if (VRInterface.IsVRPlayerHead(other) &&
            ((colliderCheckType == VRColliderCheckType.VRWholeBody) || (colliderCheckType == VRColliderCheckType.VRBody_Head)))
        {
            AttachToTarget(colliderCheckType);
            attachedToType = VRColliderCheckType.VRBody_Head;
        }

        if (VRInterface.IsVRPlayerLowerBody(other) &&
            ((colliderCheckType == VRColliderCheckType.VRWholeBody) || (colliderCheckType == VRColliderCheckType.VRBody_LowerBody)))
        {
            AttachToTarget(colliderCheckType, other.transform);
            attachedToType = VRColliderCheckType.VRBody_LowerBody;
        }
    }


    public void Attach()
    {
        AttachToTarget(colliderCheckType);
    }

    public void AttachToHead()
    {
        AttachToTarget(VRColliderCheckType.VRBody_Head);
    }

    void AttachToTarget(VRColliderCheckType colliderCheckType, Transform tfOptional = null)
    {

        if (!IsActive)
            return;

        if (isAttached)
            return;

        isAttached = true;

#if USE_VRTK
        if (interactableObject)
            interactableObject.Ungrabbed();//释放该物体，否则Controller会一直持有该物体导致不能使用其他物体
        //interactableObject.ForceStopInteracting();
        //interactableObject.isGrabbable = false;
        if (isRemoveCompAfterAttach)
        {
            //Destroy(interactableObject);
            if (GetComponent<VRTK_BaseGrabAttach>())
            {
                Destroy(GetComponent<VRTK_BaseGrabAttach>());
            }
            if (GetComponent<VRTK_BaseHighlighter>())
            {
                Destroy(GetComponent<VRTK_BaseHighlighter>());
            }
            tfThis.ForEachChildComponent<Collider>(col => Destroy(col), includeSelf: true);
            if (tfThis.GetComponent<Rigidbody>())
            {
                Destroy(tfThis.GetComponent<Rigidbody>());
            }
        }
#endif

        onAttach.Invoke();

        Transform tfAttachTo = null;
        switch (colliderCheckType)
        {
            case VRColliderCheckType.VRBody_Head:
                tfAttachTo = VRInterface.tfCameraEye; break;
            case VRColliderCheckType.VRBody_LowerBody:
                if (!tfOptional)
                    Debug.LogError("空对象！");
                tfAttachTo = tfOptional;
                break;
            case VRColliderCheckType.VRWholeBody:
                tfAttachTo = tfOptional ? tfOptional : VRInterface.tfCameraEye; break;// 自动附着

        }

        tfThis.SetParent(tfAttachTo);
        tfThis.localPosition = attachLocalPos;
        tfThis.localEulerAngles = attachLocalRot;
    }

    //Debug
    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.G))
        {
            print("Attach!");
            AttachToTarget(colliderCheckType);
        }
#endif

        //PS:如果Attach到身体，那么Transform不会动，Collider会变，要及时同步位置
        if (isAttached && attachedToType == VRColliderCheckType.VRBody_LowerBody)
        {
            Transform tfParent = transform.parent;
            CapsuleCollider collider = tfParent.GetComponent<CapsuleCollider>();
            Vector3 thisCurPos = tfParent.TransformPoint(collider.center + attachLocalPos);
            tfThis.position = thisCurPos;
            tfThis.localEulerAngles = attachLocalRot;
        }
    }

    //#endif
}