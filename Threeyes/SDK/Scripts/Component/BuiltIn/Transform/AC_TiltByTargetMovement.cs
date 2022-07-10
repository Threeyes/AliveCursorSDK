using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Tilt rotate around the axis
///
/// </summary>
public class AC_TiltByTargetMovement : AC_ConfigableUpdateComponentBase<Transform, AC_SOTiltByTargetMovementConfig, AC_TiltByTargetMovement.ConfigInfo>
{
	public Transform target;//Movement target

	[Header("Run Time")]
	public int sign;
	public Vector3 curVelocity;
	public Vector3 velocityOnMovementAxis;
	public float targetAngle = 0;//对应轴向的目标角度
	Vector3 lastTargetPosition;

	void Start()
	{
		lastTargetPosition = target.position;
	}

	protected override void UpdateFunc()
	{
		base.UpdateFunc();

		if (AC_ManagerHolder.TransformManager.ActiveController is AC_DefaultTransformController defaultTransformController)
		{
			if (!defaultTransformController.Config.isFixedAngle)//Only valid on FixedAngle
				return;
		}

		curVelocity = (target.position - lastTargetPosition) / DeltaTime;
		if (curVelocity.sqrMagnitude > 0.01f)//移动中
		{
			sign = Vector3.Dot(Comp.up, Vector3.up) > 0 ? -1 : 1;//检查当前物体朝向，确认sign(朝上为-1，朝下为1)                                                                        
			velocityOnMovementAxis = Vector3.Project(curVelocity, Comp.TransformDirection(Config.localDetectMovementAxis)); //获取velocity在物体局部移动轴上的分力矢量（因为光标可以任意旋转，所以使用局部坐标）
			targetAngle = Mathf.Clamp(targetAngle + sign * velocityOnMovementAxis.x * Config.increaseSpeed, -Config.maxAngle, Config.maxAngle);//计算要增加的角度
		}
		else//暂停移动
		{
			targetAngle = Mathf.Lerp(targetAngle, 0, Config.decreaseSpeed);//恢复原状
		}
		Comp.localRotation = Quaternion.Euler(Vector3.one.Multi(Config.localTiltAxis) * targetAngle);

		lastTargetPosition = Comp.position;
	}

	#region Define


	[System.Serializable]
	public class ConfigInfo : AC_SerializableDataBase
	{
		public float increaseSpeed = 0.3f;
		public float decreaseSpeed = 0.1f;
		public float maxAngle = 20;
		public Vector3 localDetectMovementAxis = new Vector3(1, 0, 0);//which local axis to detect movemnt
		public Vector3 localTiltAxis = new Vector3(0, 1, 0);//which axis to tilt
	}

	#endregion
}
