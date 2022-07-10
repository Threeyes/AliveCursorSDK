using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 跟随指定物体
/// </summary>
[ExecuteInEditMode]
public class AutoFollow : GameObjectBase
{

    #region Property

    public override Transform tfThis
    {
        get
        {
            if (tfSourceOverride)
                return tfSourceOverride;
            else
                return base.tfThis;
        }
    }

    public bool IsFollowPos
    {
        get
        {
            return isFollowPos;
        }

        set
        {
            isFollowPos = value;
        }
    }

    public bool IsFollowRot
    {
        get
        {
            return isFollowRot;
        }

        set
        {
            isFollowRot = value;
        }
    }

    public bool IsFollow
    {
        get { return isFollow; }
        set
        {
            isFollow = value;
            if (value)
                isInitAttachPos = false;
        }
    }
    public bool IsMorror { get { return isMirror; } set { isMirror = value; } }
    #endregion

    public UpdateMethodType updateType = UpdateMethodType.Late;
    public bool isExecuteInEditMode = false;

    [SerializeField]
    protected bool isFollow = true;//开关跟随

    [SerializeField]
    protected bool isAttachMode = false;//不修改位置及旋转，使用当前的状态进行持续追踪（类似FixJointAttach）

    [Header("位置")]
    [SerializeField]
    protected bool isFollowPos = true;//跟随位置
    public bool isUseLocalPosition = false;//是否使用局部坐标，适用于同步
    public bool isBaseOnTarget = false;//true:基于目标局部坐标+deltaPos; false:全局坐标 (针对Pos)（适用于远程操控效果）
    public Vector3 deltaPos = new Vector3(0, 0, 0);
    public Vector3 allowPosAxis = new Vector3(1, 1, 1);//允许更改的位置
    public bool isUseRangePos = false;
    public Range_Vector3 rangePos;//有效范围


    [Header("旋转")]
    [SerializeField]
    protected bool isFollowRot = false;//跟随旋转
    public bool isUseLocalRotation = false;//是否使用局部旋转，适用于Wheel、Knob等物体（针对Rot）
    public Vector3 deltaRot = new Vector3(0, 0, 0);
    public Vector3 allowRotAxis = new Vector3(1, 1, 1);//允许更改的旋转


    [Header("镜像")]
    [SerializeField]
    public bool isMirror = false;//镜像（Todo：功能未完善，暂时只支持局部坐标）
    public Transform tfMirrorPoint;//镜像的中心点
    public Vector3 allowMirrorAxis = new Vector3(1, 1, 1);//允许更改的镜像位置

    [Header("通用配置")]
    public Transform tfSourceOverride;//跟随的主体
    public Transform target;//待跟随的目标


    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }


    protected virtual void Update()
    {
        if (updateType == UpdateMethodType.Default)
            UpdateFollowState();
    }
    protected virtual void LateUpdate()
    {
        if (updateType == UpdateMethodType.Late)
            UpdateFollowState();
    }

    public void UpdateFollowState()
    {
        if (!IsFollow)
        {
            isInitAttachPos = false;
            isInitAttachRot = false;
            return;
        }

        if (!Application.isPlaying && !isExecuteInEditMode)
            return;

        if (!target)
            return;

        Follow();
    }

    //Attach初始化
    bool isInitAttachPos = false;
    bool isInitAttachRot = false;
    Vector3 relateVet;
    Quaternion relateQua;

    protected Vector3 CalTargetLocalPos()
    {
        Vector3 targetPos = default(Vector3);
        if (!target)
        {
            Debug.LogError("Empty target!");
        }
        else
        {
            targetPos = target.localPosition + deltaPos;
            targetPos = transform.localPosition.Lerp(targetPos, allowPosAxis);//剔除需要忽略的轴向
        }
        return targetPos;
    }
    /// <summary>
    /// 计算目标位置
    /// </summary>
    /// <returns></returns>
    protected Vector3 CalTargetPos()
    {
        Vector3 targetPos = default(Vector3);
        if (!target)
        {
            Debug.LogError("Empty target!");
        }
        else
        {
            //AttachMode
            if (isAttachMode)
            {
                if (!isInitAttachPos)
                {
                    relateVet = target.InverseTransformVector(tfThis.position - target.position);
                    isInitAttachPos = true;
                }
                targetPos = target.TransformPoint(relateVet);
                return targetPos;
            }

            //BaseOnTarget
            if (isBaseOnTarget)
            {
                targetPos = target.TransformPoint(deltaPos);//基于target坐标系的局部坐标
            }
            else
            {
                targetPos = target.position + deltaPos;//世界坐标
            }

            //Mirror
            if (isMirror)
            {
                if (tfMirrorPoint)
                {
                    Vector3 localPoint = tfMirrorPoint.InverseTransformPoint(targetPos);//将计算得出的坐标转为局部坐标
                    Vector3 totalMirrorPoint = localPoint * -1;

                    Vector3 mirrorPoint = localPoint.Lerp(totalMirrorPoint, allowMirrorAxis);//剔除需要忽略的轴向

                    targetPos = tfMirrorPoint.TransformPoint(mirrorPoint);
                }
            }

            if (isUseRangePos)
                targetPos = targetPos.ClampRange(rangePos.MinValue, rangePos.MaxValue);//限制有效的轴向

            targetPos = tfThis.position.Lerp(targetPos, allowPosAxis);//剔除多余的轴向

        }
        return targetPos;
    }

    [ContextMenu("Follow")]
    public virtual void Follow()
    {
        if (IsFollowPos)
        {
            if (isUseLocalPosition)
                tfThis.localPosition = CalTargetLocalPos();
            else
                tfThis.position = CalTargetPos();
        }

        if (IsFollowRot)
        {
            CalTargetRot();
        }
    }

    private void CalTargetRot()
    {
        if (!target)
            return;


        //相对旋转轴计算，参考：https://forum.unity.com/threads/need-help-with-relative-rotations.469764/
        if (isAttachMode)
        {
            if (!isInitAttachRot)
            {
                relateQua = Quaternion.Inverse(target.rotation) * tfThis.rotation;

                //relateVet = target.InverseTransformVector(tfThis.position - target.position);
                isInitAttachRot = true;
            }
            tfThis.rotation = target.rotation * relateQua;
            return;
        }

        if (isUseLocalRotation)
        {
            Quaternion quatTarget = target.localRotation * Quaternion.Euler(deltaRot);
            tfThis.localEulerAngles = tfThis.localEulerAngles.Lerp(quatTarget.eulerAngles, allowRotAxis);
        }
        else
        {
            Quaternion quatTarget = target.rotation * Quaternion.Euler(deltaRot);
            tfThis.eulerAngles = tfThis.eulerAngles.Lerp(quatTarget.eulerAngles, allowRotAxis);
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
            return;

        ///Version Update:
        ///参考DoTween，UpdateType.Manual 的值从-1变为4
        if ((int)updateType == (-1))
        {
            updateType = UpdateMethodType.Manual;
            Debug.Log("updateType Version Update for " + gameObject);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
        }

        LateUpdate();
    }
}
