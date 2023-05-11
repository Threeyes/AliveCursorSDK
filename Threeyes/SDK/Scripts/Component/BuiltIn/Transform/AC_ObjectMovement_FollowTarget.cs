using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;


/// <summary>
/// Follow and look at target
/// 
/// 要求：
/// -跟随某一点移动
/// </summary>
public class AC_ObjectMovement_FollowTarget : AC_ConfigableComponentBase<AC_SOObjectMovement_FollowTargetConfig, AC_ObjectMovement_FollowTarget.ConfigInfo>, IAC_ObjectMovement
{
	public bool IsMoving { get { return isMoving; } }
	public float CurMoveSpeedPercent { get { return CurMoveSpeed / MaxMoveSpeed; } }
	public float MaxMoveSpeed { get { return Config.maxMoveSpeed; } }
	public float CurMoveSpeed { get { return curMoveSpeed; } }
	public float LastMoveTime { get { return lastMoveTime; } }

	public Transform tfPosTarget;
	public Transform tfLookTarget;
	public float stoppingDistance = 0.01f;

	[Header("Runtime")]
	private bool isMoving = false;
	private float curMoveSpeed = 0;
	float lastMoveTime = 0;
	Vector3 lastPos;

	private void Start()
	{
		if (!tfLookTarget)
			tfLookTarget = tfPosTarget;
		lastPos = tfPosTarget.position;
	}

	Vector3 lastForward;
	protected virtual void Update()
	{
		isMoving = false;
		curMoveSpeed = 0;
		Vector3 curPos = transform.position;
		Vector3 targetPos = tfPosTarget.position;
		Vector3 targetDirection = targetPos - transform.position;
		float curDistance = Vector3.Distance(targetPos, curPos);
		if (curDistance > stoppingDistance)
		{
			isMoving = true;
			curMoveSpeed = Mathf.Min(curDistance, Config.maxMoveSpeed);
			transform.position = transform.position + targetDirection.normalized * curMoveSpeed * Time.deltaTime * AC_ManagerHolder.CommonSettingManager.CursorSize;
			lastMoveTime = Time.time;
		}
		else
		{
			transform.position = targetPos;
		}

		Vector3 worldUp = tfLookTarget.TransformVector(Config.localUpAxis);
		Vector3 forward = tfLookTarget.position - transform.position;
		if (forward == Vector3.zero)
			forward = lastForward;
		else
			lastForward = forward;
		Quaternion targetRotation = Quaternion.LookRotation(forward, worldUp);//通过局部轴的up动态转换为世界worldUp

		transform.rotation = Config.isInstantRotate ? targetRotation : Quaternion.RotateTowards(transform.rotation, targetRotation, Config.rotateSpeed * Time.deltaTime);

		lastPos = curPos;
	}

	#region Define
	[System.Serializable]
	public class ConfigInfo : AC_SerializableDataBase
	{
		[Min(0.01f)] public float maxMoveSpeed = 1;//Max move speed per second
		public Vector3 localUpAxis = new Vector3(0, 0, -1);//Up axis base on tfPosTarget
		public bool isInstantRotate = true;
		[DisableIf(nameof(isInstantRotate))] [AllowNesting] public float rotateSpeed = 360;//Max rotate speed per second
	}
	#endregion
}
