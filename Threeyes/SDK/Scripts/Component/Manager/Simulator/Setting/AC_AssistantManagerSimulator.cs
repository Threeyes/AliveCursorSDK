#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 辅助Modder进行Cursor建模
/// </summary>
[ExecuteInEditMode]
public class AC_AssistantManagerSimulator : MonoBehaviour
{
	public AC_DefaultTransformController defaultTransformController;
	public Transform tfGizmoGroup;//显示光标默认边界及热点
	public Transform tfInfoGroup;//显示光标信息

	public Toggle toggleFixedAngle;
	public Text textToggleFixedAngle;
	public Slider sliderCursorWorkingAngle;
	public Text textCursorInfo;

	private void OnEnable()
	{
		//根据需要选择是否显示对应物体(通过ExecuteInEditMode调用)
		ShowGameobjectWithoutSaving(tfGizmoGroup.gameObject, AC_SOAliveCursorSDKManager.Instance.HubSimulator_ShowAssistantGizmo);
		ShowGameobjectWithoutSaving(tfInfoGroup.gameObject, AC_SOAliveCursorSDKManager.Instance.HubSimulator_ShowAssistantInfo);
	}
	private void Start()
	{
		if (!Application.isPlaying)
			return;
		sliderCursorWorkingAngle.SetValueWithoutNotify(defaultTransformController.Config.workingAngle);
		sliderCursorWorkingAngle.onValueChanged.AddListener(OnCursorWorkingAngleChanged);
		toggleFixedAngle.SetIsOnWithoutNotify(defaultTransformController.Config.isFixedAngle);
		toggleFixedAngle.onValueChanged.AddListener(OnFixedAngleToggleChanged);
		UpdateUIState();
	}

	//因为某些原因需要临时隐藏UI（如截图）
	public void TempShowInfoGroup(bool isShow)
	{
		if (!isShow)
		{
			ShowGameobjectWithoutSaving(tfInfoGroup.gameObject, false);
		}
		else
		{
			ShowGameobjectWithoutSaving(tfInfoGroup.gameObject, AC_SOAliveCursorSDKManager.Instance.HubSimulator_ShowAssistantInfo);//根据设置决定是否临时显示
		}
	}

	string strCursorInfo = "";
	AC_StateManagerSimulator stateManagerSimulator { get { return AC_StateManagerSimulator.Instance; } }
	void Update()
	{
		if (!Application.isPlaying)
			return;

		//#Info Group
		if (textCursorInfo.gameObject.activeInHierarchy)
		{
			//Appearance
			strCursorInfo =
		$"SystemCursor (↕↔): {(AC_ManagerHolder.SystemCursorManager.IsSystemCursorShowing ? "Show" : "Hide") + "_" + AC_ManagerHolder.SystemCursorManager.CurSystemCursorAppearanceType}" +
		"\r\n" + $"CursorSize (-=): {AC_ManagerHolder.CommonSettingManager.CursorSize}";

			//State
			strCursorInfo += "\r\n" + $"CursorState {(stateManagerSimulator.isDebugNumberKeysChangeState ? "(1~7)" : "")}: {AC_ManagerHolder.StateManager.CurCursorState}";
			if (stateManagerSimulator.isDebugNumberKeysChangeState && stateManagerSimulator.isDebugIgnoreInput)
			{
				strCursorInfo += "\r\n" + "(State ignoring Input)";

			}
			else
			{
				if (AC_ManagerHolder.CommonSettingManager.IsStandByActive)
					strCursorInfo += "\r\n" + $"StandBy after: {AC_ManagerHolder.CommonSettingManager.StandByDelayTime.ToString("F2")}s";
				if (AC_ManagerHolder.CommonSettingManager.IsBoredActive)
					strCursorInfo += "\r\n" + $"Bored after: {AC_ManagerHolder.CommonSettingManager.BoredDelayTime.ToString("F2")}s";
			}

			textCursorInfo.text = strCursorInfo;
		}
	}

	#region UI Callback
	private void OnFixedAngleToggleChanged(bool isOn)
	{
		defaultTransformController.Config.isFixedAngle = isOn;
		UpdateUIState();
	}
	private void OnCursorWorkingAngleChanged(float value)
	{
		defaultTransformController.Config.workingAngle = value;
		UpdateUIState();
	}
	void UpdateUIState()
	{
		bool isFixedAngle = defaultTransformController.Config.isFixedAngle;
		sliderCursorWorkingAngle.gameObject.SetActive(isFixedAngle);

		textToggleFixedAngle.text = $"FixedAngle" + (isFixedAngle ? "[" + (int)defaultTransformController.Config.workingAngle + "]" : "");
	}
	#endregion

	#region Utility
	static void ShowGameobjectWithoutSaving(GameObject go, bool isShow)
	{
		go.SetActive(isShow);
		EditorUtility.ClearDirty(go);//避免修改导致Scene需要保存
	}
	#endregion
}
#endif
