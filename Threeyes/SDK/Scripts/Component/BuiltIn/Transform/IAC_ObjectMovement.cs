using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Control object movement
/// </summary>
public interface IAC_ObjectMovement
{
	/// <summary>
	/// Check if the target moving 
	/// </summary>
	bool IsMoving { get; }

	
	float CurMoveSpeedPercent { get; }
	float CurMoveSpeed { get; }
	float MaxMoveSpeed { get; }
	float LastMoveTime { get; }
}
