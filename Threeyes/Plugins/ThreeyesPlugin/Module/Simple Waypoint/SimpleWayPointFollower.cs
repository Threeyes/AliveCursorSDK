using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 跟随路标移动
/// </summary>
public class SimpleWayPointFollower : GameObjectBase
{
    [Header("属性")]
    public SimpleWaypoint wayPoint;
    public float MoveSpeed
    {
        get
        {
            return moveSpeed;
        }

        set
        {
            moveSpeed = value;
        }
    }
    public bool IsLookAt
    {
        get
        {
            return isLookAt;
        }

        set
        {
            isLookAt = value;
        }
    }

    public Vector3 avaliableRotAxis = new Vector3(1, 1, 1);//可以更改的旋转轴向（0禁止，1允许）
    public float moveSpeed = 3;
    public bool isLookAt = true;

    public float finishDistance = 0.3f;
    public bool isReverseDirPointAsComplete = true;//是否把后方的点算作已经到达
    public float turnSpeed = 3;

    public bool isFinishMoving = true;//移动结束，
    bool isPause = false;//暂停移动
    public bool isMoveOnStart = false;
    public UnityEvent onMoveStart;
    public UnityEvent onMoveFinish;

    private void Start()
    {
        if (isMoveOnStart && wayPoint)
            MoveAlong();
    }
    public void SetWayPoint(SimpleWaypoint wayPoint)
    {
        this.wayPoint = wayPoint;
    }

    public void MoveAlong(SimpleWaypoint wayPoint)
    {
        SetWayPoint(wayPoint);
        MoveStart();
    }
    public void MoveAlong()
    {
        MoveStart();
    }
    public void Play()
    {
        isPause = false;
    }
    public void Pause()
    {
        isPause = true;
    }

    [Header("RunTime Property")]
    public float curSpeed = 0;
    protected Vector3 m_MoveDir;
    protected Transform targetPoint { get { return wayPoint ? wayPoint.tfThis : null; } }


    private void Update()
    //private void FixedUpdate()
    {
        if (isFinishMoving)
        {
            return;
        }
        if (!targetPoint)
            isFinishMoving = true;

        if (isPause)
            return;

        if (Vector3.Distance(tfThis.position, targetPoint.position) <= finishDistance || wayPoint.isImmediatelyReach || (isReverseDirPointAsComplete && Vector3.Dot(tfThis.forward, targetPoint.position - tfThis.position) < -0.5f))//到达目标点 | 立即到达 | 速度太快导致越过目标点（即目标点在后方）
        {
            //到达目标点
            if (wayPoint.isImmediatelyReach && isLookAt)
                tfThis.rotation = wayPoint.tfThis.rotation;
            tfThis.position = targetPoint.position;
            wayPoint.onReach.Invoke();//调用路标点的事件
            wayPoint = GetNextWayPoint();//朝向到下一路标点
            if (!wayPoint)//到达终点：结束.
            {
                MoveFinish();
                return;
            }
        }
        UpdateMovement();
    }

    protected virtual void UpdateMovement()
    {
        if (isLookAt)
        {
            //执行旋转
            Quaternion direction = Quaternion.LookRotation(targetPoint.position - tfThis.position, tfThis.TransformDirection(Vector3.up));
            Vector3 vt3NewDir = direction.eulerAngles;
            Vector3 vt3CurRot = tfThis.rotation.eulerAngles;
            vt3NewDir = new Vector3(
                avaliableRotAxis.x>0?vt3NewDir.x:vt3CurRot.x,
                avaliableRotAxis.y>0?vt3NewDir.y:vt3CurRot.y,
                avaliableRotAxis.z > 0 ? vt3NewDir.z : vt3CurRot.z);
            direction = Quaternion.Euler(vt3NewDir);
            Quaternion newRotation = Quaternion.Lerp(tfThis.rotation, direction, Time.deltaTime * turnSpeed);
            tfThis.rotation = newRotation;
        }

        curSpeed = Mathf.Lerp(curSpeed, moveSpeed, Time.deltaTime);

        m_MoveDir = (targetPoint.position - tfThis.position).normalized * moveSpeed * Time.deltaTime;
        tfThis.Translate(m_MoveDir, Space.World);
    }

    SimpleWaypoint GetNextWayPoint()
    {
        SimpleWaypoint nextWaypoint = wayPoint.nextWayPoint;
        return nextWaypoint;
    }
    protected virtual void MoveStart()
    {
        onMoveStart.Invoke();
        isFinishMoving = false;
        isPause = false;
    }
    protected virtual void MoveFinish()
    {
        isFinishMoving = true;
        onMoveFinish.Invoke();
    }
}
