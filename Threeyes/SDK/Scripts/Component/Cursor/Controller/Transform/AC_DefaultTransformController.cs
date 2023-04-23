using NaughtyAttributes;
using Newtonsoft.Json;
using Threeyes.Action;
using UnityEngine;
/// <summary>
/// Control Cursor Movement
/// </summary>
[AddComponentMenu(AC_EditorDefinition.ComponentMenuPrefix_AC_Cursor_Controller + "AC_DefaultTransformController")]
public class AC_DefaultTransformController : AC_TransformControllerBase
	<AC_SODefaultTransformControllerConfig, AC_DefaultTransformController.ConfigInfo>
{
	#region Property & Field
	//Runtime
	float curDistance;
	Vector3 wantedPosition;
	Vector3 moveDirection;
	float curSlowDownAnglePercent;
	Quaternion wantedRotation;
	Vector3 targetPos;
	Quaternion targetRot = Quaternion.identity;

	protected override Vector3 SystemCursorPosition
	{
		get
		{
			Vector3 value = base.SystemCursorPosition;

			return value;
		}
	}
	#endregion


	public override void UpdateMovement()
	{
		if (!cursorTransform)
			return;

		wantedPosition = SystemCursorPosition;
		AC_CursorState curCursorState = StateManager.CurCursorState;

		if (Config.dimensionType == ConfigInfo.DimensionType.TwoD)//Set Z to 0
		{
			if (curCursorState != AC_CursorState.Enter && curCursorState != AC_CursorState.Exit)
				wantedPosition.z = 0;
		}

		if (curCursorState == AC_CursorState.Bored)
		{
			bool isBoredInit = lastSavedCursorState != AC_CursorState.Bored;
			UpdateMovement_Bored(isBoredInit);
		}
		else//Other States
		{
			AC_MovementConfig movementConfig = Config.commonMovementConfig;
			curDistance = Vector3.Distance(wantedPosition, cursorTransform.position);

			//根据与目标的距离,决定朝向
			if (curDistance > Config.reachThreshold)//移动中：更新moveDirection（PS: 如果光标停止移动，moveDirection应该保持上次的朝向，避免停止时moveDirection为0导致光标变成默认旋转角度的bug）
			{
				moveDirection = wantedPosition - cursorTransform.position;
			}
			wantedRotation = Quaternion.FromToRotation(Vector3.up, moveDirection);//参考transform.up的set方法实现，原理是将moveDirection对应为光标的up轴
			if (Vector3.Angle(moveDirection, Vector3.down) < 1)//修正：光标朝正下方时，偏转180度的Bug
			{
				wantedRotation = Quaternion.Euler(0, 0, 180);
			}

			//——Rotation——
			bool lerpAngle = true;//Lerp计算旋转方向(Show、Hide、StandBy等也需要固定角度，否则会变来变去).Working_Enter_Complete后才能设置为固定角度（也就是要等待动画完成）
			curSlowDownAnglePercent = 1;
			if (!Config.isFixedAngle)//Rotate around pivot
			{
				//根据 （前进方向与当前方向的夹角）与（阈值夹角）的比值，缩小旋转的速率[即角度变化越小，旋转速率变化越慢] 解决:小范围移动时频繁偏转导致抽搐的问题
				float curAngle = Vector3.Angle(cursorTransform.up, moveDirection);
				if (curAngle < Config.slowDownAngleThreshold)
				{
					curSlowDownAnglePercent = Config.slowDownAnimationCurve.Evaluate(Mathf.Clamp01(curAngle / Config.slowDownAngleThreshold));
				}
			}
			else//Fixed Angle
			{
				//从（Show/Hide/StangBy）时，需要固定其角度,禁止Lerp（因为通常是在原点显隐，没有更改深度，所以不需要多余的旋转）;
				if ((curCursorState == AC_CursorState.Show || curCursorState == AC_CursorState.Hide || curCursorState == AC_CursorState.StandBy))
				{
					lerpAngle = false;
				}
				else if (curCursorState == AC_CursorState.Working)
				{
					//Working时，默认禁止Lerp
					lerpAngle = false;

					//从非（Show/StangBy）状态切换到Working，且当前Tween未完成前，需要Lerp（因为深度可能改变，需要做朝向动画）
					AC_CursorState lastCursorState = StateManager.LastCursorState;
					if ((lastCursorState != AC_CursorState.Show && lastCursorState != AC_CursorState.StandBy) && !StateManager.IsCurStateActionComplete(ActionState.Enter))
						lerpAngle = true;
				}
				//其他情况：允许Lerp
			}
			targetRot = lerpAngle ? Quaternion.Slerp(cursorTransform.rotation, wantedRotation, movementConfig.rotateSpeed * DeltaTime * curSlowDownAnglePercent) : Quaternion.Euler(new Vector3(0, 0, Config.workingAngle));
			UpdateCursorRotation(targetRot);

			//——Position——
			bool lerpPos = !(curCursorState == AC_CursorState.Working && Config.isFixedAngle);//非(Working&&固定轴向)：Lerp，避免移动顿挫。
			targetPos = lerpPos ? Vector3.Lerp(cursorTransform.position, wantedPosition, movementConfig.moveSpeed * DeltaTime) : wantedPosition;
			UpdateCursorPosition(targetPos);
		}
		lastSavedCursorState = curCursorState;
	}
	protected virtual void UpdateMovement_Bored(bool isBoredInit)
	{
		SOBoredAction.UpdateMovement_Bored(this, isBoredInit);
	}

	#region Define
	[System.Serializable]
	public class ConfigInfo : AC_TransformControllerConfigInfoBase
	{
		[Header("Common")]
		public AC_MovementConfig commonMovementConfig = new AC_MovementConfig(30, 15);//Movement config for common states excpet bored
		[Range(1, 180)] public float slowDownAngleThreshold = 30;//[When Lerping] Avoid glithing on small movement(绕轴旋转时减弱小范围移动/旋转时的抖动)
		[JsonIgnore] public AnimationCurve slowDownAnimationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));//Curve to slowdown rotation
		public float reachThreshold = 0.05f;//How close is treat as reach

		[Header("Working")]
		public bool isFixedAngle = true;//Using fixed angle on working state
		[EnableIf(nameof(isFixedAngle))] [AllowNesting] [Range(0, 360)] public float workingAngle = 0;//The target angle if isFixedAngle set to true
	}
	#endregion

	#region Editor Method
#if UNITY_EDITOR
	//——MenuItem——
	static string instName = "DefaultTransformController";
	[UnityEditor.MenuItem(AC_EditorDefinition.HierarchyMenuPrefix_Cursor_Controller_Transform + "Default", false)]
	public static void CreateInst()
	{
		Threeyes.Editor.EditorTool.CreateGameObjectAsChild<AC_DefaultTransformController>(instName);
	}
#endif
	#endregion
}