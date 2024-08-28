using Threeyes.GameFramework;
using UnityEngine;
/// <summary>
/// Cursor's appearance and Position Info in Scene
/// </summary>
public interface IAC_SystemCursorManager : IHubManagerModInitHandler
{
	bool IsSystemCursorShowing { get; }
	AC_SystemCursorAppearanceType CurSystemCursorAppearanceType { get; }
	AC_SystemCursorAppearanceType LastSystemCursorAppearanceType { get; }

	Vector3 WorldPosition { get; }
	Vector3 MousePosition { get; }
	float CurDepth { get; set; }

	/// <summary>
	/// Cursor position's depth relate to maincamera on working state
	/// </summary>
	float WorkingStateCameraDepth { get; }

	/// <summary>
	/// Cursor position's depth range relate to maincamera on bored state
	/// </summary>
	Vector2 BoredStateCameraDepthRange { get; }

	/// <summary>
	/// Cursor position's z range on bored state
	/// </summary>
	Vector2 BoredStateWorldZRange { get; }

	/// <summary>
	/// Get random point inside bored bounds
	/// </summary>
	/// <returns></returns>
	Vector3 GetRandomPointInsideBoredBounds();

	/// <summary>
	/// Check if world point inside bored bounds
	/// </summary>
	/// <param name="worldPosition"></param>
	/// <returns></returns>
	public bool IsInsideBoredBounds(Vector3 worldPosition);

	/// <summary>
	/// Check if a volume object inside main camera's view area
	/// </summary>
	/// <param name="pos"></param>
	/// <param name="boundSize"></param>
	/// <returns></returns>
	public bool IsVisible(Vector3 pos, Vector3 boundSize);

	/// <summary>
	/// Transforms current MousePosition from screen space into world space base on main camera
	/// </summary>
	/// <param name="depth"></param>
	/// <returns></returns>
	Vector3 GetWorldPosition(float depth);
}

public interface IAC_SystemCursor_AppearanceChangedHandler
{
	/// <summary>
	/// Called on the system cursor's appearance changed
	/// </summary>
	/// <param name="isSystemCursorShowing">If the system cursor showing</param>
	/// <param name="cursorAppearanceType">Current system cursor type, will be Null if failed to recognize by app</param>
	void OnSystemCursorAppearanceChanged(bool isSystemCursorShowing, AC_SystemCursorAppearanceInfo systemCursorAppearanceInfo);
}
