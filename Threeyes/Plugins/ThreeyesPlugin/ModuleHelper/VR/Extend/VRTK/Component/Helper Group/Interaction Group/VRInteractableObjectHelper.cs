using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

#if USE_VRTK|| USE_VIU
using UnityEngine.Events;
#if USE_VRTK
using VRTK;
#endif
/// <summary>
/// 用于使用了VRTK_InteractableObject的交物体的碰撞判断
/// 
/// 注意！：
/// 1、VRTK_InteractableObject需要RigidBody（如果不移动，可设置为isKinematic），否则容易容易因为休眠而出现不响应交互的问题
/// </summary>
#if USE_VRTK
[RequireComponent(typeof(VRTK_InteractableObject))]
#elif USE_VIU
[RequireComponent(typeof(VIU_InteractableObject))]
#endif
public class VRInteractableObjectHelper : VRColliderEventReceiver
{
    #region Common Param

    public BoolEvent onHoverUnHover;

    public Color touchHighlightColor = Color.clear;
    [Header("触发事件的最小力度")]
    public float targetForce = -1;//碰撞时触发事件的最小力度

    [Header("手柄震动")]
    public bool isVibControllerOnCollisionEnter;//碰撞时震动手柄
    public float virStrength = 0.8f;
    public float duration = -1;
    public float pulseInterval = 0.01f;//震动间隔

    [Header("释放后的配置")]
    public bool isResetKinematicOnUnGrab = false;//UnGrab之后禁用isKinematic，让物体能够受重力的作用而下落

    public bool IsGrabbable
    {
        get
        {
            if (InteractableObject)
                return InteractableObject.isGrabbable;
            return false;
        }
        set
        {
            if (InteractableObject)
                InteractableObject.isGrabbable = value;
        }
    }

    public bool IsPointerActivatesUseAction
    {
        get
        {
            if (InteractableObject)
                return InteractableObject.pointerActivatesUseAction;
            return false;
        }
        set
        {
            if (InteractableObject)
                InteractableObject.pointerActivatesUseAction = value;
        }

    }
    public bool IsUseable
    {
        get
        {
            if (InteractableObject)
                return InteractableObject.isUsable;
            return false;
        }
        set
        {
            if (InteractableObject)
                InteractableObject.isUsable = value;
        }
    }
    public bool IsResetKinematicOnUnGrab
    {
        get
        {
            if (InteractableObject)
                return isResetKinematicOnUnGrab;
            return false;
        }
        set
        {
            if (InteractableObject)
                isResetKinematicOnUnGrab = value;
        }
    }

    Rigidbody rigidbodyThis;

#if USE_VRTK
    VRTK_InteractableObject InteractableObject { get { if (!interactableObject) interactableObject = GetComponent<VRTK_InteractableObject>(); return interactableObject; } }
    VRTK_InteractableObject interactableObject;
    VRTK_ControllerEvents controllerEvents;
#elif USE_VIU
    VIU_InteractableObject InteractableObject
    {
        get
        {
            if (!interactableObject)
                interactableObject = GetComponent<VIU_InteractableObject>();
            if (!interactableObject)
                Debug.LogError(name + " 找不到指定组件！" + typeof(VIU_InteractableObject));
            return interactableObject;
        }
    }
    VIU_InteractableObject interactableObject;
#endif

    #endregion

    #region Common

    #region Public Func

    #region Touch

    readonly Color colorClear = Color.clear;//触碰后不会显示颜色

    /// <summary>
    /// 适用于临时高亮显示颜色，但平时触碰不会显示颜色
    /// </summary>
    /// <param name="isVisible"></param>
    public void SetTouchHighlightColorVisible(bool isVisible)
    {
        if (isVisible)
            SetTouchHighlightColorAndHighLight();
        else
            SetTouchHighlightColorClear();
    }

    public void SetTouchHighlightColorAndHighLight()
    {
        SetTouchHighlightColor(touchHighlightColor);
        ToggleHighlight(true);
    }

    /// <summary>
    /// 透明颜色
    /// </summary>
    public void SetTouchHighlightColorClear()
    {
        ToggleHighlight(false);//注意：不能直接使用Color.Clear，否则VRTK_InteractableObject会判断并剔除掉
        SetTouchHighlightColor(colorClear);
    }

    /// <summary>
    /// 切换高光状态
    /// </summary>
    /// <param name="toggle"></param>
    void ToggleHighlight(bool toggle)
    {
        //暂时统一使用VRTK的HighLight组件
#if VRTK_VERSION_3_1_0_OR_NEWER&&USE_VRTK
        var vrtkInteractableObjectHelper = GetComponent<VRTK.VRTK_InteractableObject>();
        if (vrtkInteractableObjectHelper)
            vrtkInteractableObjectHelper.ToggleHighlight(toggle);
#endif
    }

    void SetTouchHighlightColor(Color color)
    {
#if VRTK_VERSION_3_1_0_OR_NEWER&&USE_VRTK
        var vrtkInteractableObjectHelper = GetComponent<VRTK.VRTK_InteractableObject>();
        if (vrtkInteractableObjectHelper)
            vrtkInteractableObjectHelper.touchHighlightColor = color;
#endif
    }

    #endregion

    /// <summary>
    /// 是否可以随意放下物体，常用于工具使用后才可释放的情况
    /// </summary>
    /// <param name="isEnable"></param>
    public void SetDropAnywhere()
    {
        if (!interactableObject)
            return;
#if USE_VRTK
        InteractableObject.validDrop = VRTK_InteractableObject.ValidDropTypes.DropAnywhere;
#elif USE_VIU
        InteractableObject.isGrabbable = true;
        InteractableObject.toggleToRelease = true;
#endif
    }

    public void ForceStopInteracting()
    {
        if (!interactableObject)
            return;
        InteractableObject.ForceStopInteracting();
    }
    public void EnableHighlightAndInteractive(bool isEnable)
    {
        //Todo:整合两个常用的方法
    }

    public void VibrationController()
    {
        VibrationController(virStrength);
    }

    public void VibrationController(float strength)
    {
#if USE_VRTK
        VRInterface.ViberationFunc(controllerEvents, virStrength, duration, pulseInterval);
#endif
    }

    #endregion


    private void Awake()
    {
        if (!InteractableObject)
        {
            Debug.LogError(name + "不包含 " + InteractableObject.GetType() + " 组件！");
            return;
        }

        //检测物体是否被抓取
        InteractableObject.InteractableObjectGrabbed += OnInteractableObjectGrabbed;
        InteractableObject.InteractableObjectUngrabbed += OnInteractableObjectUnGrabbed;

        rigidbodyThis = GetComponent<Rigidbody>();
        Active(false);//避免频繁检测
    }

    private void OnInteractableObjectGrabbed(object sender
#if USE_VRTK
    , InteractableObjectEventArgs e)
#elif USE_VIU
        )
#endif
    {
#if USE_VRTK
        controllerEvents = e.interactingObject.GetComponentInParent<VRTK_ControllerEvents>();
#endif

        Active(true);
    }

    private void OnInteractableObjectUnGrabbed(object sender
#if USE_VRTK
    , InteractableObjectEventArgs e)
#elif USE_VIU
        )
#endif
    {
#if USE_VRTK
        controllerEvents = null;
#endif

        Active(false);
        if (IsResetKinematicOnUnGrab)
            rigidbodyThis.isKinematic = false;
    }
    public override void OnCollisionEnter(Collision collision)
    {
        if (IsValiable(collision.collider))
            if (isVibControllerOnCollisionEnter)
                VibrationController();

        base.OnCollisionEnter(collision);
    }
    protected override bool IsValiable(Collider other)
    {
        if (targetForce > 0)
            if (rigidbodyThis.velocity.magnitude < targetForce)
                return false;

        return base.IsValiable(other);
    }
    void Active(bool isActive)
    {
        this.enabled = isActive;
    }


    #endregion

#if UNITY_EDITOR

    [MenuItem(EditorDefinition.TopMenuItemPrefix + "Repair all VRInteractableObjectHelper")]
    public static void SetUpRDP()
    {
        List<VRInteractableObjectHelper> listRDP = GameObject.FindObjectsOfType<VRInteractableObjectHelper>().ToList();
        foreach (var rdp in listRDP)
        {
            SetUpTarget(rdp);
        }
    }

    private static void SetUpTarget(VRInteractableObjectHelper vRInteractableObjectHelper)
    {
#if USE_VRTK

#elif USE_VIU
        var viuInteractableObjectHelper = vRInteractableObjectHelper.gameObject.AddComponentOnce<VIU_InteractableObject>();

#if VRTK_VERSION_3_1_0_OR_NEWER&&USE_VRTK
        //复制VRTK的设置
        var vrtkInteractableObjectHelper = vRInteractableObjectHelper.gameObject.GetComponent<VRTK.VRTK_InteractableObject>();
        if (vrtkInteractableObjectHelper)
        {
            viuInteractableObjectHelper.isGrabbable = vrtkInteractableObjectHelper.isGrabbable;
            viuInteractableObjectHelper.isUsable = vrtkInteractableObjectHelper.isUsable;

        }
#endif

#endif
    }

#endif
}
#endif