using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if USE_LeanPool
using Lean.Pool;
#endif

/// <summary>
/// 控制人的动作
/// </summary>
public class People : GameObjectBase
{
    public static readonly int maxAudioCount = 3;//允许同时发出的最大声音数
    public static int curAudioCount = 0;

    public UnityEvent onMoveBegin;
    public UnityEvent onMoveFinish;
    public UnityEvent onDespawn;
    public ThirdPersonCharacterEx thirdPersonCharacter;
    public AudioSource audioSource;
    public Animator m_Animator;

    public float surviveTime = 20f;
    public float finishDistance = 0.5f;

    public PeopleState state = new PeopleState();
    public PeopleStateRunTime stateRT = new PeopleStateRunTime();
    protected void Awake()
    {
        m_Animator = GetComponent<Animator>();
        thirdPersonCharacter = GetComponent<ThirdPersonCharacterEx>();
        audioSource = GetComponent<AudioSource>();
    }

    public void SetState(PeopleState peopleState)
    {
        state = peopleState;
    }
    /// <summary>
    /// 沿着WayPoint进行移动
    /// </summary>
    /// <param name="wayPoint"></param>
    public void MoveAlong(SimpleWaypoint wayPoint)
    {
        stateRT.wayPoint = wayPoint;
        stateRT.isMoveAlongWayPoint = true;

        MoveTo(stateRT.wayPoint.transform);
    }

    void MoveTo(Transform targetPoint)
    {
        stateRT.targetPoint = targetPoint;
        stateRT.isFinishMoving = false;
        stateRT.isMoveToTarget = true;
        onMoveBegin.Invoke();

        if (state.isAutoDespawn)
            Invoke("Despawn", surviveTime);//超时自动销毁
    }

    private Vector3 m_MoveDir;
    private void FixedUpdate()
    {
        if (stateRT.isFinishMoving)
        {
            thirdPersonCharacter.Stand();
            return;
        }

        m_MoveDir = Vector3.zero;
        if (stateRT.isMoveToTarget)//开始移动
        {
            //到达目标点
            if (Vector3.Distance(tfThis.position, stateRT.targetPoint.position) <= finishDistance)
            {
                if (stateRT.isMoveAlongWayPoint)//跟随WayPoint移动
                {
                    stateRT.wayPoint = stateRT.wayPoint.nextWayPoint;//转到下一路标点
                    if (stateRT.wayPoint)
                    {
                        stateRT.targetPoint = stateRT.wayPoint.transform;
                    }
                    else//到达终点：结束.
                    {
                        FinishMoving();
                        return;
                    }
                }
                else
                {
                    FinishMoving();
                    return;
                }
            }
            else//执行移动
            {
                m_MoveDir = (stateRT.targetPoint.position - tfThis.position).normalized * state.MoveSpeed;
            }
        }

        //ps:比较消耗性能
        thirdPersonCharacter.Move(m_MoveDir, state.isCrouch, false);//持续更新动画，避免角色出错
    }

    void FinishMoving()
    {
        onMoveFinish.Invoke();
        stateRT.isFinishMoving = true;
        //thirdPersonCharacter.Stand();
        //thirdPersonCharacter.Move(default(Vector3), state.isCrouch, false);//持续更新动画，避免角色出错

        if (state.isAutoDespawn)
        {
            //取消自动销毁
            if (IsInvoking("Despawn"))
                CancelInvoke("Despawn");
            Despawn();
        }
    }

    public static People Spawn(GameObject original, Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject inst;
        
#if USE_LeanPool
        inst = LeanPool.Spawn(original, position, rotation, null);//因为LeanPool的原因，暂时不要设置parent，否则容易造成缩放错误
#else
        inst = GameObject.Instantiate(original, position, rotation, parent);
#endif

        return inst.GetComponent<People>();
    }

    [ContextMenu("Despawn")]
    public void Despawn()
    {
        onDespawn.Invoke();
        ResetState();

#if USE_LeanPool
        LeanPool.Despawn(gameObject);
#else
        Destroy(gameObject);
#endif
    }
    private void ResetState()
    {
        state = new PeopleState();
        stateRT = new PeopleStateRunTime();
        Shout(false);
    }
    public void TryShout()
    {
        if (curAudioCount <= maxAudioCount)//叫喊人数有上限
        {
            Shout(true);
        }
    }

    public void Shout(bool isShout)
    {
        if (!audioSource)
            return;

        if (isShout)
        {
            stateRT.isPlayedAudio = true;
            curAudioCount++;
            audioSource.Play();
        }
        else
        {
            if (stateRT.isPlayedAudio)
            {
                curAudioCount--;
                audioSource.Stop();
            }
        }
    }

#region Animator

    //蹲下前行
    public void Crouch(bool isSet)
    {
        state.isCrouch = isSet;//该属性由代码控制
    }
    public void SitDown(bool isSet)
    {
        m_Animator.SetBool("SitDown", isSet);
    }
    public void Hesitate(bool isSet)
    {
        m_Animator.SetBool("Hesitate", isSet);
    }
    public void Dodge(bool isSet)
    {
        m_Animator.SetBool("Dodge", isSet);
    }
    public void DodgeOnce()
    {
        m_Animator.SetTrigger("DodgeOnce");
    }
    public void Die()
    {
        m_Animator.SetTrigger("Die");
        Shout(false);
    }

#endregion

    /// <summary>
    /// People 运行时的状态
    /// </summary>
    [System.Serializable]
    public class PeopleStateRunTime
    {
        public bool isMoveToTarget = false;//移动到特定的目标点
        public bool isMoveAlongWayPoint = false;//是否需要移动到WayPoint
        public bool isPlayedAudio = false;//是否已经播放音频

        //Runtime
        public SimpleWaypoint wayPoint;
        public Transform targetPoint;
        public bool isFinishMoving = true;//移动结束，建议需要执行动画的设置该项为false，因为要实时更新角色位置
    }


#if UNITY_EDITOR

    //static Vector3 deltaGizmo = new Vector3(0, 0.1f, 0);
    [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NonSelected)]
    static void MyGizmo(People people, GizmoType gizmoType)  //参数1为“XX”组件，可以随意选，参数2 必须写，不用赋值  
    {
        if (people.stateRT.wayPoint)
        {
            Gizmos.color = Color.green;   //绘制时颜色  
            Gizmos.DrawSphere(people.stateRT.targetPoint.position, 0.5f);
        }
    }
#endif

}

/// <summary>
/// People的初始状态，可随意设置
/// </summary>
[System.Serializable]
public class PeopleState
{
    public float MoveSpeed = 1;//移动速度
    public bool isAutoDespawn = true;//移动结束后自动销毁

    public bool isCrouch = false;//蹲着行走
}