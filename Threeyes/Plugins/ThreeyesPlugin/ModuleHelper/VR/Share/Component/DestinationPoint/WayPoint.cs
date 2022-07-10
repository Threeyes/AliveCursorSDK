using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
//using Threeyes.EventPlayer;
using Threeyes.Coroutine;
using System.Linq;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// VR 路标/传送点
/// 负责：连接路标点。这个类主要处理WayPoint之间的关系，以及监听RDP事件
/// PS：
/// VRTK_DestinationPoint的坑：
/// 1.DestinationPointDisabled会调用两次，因此需要用isEnter做判断
/// 2.不勾选Snap to Point后，DoDestinationMarkerSet方法中不会调用相应的事件
/// </summary>
[RequireComponent(typeof(RemoteDestinationPoint))]
public class WayPoint : MonoBehaviour
{
    #region Networking

    public bool IsCommandMode = false;//Set to true to invoke actCommand instead
    public UnityAction<bool> actCommandPlay;
    public UnityAction<bool> actCommandSetIsActive;

    #endregion

    #region Property & Field

    public static string defaultName = "DP ";
    public static WayPoint CurrentWayPoint;//当前已经传送到的路标点

    public bool IsEntered { get { return Comp.IsEntered; } }
    public bool IsActive { get { return Comp.enableTeleport; } set { Comp.enableTeleport = value; } }//激活该场景，常用于编辑器调用
    public bool IsShowing { get { return Comp.IsShowing; } }


    [Header("Next")]
    public float delayTimeActiveNextWayPoint = 0;//进入该路标后，延迟多少秒才激活下个路标( <0不激活，=0立即激活 >0延迟激活）
    public List<WayPoint> listNextWayPoint = new List<WayPoint>();//随后的路标
    public WayPoint FirstNextWayPoint { get { return listNextWayPoint.FirstOrDefault(); } }

    [Header("Config")]
    [SerializeField] protected RemoteDestinationPoint comp;
    public RemoteDestinationPoint Comp { get { if (!comp) comp = GetComponent<RemoteDestinationPoint>(); return comp; } }




    [Header("Event")]
    public Toggle.ToggleEvent onShow;//显示/隐藏路标点的时候会调用，Awake会调用一次，此时可用于隐藏指定物体。常用于UI、高亮Mesh、标识点等提示。
    public Toggle.ToggleEvent onEnterExit;//进入、退出路标后的事件 (PS:onExit需要下一个路标调用！！！因此没有下一个路标的，可以用onShow代替）
    public UnityEvent onEnter;//进入路标后的事件
    public UnityEvent onExit;//退出路标后的事件
    public UnityEvent onFinishMission;//完成任务后的事件
    public FloatEvent onPlayAudioFinish;


    //——Obsolete——
    //Obsolete: 以下字段已经全部转移到RemoteDestinationPoint中。
    const string strObsoleteField = "Use same field in RemoteDestinationPoint Instead";
    [Header("以下4个字段已经过期！使用RemoteDestinationPoint的同名字段代替！")]
    /* [System.Obsolete(strObsoleteField)] [HideInInspector]*/
    public bool isShowOnAwake = false;//是否在程序运行时显示                                      、
    /* [System.Obsolete(strObsoleteField)] [HideInInspector]*/
    public bool isInvokeShowOnEnter = true;// 是否在玩家进入路标后才会调用onShow(true)事件                                          
    /* [System.Obsolete(strObsoleteField)] [HideInInspector]*/
    public bool isStayShow = false;//是否持续保持激活状态，适用于无特定路径行走（如展览馆）                                
    /* [System.Obsolete(strObsoleteField)] [HideInInspector]*/
    public bool isEnterOnce = true;//是否本次进入该路标之后，就不能再次调用OnEnter的方法

    //#Obsolete：改为EP，减少耦合
    public bool isSetTips = false;//是否设置提示信息（初始化路标如果有其他提示信息，可以设置为false。 PS：即使tipsEnter为空也会调用一次，主要是为了停止上一路标语音，所以需要预防Bug）
    public SOTipsInfo tipsEnter;//进入路标后提示信息


    public bool hasVersionUpdated = false;
#if UNITY_EDITOR
    void OnValidate()
    {
        if (hasVersionUpdated)
            return;
#pragma warning disable CS0618 
        Threeyes.Editor.EditorVersionUpdateTool.TransferField(this, ref isShowOnAwake, ref Comp.isShowOnAwake);
        Threeyes.Editor.EditorVersionUpdateTool.TransferField(this, ref isInvokeShowOnEnter, ref Comp.isInvokeShowOnEnter);
        Threeyes.Editor.EditorVersionUpdateTool.TransferField(this, ref isStayShow, ref Comp.isStayShow);
        Threeyes.Editor.EditorVersionUpdateTool.TransferField(this, ref isEnterOnce, ref Comp.isEnterOnce);
#pragma warning restore CS0618
        hasVersionUpdated = true;
    }
#endif

    #endregion

    #region Init

    void Awake()
    {
        //监听事件(RDP是主要作用方，WayPont只起到管理事件的作用
        Comp.actionShowHide += OnShow;
        Comp.actionEnterExit += OnEnter;
    }

    #endregion


    #region Callback

    void OnShow(bool isShow)
    {
        onShow.Invoke(isShow);
    }

    void OnEnter(bool isEnter)
    {
        //执行进入事件
        UpdateVRHeadTips(tipsEnter);

        if (isEnter)
            onEnter.Invoke();
        else
            onExit.Invoke();
        onEnterExit.Invoke(isEnter);

        //等待触发事件完成
        if (delayTimeActiveNextWayPoint == 0)//=0 立即完成任务
            FinishMission();
        else if (delayTimeActiveNextWayPoint > 0)//>0 延迟完成
        {
            DelayFinishMission(delayTimeActiveNextWayPoint);
        }
        else//<0 需要外部调用FinishMission
        {
        }

        CurrentWayPoint = this;
    }

    void UpdateVRHeadTips(SOTipsInfo tips)
    {
        if (isSetTips && UIVRHeadCanvas.Instance)
            UIVRHeadCanvas.Instance.SetTips(tips, (f) => onPlayAudioFinish.Invoke(f));
    }

    #endregion

    #region Mission (Obsolete, 用EPG代替）

    /// <summary>
    /// 延迟X秒，等待特定任务完成
    /// </summary>
    /// <param name="delayTime"></param>
    public void DelayFinishMission(float delayTime)
    {
        StartCoroutine(IEDelayFinishMission(delayTime));
    }
    IEnumerator IEDelayFinishMission(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        FinishMission();
    }

    /// <summary>
    /// 主动激活下一个目标。可以通过其他组件触发
    /// </summary>
    [ContextMenu("FinishMission")]
    public void FinishMission()
    {
        //ActiveNextWayPoint
        listNextWayPoint.ForEach(
            (wp) =>
            {
                if (wp)
                    wp.Show();
            });

        if (FirstNextWayPoint)
        {
            Comp.SetupNextDPPointer(true, FirstNextWayPoint.transform);
        }

        onFinishMission.Invoke();
    }

    #endregion

    #region Show/Hide

    /// <summary>
    /// 控制Comp的show/hide
    /// </summary>
    [ContextMenu("Show")]
    public void Show()
    {
        Show(true);
    }
    public void Show(bool isShow)
    {
        Comp.Show(isShow);
    }

    #endregion

    #region Teleport

    /// <summary>
    /// 强制移动到下个路标
    /// </summary>
    [ContextMenu("MovetoNextWayPoint")]
    public void MovetoNextWayPoint()
    {
        if (listNextWayPoint.Count > 0)
        {
            WayPoint nextWayPoint = listNextWayPoint[0];
            if (nextWayPoint)
            {
                MoveTo(nextWayPoint);
            }
        }
    }
    /// <summary>
    /// 如果下个路标已经激活，则移动到下个路标中
    /// </summary>
    public void TryMovetoNextWayPoint()
    {
        if (listNextWayPoint.Count > 0)
        {
            WayPoint nextWayPoint = listNextWayPoint[0];
            if (nextWayPoint.IsShowing)
            {
                MoveTo(nextWayPoint);
            }
        }
    }

    [ContextMenu("MovetoThis")]
    public void MovetoThis()
    {
        MoveTo(this);
    }
    public void MovetoThis(bool isInvokeEvent)
    {
        MoveTo(this, isInvokeEvent);
    }


    static Coroutine coroutineTeleport;//当前缓存的传送协程
    /// <summary>
    /// 统一通过此处调用，便于管理
    /// </summary>
    /// <param name="wayPoint"></param>
    /// <param name="isInvokeEvent"></param>
    public static void MoveTo(WayPoint wayPoint, bool isInvokeEvent = true)
    {
        //避免多个传送线程
        if (coroutineTeleport.NotNull())
        {
            CoroutineManager.StopCoroutineEx(coroutineTeleport);
        }
        coroutineTeleport = CoroutineManager.StartCoroutineEx(IEMoveTo(wayPoint, isInvokeEvent));
    }

    static IEnumerator IEMoveTo(WayPoint wayPoint, bool isInvokeEvent = true)
    {
        yield return RemoteDestinationPoint.BeginSetPos(wayPoint.Comp, isInvokeEvent: isInvokeEvent);
    }

    #endregion

    #region Editor
#if UNITY_EDITOR

    private void Update()
    {
        if (CurrentWayPoint != this)
            return;

        //结束当前路标的任务
        if (Input.GetKeyDown(KeyCode.F))
            FinishMission();

        //跳到下一个路标
        if (Input.GetKeyDown(KeyCode.V))
        {
            MovetoNextWayPoint();
        }
    }

    [ContextMenu("AutoSetEPG")]
    public void AutoSetEPG()
    {
        AutoSet(false);
    }

    [ContextMenu("AutoSetEPGAndTips")]
    public void AutoSetEPGAndTips()
    {
        try
        {
            AutoSet(true);//第一次会因为EventPlayer刚生成，UnityEvent为空而出错。调用2次
            //AutoSet(true);
        }
        catch
        {

        }
    }

    public void AutoSet(bool isSetTips = false)
    {
        Transform tfEPGroup = transform.AddChildOnce("EP Group");
        Transform tfEPGEnterExit = tfEPGroup.AddChildOnce("EPG OnEnterExit");
        //EventPlayer epEnterExit = tfEPGEnterExit.AddComponentOnce<EventPlayer>();
        //epEnterExit.IsGroup = true;
        //epEnterExit.RegisterPersistentListenerOnce(onEnterExit);

        if (isSetTips)
        {
            UITips uITips = transform.FindFirstComponentInChild<UITips>();
            if (uITips)
            {
                Transform tfEPShowTips = tfEPGEnterExit.AddChildOnce("EP Show&Hide Tips");
                //EventPlayer epTips = tfEPShowTips.AddComponentOnce<EventPlayer>();
                //bool isAddSuccess = epTips.onPlayStop.AddPersistentListenerOnce(uITips, "ShowAndPlay", new System.Type[1] { typeof(bool) });
                //if (isAddSuccess)
                //    UnityEditor.Selection.activeGameObject = tfEPShowTips.gameObject;
                //else
                {
                    UnityEditor.Selection.activeGameObject = tfEPShowTips.gameObject;
                    UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
                    UnityEditor.Selection.activeGameObject = transform.gameObject;
                }
            }
        }
    }

    [ContextMenu("TeleportOnThisAtStart")]
    public void TeleportOnThisAtStart()
    {
        WayPoint[] arrWP = GameObject.FindObjectsOfType<WayPoint>();
        foreach (WayPoint wayPoint in arrWP)
        {
            bool isActiveState = wayPoint == this;
            wayPoint.isShowOnAwake = isActiveState;
            if (wayPoint.GetComponent<RemoteDestinationPoint>())
            {
                wayPoint.GetComponent<RemoteDestinationPoint>().isTeleportOnGameStart = isActiveState;
            }
        }
    }



    static Vector3 deltaGizmo = new Vector3(0, 0.1f, 0);//偏移值
    [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NonSelected)]
    static void MyGizmo(WayPoint wayPoint, GizmoType gizmoType)  //参数1为“XX”组件，可以随意选，参数2 必须写，不用赋值  
    {
        if (!wayPoint.Comp)
        {
            Debug.LogWarning("请将 " + wayPoint.name + " 的传送组件更换为RemoteDestinationPoint！");
            return;
        }
        if (wayPoint.Comp.isTeleportOnGameStart)
        {
            Gizmos.color = Color.blue;   //绘制时颜色  
            Gizmos.DrawWireCube(wayPoint.transform.position, Vector3.one * 0.5f);
        }

        if (CurrentWayPoint == wayPoint)
        {
            Gizmos.color = Color.green;   //绘制时颜色  
            Gizmos.DrawWireSphere(wayPoint.transform.position, 0.5f);
        }

        Gizmos.color = Color.blue;
        EditorDrawArrow.ForGizmo(wayPoint.transform.position + deltaGizmo, wayPoint.transform.forward * 0.5f);//自身朝向 

        //指向下一个路标
        foreach (WayPoint wp in wayPoint.listNextWayPoint)
        {
            if (!wp)
                continue;

            if ((wp.transform.position - wayPoint.transform.position) == Vector3.zero)
                continue;

            Gizmos.color = Color.green;
            EditorDrawArrow.ForGizmo(wayPoint.transform.position + deltaGizmo, (wp.transform.position - wayPoint.transform.position) / 2);//指向下个路标点
            EditorDrawArrow.ForGizmo(wayPoint.transform.position + deltaGizmo, wp.transform.position - wayPoint.transform.position);//指向下个路标点
        }
    }

#endif
    #endregion
}
