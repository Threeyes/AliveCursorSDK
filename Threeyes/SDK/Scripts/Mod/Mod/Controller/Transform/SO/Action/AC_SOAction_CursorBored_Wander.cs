using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Wander on bored state
/// Ref: https://gamedev.stackexchange.com/questions/106737/wander-steering-behaviour-in-3d)
/// </summary>
[CreateAssetMenu(menuName = AC_EditorDefinition.ComponentMenuPrefix_Root_Mod_Controller_Transform_BoredAction + "Wander", fileName = "BoredAction_Wander")]
[JsonObject(MemberSerialization.OptIn)]
public class AC_SOAction_CursorBored_Wander : AC_SOAction_CursorBoredBase
{
	[JsonProperty] public AC_MovementConfig movementConfig = new AC_MovementConfig(1f, 3f);
	[JsonProperty] public float maxForce = 1;//Max acceleration
	[JsonProperty] [Range(0, 1)] public float turnChance = 0.02f;//The frequency of boredTarget to random turn
	[JsonProperty] public float turnPower = 1;//How much the boredTarget random turn


	//Runtime
	Vector3 moveDirection;
	Quaternion wantedRotation;
	Vector3 targetPos;
	Quaternion targetRot = Quaternion.identity;

	float boredTargetMaxSpeed;
	Vector3 boredTargetPos;
	Vector3 boredTargetVelocity;
	Vector3 boredTargetForce;

	protected IAC_SystemCursorManager SystemCursorManager { get { return AC_ManagerHolder.SystemCursorManager; } }

	public override void UpdateMovement_Bored(IAC_TransformController transformController, bool isBoredInit = false)
	{
		AC_TransformControllerConfigInfoBase transformControllerConfig = transformController.BaseConfig;

		float deltaTime = transformController.DeltaTime;
		Transform cursorTransform = transformController.CursorTransform;

		boredTargetMaxSpeed = movementConfig.moveSpeed * 1.5f;//Make sure the cursor can't catch up with boredTarget
		if (isBoredInit) //BoredBegin: setup start pos and velocity
		{
			boredTargetVelocity = cursorTransform.up * boredTargetMaxSpeed * 0.01f;
			boredTargetPos = cursorTransform.position;
		}

		//Calculate boredTargetPos (Don't directly change cursor's transform because the calculation is complex)
		Vector3 desiredVelocity = GetBoredTargetForce().normalized * boredTargetMaxSpeed;
		Vector3 steeringForce = Vector3.ClampMagnitude(desiredVelocity - boredTargetVelocity, maxForce * deltaTime);
		boredTargetVelocity = Vector3.ClampMagnitude(boredTargetVelocity + steeringForce, boredTargetMaxSpeed);
		boredTargetPos += boredTargetVelocity * deltaTime;
		boredTargetPos = new Vector3(boredTargetPos.x, boredTargetPos.y,
			transformControllerConfig.dimensionType == AC_TransformControllerConfigInfoBase.DimensionType.TwoD ? 0 :
			Mathf.Clamp(boredTargetPos.z, SystemCursorManager.BoredStateWorldZRange.x, SystemCursorManager.BoredStateWorldZRange.y));//Clamp the z position (Set z to 0 on 2D Dimension)

#if UNITY_EDITOR
		Debug.DrawRay(boredTargetPos, boredTargetVelocity.normalized * 2, Color.green);//Cur direction
		Debug.DrawRay(boredTargetPos, desiredVelocity.normalized * 2, Color.magenta);//Desire direction
#endif

		//——Rotation——
		moveDirection = boredTargetPos - cursorTransform.position;
		//if (transformControllerConfig.dimensionType == AC_TransformControllerConfigInfoBase.DimensionType.TwoD)//2D：保证只能在平面旋转（非必要，因为上述的代码基本可确保两个位置的Z为0）
		//{
		//	moveDirection.z = 0;
		//}
		wantedRotation = Quaternion.FromToRotation(Vector3.up, moveDirection);//参考transform.up的set方法实现，原理是将moveDirection对应为光标的up轴
		targetRot = Quaternion.Slerp(cursorTransform.rotation, wantedRotation, movementConfig.rotateSpeed * deltaTime);
		transformController.UpdateCursorRotation(targetRot);

		//——Position——
		targetPos = Vector3.Lerp(cursorTransform.position, boredTargetPos, movementConfig.moveSpeed * deltaTime);
		transformController.UpdateCursorPosition(targetPos);
	}
	Vector3 GetBoredTargetForce()
	{
		if (!SystemCursorManager.IsInsideBoredBounds(boredTargetPos))//Return to center once out of bounds
		{
			Vector3 boredBoundsCenterPos = new Vector3(0, 0, (SystemCursorManager.BoredStateWorldZRange.x + SystemCursorManager.BoredStateWorldZRange.y) / 2);//Calculate bored bounds' center
			Vector3 directionToCenter = (boredBoundsCenterPos - boredTargetPos).normalized;
			boredTargetForce = boredTargetVelocity.normalized + directionToCenter + Random.insideUnitSphere * 0.01f;//(Add small random direction)
		}
		else if (Random.value < turnChance)//Random turn
		{
			boredTargetForce = boredTargetVelocity.normalized + Quaternion.LookRotation(boredTargetVelocity) * Random.insideUnitSphere * turnPower;//(Rotates the velocity with rotation)
		}
		return boredTargetForce;
	}
}
