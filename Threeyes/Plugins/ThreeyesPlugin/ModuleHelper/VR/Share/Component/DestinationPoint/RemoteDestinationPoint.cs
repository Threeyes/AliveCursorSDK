#if USE_OpenXR||USE_VRTK||USE_VIU
#define USE_AnyVRPlugin//使用了任意VR插件
#endif

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;
using Threeyes.Coroutine;


#if USE_OpenXR
using UnityEngine.XR.Interaction.Toolkit;
#endif
#if USE_VRTK
using VRTK;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 可以远程设置 VRRig 的位置，相比默认的VRTK_DestinationPoint，忽略了Raycast
/// 注意：为了避免传送失败，要WayPoint里设置：isShowOnAwake，让该物体和目标物体激活，还需要提前把两点之间的碰撞体去掉
/// 
/// 场景运行时自动传送的路标点的相应设置：
/// 1.isTeleportOnGameStart=true
/// 2.isAlignToDP=true;
/// 5.该路标朝向指定的目标
/// </summary>
public partial class RemoteDestinationPoint :
#if USE_OpenXR
BaseTeleportationInteractable//XRBaseInteractable
#elif USE_VRTK
   VRTK_DestinationPoint
#else
    MonoBehaviour
#endif
{

#if !USE_VRTK //与VRTK父类相同的定义，防止引用丢失和报错

    [Header("参考VRTK的设置")]
    [Tooltip("The GameObject to use to represent the default cursor state.")]
    public GameObject defaultCursorObject;
    [Tooltip("The GameObject to use to represent the hover cursor state.")]
    public GameObject hoverCursorObject;
    [Tooltip("The GameObject to use to represent the locked cursor state.")]
    public GameObject lockedCursorObject;
    public Transform destinationLocation;
    public bool snapToPoint = true;//false适用于Area传送，需要同时开启isModifyTransform
    public bool enableTeleport = true;
    protected virtual Quaternion? GetRotation() { return default(Quaternion); }
    public delegate void DestinationPointEventHandler(object sender);
    public event DestinationPointEventHandler DestinationPointDisabled;
    public static RemoteDestinationPoint currentDestinationPoint;
    protected Collider pointCollider { get { if (!_pointCollider) _pointCollider = GetComponentInChildren<Collider>(); return _pointCollider; } }
    protected Collider _pointCollider;


#if USE_OpenXR
    protected override void OnEnable()
    {
        base.OnEnable();
#else
    void OnEnable()
    {
#endif
        ResetPoint();
    }

    protected virtual void ResetPoint()
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
        }
        else
        {
            pointCollider.enabled = false;
            ToggleObject(lockedCursorObject, true);
            ToggleObject(defaultCursorObject, false);
        }
    }
    protected virtual void EnablePoint()
    {
        ToggleObject(lockedCursorObject, false);
        ToggleObject(defaultCursorObject, false);
        ToggleObject(hoverCursorObject, true);
    }

    protected virtual void DisablePoint()
    {
        if (isStayShow)
            return;

        pointCollider.enabled = false;
        ToggleObject(lockedCursorObject, false);
        ToggleObject(defaultCursorObject, false);
        ToggleObject(hoverCursorObject, false);
        OnDestinationPointDisabled();
    }
    public virtual void OnDestinationPointDisabled()
    {
        if (DestinationPointDisabled != null)
        {
            DestinationPointDisabled(this);
        }
    }

    protected virtual void ToggleObject(GameObject givenObject, bool state)
    {
        if (givenObject != null)
        {
            givenObject.SetActive(state);
        }
    }

#endif

    //全局事件，主要用于WayPoint
    public static RemoteDestinationPoint CurrentDestinationPoint
    {
        get
        {
            return currentDestinationPoint
#if USE_VRTK
                as RemoteDestinationPoint
#endif
                ;
        }
        set { currentDestinationPoint = value; }
    }
    public static UnityAction<RemoteDestinationPoint> actionTeleportFinished;
    public static List<RemoteDestinationPoint> listRDP = new List<RemoteDestinationPoint>();//现存的RDP

    public UnityAction<bool> actionShowHide;
    public UnityAction<bool> actionEnterExit;

    public UnityAction actionBeforeTeleport;//传送前调用
    public UnityAction actionAfterTeleport;//传送完成后调用

    public float LocationHeight//用于Editor编辑高度
    {
        get { return DestinationLocation.localPosition.y; }
        set { DestinationLocation.localPosition = DestinationLocation.localPosition.ChangeY(value); }
    }
    Transform DestinationLocation { get { if (!destinationLocation) destinationLocation = transform; return destinationLocation; } }

    [Header("自定义设置")]
    public bool isTeleportOnGameStart = false;//程序开始时传送到该位置
    public bool isModifyTransform = true;//false适用于玩家自行进入该碰撞体但不需要调整方向的情况
    [Tooltip("设置为true，可以作为VRRig的起始点")]
    public bool isMoveRig = false;//将Rig物体移动到该路标，适用于驾驶等需要跟随的实现
    public bool isAlignToDP = true;//使玩家的旋转值跟跟Dp的旋转值一致，常用于并非垂直于地面的传送点
    [Tooltip("设置为true，可以将传送后的朝向永远指定为正前方")]
    public bool isAlignToForward;//传送朝向指定正前方，实现方式：对齐以前，计算头盔和上一个DP的Z朝向的夹角，对齐以后，将这个夹角补正。
    public Transform tfAlignTarget;//对齐目标，一般是上一个DP。

    [Tooltip("是否传送之后进行二次修正")]
    public DestinationBase destinationBase = DestinationBase.Head;
    public Transform tfNextDPPointerPivot;//指向下一路标的指针
    public bool isUseGlobalHeight = false;//是否使用全局高度

    [Header("Setting from WayPoint")]
    bool isEntered = false;//PS：因为VRTK_DestinationPoint的相应事件会被调用2次，所以需要一个bool值做限定
    public bool IsEntered { get { return isEntered; } set { isEntered = value; } }
    public bool isShowOnAwake = false;//是否在程序运行时显示
    public bool isInvokeShowOnEnter = true;// 是否在玩家进入路标后才会调用onShow(true)事件
    public bool isStayShow = false;//是否持续保持激活状态，适用于无特定路径行走（如展览馆）
    public bool isEnterOnce = true;//是否本次进入该路标之后，就不能再次调用OnEnter的方法


    #region Unity Func

#if UNITY_EDITOR
    void OnValidate()
    {
        if (pointCollider)
            pointCollider.isTrigger =
#if USE_OpenXR//OpenXR只支持非Trigger的碰撞体
        false;
#else
        true;
#endif
    }
#endif

    bool isInit = false;
#if USE_OpenXR || USE_VRTK
    protected override void Awake()
    {
        base.Awake();
#else
    private void Awake()
    {
#endif

        AddListenerFunc();
        actionTeleportFinished += OnTeleportFinished;
        listRDP.Add(this);

        Show(isShowOnAwake);

        if (isTeleportOnGameStart)
        {
#if USE_AnyVRPlugin
            bool isRegistSuccess = VRInterface.RegistOnHMDLoaded(OnHMDDeviceLoad);//等待VRHMD初始化完成才能进行传送
            //避免不使用VRInterface的情况
            if (!isRegistSuccess)
                BeginSetPos(this);
#else
            BeginSetPos(this);
#endif
        }
        isInit = true;
    }


#if USE_OpenXR || USE_VRTK
    protected override void OnDestroy()
    {
        base.OnDestroy();
#else
    void OnDestroy()
    {
#endif
        RemoveListenerFunc();
        actionTeleportFinished -= OnTeleportFinished;
        listRDP.Remove(this);
    }

    void OnHMDDeviceLoad()
    {
        BeginSetPos(this);
        VRInterface.UnRegistOnHMDUnLoaded(OnHMDDeviceLoad);
    }
    #endregion

    #region Teleport


    /// <summary>
    ///  所有主动传送（射线、点击、碰撞体触发等）的统一入口
    /// </summary>
    /// <param name="tfTeleportTo"></param>
    /// <param name="targetPosition"></param>
    /// <param name="actionOnSetPosOverride"></param>
    void DoDestinationMarkerSetFunc(Transform tfTeleportTo, Vector3? targetPosition = null, UnityAction actionOnSetPosOverride = null)
    {
        //PS：以下方法参考VRTK.VRTK_DestinationPoint.DoDestinationMarkerSet
        if (tfTeleportTo == transform)
        {
            if (actionOnSetPosOverride != null)
                actionOnSetPosOverride.Execute();
            else
                SetPos(targetPosition);
        }
        else if (currentDestinationPoint != this)
        {
            ResetPoint();
        }
        else if (currentDestinationPoint != null && tfTeleportTo != currentDestinationPoint.transform)
        {
            currentDestinationPoint = null;
            ResetPoint();
        }
    }


    /// <summary>
    /// 所有代码传送的统一入口
    /// </summary>
    /// <param name="destinationPoint"></param>
    /// <param name="onSuccess"></param>
    public static Coroutine BeginSetPos(RemoteDestinationPoint destinationPoint, Vector3? positionOverride = null, bool isInvokeEvent = true)
    {
        return CoroutineManager.StartCoroutineEx(IESetPos(destinationPoint, positionOverride));
    }

    static IEnumerator IESetPos(RemoteDestinationPoint destinationPoint, Vector3? positionOverride = null, bool isInvokeEvent = true)
    {
        if (destinationPoint.IsShowing)
        {
            destinationPoint.Show();
        }
        ////ToAdd：在VRInterface中加一个等待初始化各VR插件初始化完成的协程，完成后继续下一步
        //yield return VRInterface.IEWaitForInitComplete();

        //等VRTK_DestinationPoint组件激活并初始化完成后，再传送
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        try
        {
            destinationPoint.SetPos(positionOverride, isInvokeEvent);
        }
        catch (Exception e)
        {
            Debug.LogError("传送失败：\r\n" + e);
        }
    }

    [ContextMenu("MovetoThisRDP")]
    public void SetPos()
    {
        SetPos(null);
    }

    public void SetPos(Vector3? positionOverride, bool isInvokeEvent = true)
    {
        RemoteDestinationPointManager managerInst = RemoteDestinationPointManager.Instance;
        if (managerInst)
        {
            if (managerInst.IsCommandMode)
            {
                managerInst.actCommandSetPos.Execute(this, positionOverride, isInvokeEvent);
                return;
            }
            else
            {
                managerInst.actRealSetPos.Execute(this, positionOverride, isInvokeEvent);
            }
        }

        SetPosFunc(positionOverride, isInvokeEvent);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="positionOverride">指定位置，通常是射线与平面的交点</param>
    protected void SetPosFunc(Vector3? positionOverride, bool isInvokeEvent = true)
    {
        if (!this)
            return;

        currentDestinationPoint = this;
        //设置目标高度
        if (isUseGlobalHeight)
        {
            if (DestinationLocation != transform)//自定义了目标点
            {
                if (RemoteDestinationPointManager.Instance)
                    LocationHeight = RemoteDestinationPointManager.Instance.GlobalHeight;
            }
        }

        actionBeforeTeleport.Execute();

        //先设置CameraRig的位置
        if (isModifyTransform)
        {
            if (snapToPoint)//传送到指定位置
            {
                if (DestinationLocation)
                    Teleport(DestinationLocation.position, GetRotation(), DestinationLocation);//不使用协程，直接传送
            }
            else if (positionOverride.HasValue)//传送到射线指向位置
            {
                Teleport(positionOverride.Value, GetRotation(), DestinationLocation);//不使用协程，直接传送
            }
        }

        //根据DestinationBase的设置，进行二次调整
#if USE_VIU
        AlterSetPosFunc(isInvokeEvent);//PS：这个方法里面会延后调用AlterSetPos
#else
        AlterSetPos(isInvokeEvent);
#endif
    }


    /// <summary>
    /// 强制传送到目的点
    /// </summary>
    /// <param name="destinationPosition"></param>
    /// <param name="destinationRotation"></param>
    public static void Teleport(Vector3 destinationPosition, Quaternion? destinationRotation = null, Transform target = null)
    {
        //ToAdd:OpenXR的传送，先看有没必要使用其内置的事件，如果不需要则不实现
#if USE_VRTK
        TeleportFunc(target, destinationPosition, destinationRotation);
#else
        if (VRInterface.tfCameraRig)
            VRInterface.tfCameraRig.position = destinationPosition;
        else
            Debug.LogError("找不到相机父对象！");
#endif
    }


    /// <summary>
    /// 对传送进行修正（如改变朝向），并调用完成事件
    /// </summary>
    void AlterSetPos(bool isInvokeEvent = true)
    {
        if (isModifyTransform)
        {
            //存储头盔和上一个Dp之间的夹角
            Vector3 vt3camRigRot = default(Vector3);
            if (tfAlignTarget != null)
                vt3camRigRot = VRInterface.tfCameraEye.eulerAngles - tfAlignTarget.eulerAngles;

            //等待
            if (isAlignToDP)//对齐朝向轴
            {
                VRInterface.tfCameraRig.eulerAngles = DestinationLocation.eulerAngles;//先保证旋转一致
                float camEyeYRot = VRInterface.tfCameraEye.localEulerAngles.y;
                VRInterface.tfCameraRig.Rotate(0, -camEyeYRot, 0, Space.Self);
            }

            if (isAlignToForward)
            {
                if (tfAlignTarget == null)
                {
                    Debug.Log("对齐目标为空，无法对齐。");
                }
                VRInterface.tfCameraRig.eulerAngles = new Vector3(VRInterface.tfCameraRig.eulerAngles.x,
                    VRInterface.tfCameraRig.eulerAngles.y + vt3camRigRot.y, VRInterface.tfCameraRig.eulerAngles.z);//头盔父物体补正夹角
            }

            if (destinationBase == DestinationBase.Head)//让头与目标点位于同一高度
            {
                Vector3 eyeOffset = VRInterface.tfCameraRig.position - VRInterface.tfCameraEye.position;

                if (snapToPoint)//完全匹配
                {
                    VRInterface.tfCameraRig.position += eyeOffset;//计算位移值
                }
                else//只适配高度
                {
                    Vector3 curPos = VRInterface.tfCameraRig.position;
                    curPos.y = LocationHeight + eyeOffset.y;//世界坐标高度
                    VRInterface.tfCameraRig.position = curPos;

                }
            }
            else if (destinationBase == DestinationBase.Foot)//让脚位于目标点正中
            {
                //与Head类似，只是Foot的位移只影响XZ轴
                Vector3 eyeOffset = VRInterface.tfCameraRig.position - VRInterface.tfCameraEye.position;
                Vector3 projection = Vector3.ProjectOnPlane(eyeOffset, VRInterface.tfCameraRig.up);//XZ轴的位移
                VRInterface.tfCameraRig.position += projection;//计算位移值
            }

            if (isMoveRig)
            {
                VRInterface.Instance.SetCameraRigParent(DestinationLocation);//
            }
        }
        else
        {
            Debug.Log("不更改VR的位置!");
        }

        if (snapToPoint)
            DisablePoint();

        if (this)//检查是否为空，避免切换场景导致物体销毁
        {
            if (!isInvokeEvent)
                isActiveEnterExitEvent = false;//临时禁用
            actionTeleportFinished.Execute(this);//传送完成（会调用所有RDP的OnTeleportFinished）
            if (!isInvokeEvent)
                isActiveEnterExitEvent = true;//恢复

            actionAfterTeleport.Execute();
        }
    }

    bool isActiveEnterExitEvent = true;//是否允许调用Enter/Exit事件（仅针对自身，常用于静默传送）
    void OnTeleportFinished(RemoteDestinationPoint sender)
    {
        if (this == sender)//是当前的DP
        {
            if (isEnterOnce & IsEntered)//已经执行过进入事件
                return;
        }
        else//不是当前DP：调用退出
        {
            if (IsEntered)//之前进入的DP：
            {
                if (isActiveEnterExitEvent)
                {
                    actionEnterExit.Execute(false);
                }
                Show(false);//隐藏该DP

                IsEntered = false;
            }
            return;
        }

        if (isInvokeShowOnEnter)
            actionShowHide.Execute(true);

        if (isActiveEnterExitEvent)
        {
            actionEnterExit.Execute(true);
        }

        IsEntered = true;
    }

    /// <summary>
    /// 设置对齐目标
    /// </summary>
    /// <param name="AlignTarget"></param>
    public void SettfAlignTarget(Transform AlignTarget)
    {
        tfAlignTarget = AlignTarget;
    }

    /// <summary>
    /// 设置路标指向器的朝向
    /// </summary>
    /// <param name="isShow"></param>
    /// <param name="targetToPointAt"></param>
    public void SetupNextDPPointer(bool isShow, Transform targetToPointAt = null)
    {
        if (tfNextDPPointerPivot)
        {
            tfNextDPPointerPivot.gameObject.SetActive(isShow);
            tfNextDPPointerPivot.LookAt(targetToPointAt);
        }
    }

    #region RemoteDestinationPoint_Common partial Class(放在这里方便使用宏定义
#if !USE_AnyVRPlugin

    /// <summary>
    /// 参考VIU做的结构
    /// </summary>
    public class Pointer3DEventData
    {
        public Ray ray;
        public Vector3 position;
    }

#pragma warning disable CS0067
    public static event UnityAction<Transform, Pointer3DEventData> DestinationMarkerSet;
#pragma warning restore CS0067

    //ToDo:针对无VR版本，做一个射线传送的方法
    private void AddListenerFunc()
    {
        DestinationMarkerSet += DoDestinationMarkerSet;
    }

    private void RemoveListenerFunc()
    {
        DestinationMarkerSet -= DoDestinationMarkerSet;
    }

    /// <summary>
    /// 类似其他VR一样，需要手动调用
    /// </summary>
    public void ManualTeleport()
    {
        DoDestinationMarkerSet(DestinationLocation, new Pointer3DEventData());
    }
    void DoDestinationMarkerSet(Transform tfTeleportTo, Pointer3DEventData eventData)//类似VRTK的DoDestinationMarkerSet
    {
        DoDestinationMarkerSetFunc(tfTeleportTo);
    }

#endif
    #endregion

    #endregion

    #region Override IShowHideInterface

    public bool IsShowing { get { return isShowing; } set { isShowing = value; } }
    protected bool isShowing = false;

    public void Show()
    {
        Show(true);
    }
    public void Hide()
    {
        Show(false);
    }
    public void ToggleShow()
    {
        Show(!IsShowing);
    }
    public void Show(bool isShow)
    {
        if (isStayShow)
        {
            if (!isShow)
                return;
        }
        IsShowing = isShow;
        ShowFunc(isShow);
    }
    protected virtual void ShowFunc(bool isShow)
    {
        if (!this)
            return;
        gameObject.SetActive(isShow);
        if (!isInvokeShowOnEnter && isInit)//除此（不允许在初始化时调用Show事件）之外
        {
            actionShowHide.Execute(isShow);
        }

        SetupNextDPPointer(false);//统一隐藏DP指针
    }

    #endregion

    #region EDITOR

#if UNITY_EDITOR

#if USE_OpenXR || USE_VIU || USE_VRTK

    #region Editor
#if USE_OpenXR
    protected override void Reset()
    {
        base.Reset();
#else
    private void Reset()
    {
#endif
        TryCopyValue();
    }

    //ToMove:移动到VRTK
    [ContextMenu("CopyValue")]
    public void TryCopyValue()
    {
#if VRTK_VERSION_3_1_0_OR_NEWER && USE_VRTK
        VRTK.VRTK_DestinationPoint destinationPoint = GetComponent<VRTK.VRTK_DestinationPoint>();
        if (!destinationPoint)
            return;
#if USE_VRTK
        this.targetListPolicy = destinationPoint.targetListPolicy;
        this.hidePointerCursorOnHover = destinationPoint.hidePointerCursorOnHover;
        this.hideDirectionIndicatorOnHover = destinationPoint.hideDirectionIndicatorOnHover;
        this.snapToRotation = destinationPoint.snapToRotation;
#endif

        this.enableTeleport = destinationPoint.enableTeleport;
        this.defaultCursorObject = destinationPoint.defaultCursorObject;
        this.hoverCursorObject = destinationPoint.hoverCursorObject;
        this.lockedCursorObject = destinationPoint.lockedCursorObject;
        this.destinationLocation = destinationPoint.destinationLocation;
        this.snapToPoint = destinationPoint.snapToPoint;
        DestroyImmediate(destinationPoint);
#endif
    }

    #endregion
    [MenuItem(EditorDefinition.TopMenuItemPrefix + "RepairReference for all RemoteDestinationPoint")]
    public static void RepairAllReference()
    {
        List<RemoteDestinationPoint> listRDP = GameObject.FindObjectsOfType<RemoteDestinationPoint>().ToList();
        foreach (var rdp in listRDP)
        {
            rdp.RepairReference();
        }
    }

    /// <summary>
    /// 修复引用丢失的问题
    /// </summary>
    [ContextMenu("RepairReference")]
    public void RepairReference()
    {
        if (!this.defaultCursorObject)
            this.defaultCursorObject = this.transform.Find("defaultCursor").gameObject;
        if (!this.hoverCursorObject)
            this.hoverCursorObject = this.transform.Find("hoverCursor").gameObject;
        if (!this.lockedCursorObject)
            this.lockedCursorObject = this.transform.Find("lockedCUrsor").gameObject;
        EditorUtility.SetDirty(this);
    }

    /// <summary>
    /// 为所有RemoteDestinationPoint设置参数
    /// </summary>
    [MenuItem(EditorDefinition.TopMenuItemPrefix + "Improve all RemoteDestinationPoint")]
    public static void SetUpRDP()
    {
        List<RemoteDestinationPoint> listRDP = GameObject.FindObjectsOfType<RemoteDestinationPoint>().ToList();
        foreach (var rdp in listRDP)
        {
            SetUpTarget(rdp);
        }
    }

    [ContextMenu("SetToArea")]
    public void SetToArea()
    {
        //PS:区域传送时，高度不变，因此要在程序开始时就设置好高度
        snapToPoint = false;
        isModifyTransform = true;
        isAlignToDP = false;
        destinationBase = DestinationBase.Foot;

    }
    [ContextMenu("TeleportOnGameStartSolo")]
    public void TeleportOnGameStartSolo()
    {
        List<RemoteDestinationPoint> listrdp = GameObject.FindObjectsOfType<RemoteDestinationPoint>().ToList();
        foreach (var rdp in listrdp)
        {
            bool isCurSelect = rdp == this;
            rdp.isTeleportOnGameStart = isCurSelect ? true : false;

            WayPoint wayPoint = rdp.GetComponent<WayPoint>();
            if (wayPoint)
            {
                isShowOnAwake = isCurSelect;
                EditorUtility.SetDirty(wayPoint);
            }

            EditorUtility.SetDirty(rdp);
        }
    }

    [ContextMenu("SetUpSelf")]
    public void SetUpSelf()
    {
        SetUpTarget(this);
    }

    private static void SetUpTarget(RemoteDestinationPoint remoteDestinationPoint)
    {
        remoteDestinationPoint.destinationBase = DestinationBase.Head;
        remoteDestinationPoint.isAlignToDP = true;
        if (!remoteDestinationPoint.destinationLocation || remoteDestinationPoint.destinationLocation == remoteDestinationPoint.transform)
        {
            GameObject go = null;
            Transform target = remoteDestinationPoint.transform.Find("Head Pivot");
            if (target)
                go = target.gameObject;
            else
            {
                go = new GameObject("Head Pivot");
                go.transform.SetParent(remoteDestinationPoint.transform);
                go.transform.localPosition = new Vector3(0, 1.7f, 0);//设置默认高度
                go.transform.localRotation = default(Quaternion);
            }
            remoteDestinationPoint.destinationLocation = go.transform;

            EditorUtility.SetDirty(remoteDestinationPoint);
            Debug.Log("为" + remoteDestinationPoint.name + "设置头朝向点");
        }
        else
        {
            remoteDestinationPoint.destinationLocation.transform.localPosition = new Vector3(0, 1.2f, 0);
            EditorUtility.SetDirty(remoteDestinationPoint);
        }

    }

    [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NonSelected)]
    static void MyGizmo(RemoteDestinationPoint destinationPoint, GizmoType gizmoType)  //参数1为“XX”组件，可以随意选，参数2 必须写，不用赋值  
    {
        //绘制destinationLocation的状态
        if (destinationPoint.destinationLocation)
        {
            Transform tfDL = destinationPoint.destinationLocation;
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(tfDL.position, 0.05f);
            EditorDrawArrow.ForGizmo(tfDL.position, tfDL.forward * 0.3f, 0.1f);//自身朝向 
        }
    }
#endif
#endif

    #endregion

    [Serializable]
    /// <summary>
    /// 目标点对应玩家身体的位置
    /// </summary>
    public enum DestinationBase
    {
        Default,//PlayArea的位置，默认值
        Head,//头部的位置
        Foot//脚部的位置
    }

}