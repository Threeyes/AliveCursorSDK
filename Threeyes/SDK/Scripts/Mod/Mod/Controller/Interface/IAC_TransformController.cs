using Threeyes.Steamworks;
using UnityEngine;

public interface IAC_TransformController : IModControllerHandler
{
	AC_TransformControllerConfigInfoBase BaseConfig { get; }
	Transform CursorTransform { get; }
	float DeltaTime { get; }
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
	void UpdateCursorPosition(Vector3 value);
	void UpdateCursorRotation(Quaternion value);
}
