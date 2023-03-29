using System;
using UnityEngine;

public interface IAC_TransformController : IAC_ModControllerHandler
{
	void UpdateFunc();
	void FixedUpdateFunc();

	/// <summary>
	/// Move to target point at once
	/// 
	/// Eg: Move the cursor to other display
	/// </summary>
	/// <param name="position"></param>
	void Teleport(Vector3 position);
	/// <summary>
	/// Set Cursor's local scale
	/// </summary>
	/// <param name="size"></param>
	/// <param name="unitScale"></param>
	void SetLocalScale(float size, Vector3 unitScale);
}

/// <summary>
/// Control Cursor Movement
/// 
/// PS:
/// 1. Must attached to the AliveCursor gameobject
/// </summary>
public abstract class AC_TransformControllerBase<TSOConfig, TConfig> : AC_ConfigableComponentBase<TSOConfig, TConfig>,
	IAC_TransformController,
	IAC_CommonSetting_CursorSizeHandler
	where TSOConfig : AC_SOConfigBase<TConfig>
{
	protected IAC_TransformManager TransformManager { get { return AC_ManagerHolder.TransformManager; } }
	protected IAC_StateManager StateManager { get { return AC_ManagerHolder.StateManager; } }
	protected IAC_SystemCursorManager SystemCursorManager { get { return AC_ManagerHolder.SystemCursorManager; } }

	public AC_AliveCursor CurAliveCursor { get { if (!curAliveCursor) curAliveCursor = AC_AliveCursor.Instance; return curAliveCursor; } }
	protected AC_AliveCursor curAliveCursor;

	public Rigidbody CursorRigidbody { get { return cursorRigidbody; } }
	protected Rigidbody cursorRigidbody;//Cursor's rigidbody, can be null (PS: should set to kinematic)
	public Transform CursorTransform { get { return cursorTransform; } }
	protected Transform cursorTransform;//Cursor's transform

	protected float deltaTime { get { return/* CursorRigidbody ? Time.fixedDeltaTime : */Time.deltaTime; } }
	protected Vector3 SystemCursorPosition { get { return AC_ManagerHolder.SystemCursorManager.WorldPosition; } }

	#region Callback
	protected AC_CursorState lastSavedCursorState = AC_CursorState.None;
	public virtual void OnModControllerInit()
	{
		cursorRigidbody = CurAliveCursor.GetComponent<Rigidbody>();//Try to get Rigidbody
		cursorTransform = CurAliveCursor.transform;
		lastSavedCursorState = StateManager.CurCursorState;

		//Init
		Teleport(SystemCursorPosition);//Teleport at once
		SetLocalScale(AC_ManagerHolder.CommonSettingManager.CursorSize, TransformManager.CursorBaseScale);//Set init scale
	}
	public virtual void OnModControllerDeinit() { }

	public virtual void OnCursorSizeChanged(float size)
	{
		SetLocalScale(size, TransformManager.CursorBaseScale);
	}
	#endregion

	//根据是否使用Rigidbody，决定对应调用的Update方法
	public virtual void UpdateFunc()
	{
		if (!CurAliveCursor)
			return;

		//if (!cursorRigidbody)
		UpdateMovement();
	}
	public virtual void FixedUpdateFunc()
	{
		//ToDelete:会导致跟随系统光标延迟的问题，统一改为移动Transform组件
		//if (!CurAliveCursor)
		//	return;

		//if (cursorRigidbody)
		//	UpdateMovement();
	}
	public abstract void UpdateMovement();
	public virtual void Teleport(Vector3 position)
	{
		UpdateCursorPosition(position);
	}
	public virtual void SetLocalScale(float size, Vector3 unitScale)
	{
		if (cursorTransform)
			cursorTransform.localScale = size * unitScale;
	}

	protected virtual void UpdateCursorPosition(Vector3 value)
	{
		//根据物体有无Rigidbody，调用对应方法
		//if (cursorRigidbody)
		//	cursorRigidbody.MovePosition(value);
		//else if (cursorTransform)
		cursorTransform.position = value;
	}
	protected virtual void UpdateCursorRotation(Quaternion value)
	{
		//if (cursorRigidbody)
		//	cursorRigidbody.MoveRotation(value);
		//else
		cursorTransform.rotation = value;
	}
}

[Serializable]
public class AC_MovementConfig
{
	public float moveSpeed = 30;
	public float rotateSpeed = 15;

	public AC_MovementConfig()
	{
	}

	public AC_MovementConfig(float moveSpeed, float rotateSpeed)
	{
		this.moveSpeed = moveSpeed;
		this.rotateSpeed = rotateSpeed;
	}
}
