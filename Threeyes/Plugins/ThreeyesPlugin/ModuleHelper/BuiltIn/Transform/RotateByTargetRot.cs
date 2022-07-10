using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 同步旋转（通过target的差值计算，因此2个物体不需要保持局部坐标一致）
/// 适用：方向盘、轮胎回正
/// </summary>
[ExecuteInEditMode]
public class RotateByTargetRot : GameObjectBase
{
    public Transform target;
    public Vector3 rotRange = new Vector3(180, 180, 180);//旋转范围[-180,180]
    public float rotatePower = 0.8f;
    public bool isInitStartRot = false;
    public bool isReturnToOrigin = false;
    public float returnAngleThreshold = 0.02f;//当target前后两帧的角度小于某个值时，开始回正操作
    public float returnToOriginPower = 0.5f;//回正力度（适用于轮胎转正）（值范围为[0,1]）

    [Header("Runtime")]
    public Vector3 startRot;

    public Vector3 rotateDeltaVectorLastFrame;
    public float targetBetweenAngleLastFrame;
    public Vector3 lastTargetRot;//缓存上帧Target的旋转值
    private void Start()
    {
        if (isInitStartRot)
            RecordOriginRot();

        targetRotateAngle = tfThis.localEulerAngles;
    }

    [ContextMenu("RecordStartRot")]
    public void RecordOriginRot()
    {
        startRot = tfThis.localEulerAngles;
    }

    private void Update()
    {
        if (!target)
            return;


        targetBetweenAngleLastFrame = Vector3.Angle(lastTargetRot, target.localEulerAngles);
        rotateDeltaVectorLastFrame = target.localEulerAngles.DeltaAngle(lastTargetRot);

        if (isReturnToOrigin && targetBetweenAngleLastFrame < returnAngleThreshold)
        {
            ReturnToOrigin();
        }
        else
        {
            RotateThis();
        }

        lastTargetRot = target.localEulerAngles;
    }

    Vector3 targetRotateAngle;
    protected virtual void RotateThis()
    {
        targetRotateAngle = (targetRotateAngle + rotateDeltaVectorLastFrame).ClampAngle(startRot, rotRange);
        tfThis.localEulerAngles = Vector3.Lerp(tfThis.localEulerAngles.GetStandardAngle(), targetRotateAngle, rotatePower * Time.deltaTime);
    }
    protected virtual void ReturnToOrigin()
    {
        tfThis.localEulerAngles = Vector3.Lerp(tfThis.localEulerAngles.GetStandardAngle(), startRot, returnToOriginPower * Time.deltaTime);
    }
}
