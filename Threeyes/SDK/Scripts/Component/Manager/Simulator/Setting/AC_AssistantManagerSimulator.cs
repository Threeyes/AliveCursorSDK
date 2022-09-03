#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 辅助Modder进行Cursor建模
/// </summary>
[ExecuteInEditMode]
public class AC_AssistantManagerSimulator : MonoBehaviour
{
	public Transform tfGizmoGroup;//显示光标默认边界及热点
	public Transform tfInfoGroup;//显示光标信息

	public Text textCursorInfo;
	private void OnEnable()
	{
		//根据需要选择是否显示对应物体
		ShowGameobjectWithoutSaving(tfGizmoGroup.gameObject, AC_SOAliveCursorSDKManager.Instance.HubSimulator_ShowAssistantGizmo);
		ShowGameobjectWithoutSaving(tfInfoGroup.gameObject, AC_SOAliveCursorSDKManager.Instance.HubSimulator_ShowAssistantInfo);
	}

	//因为某些原因需要临时隐藏UI
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

	static void ShowGameobjectWithoutSaving(GameObject go, bool isShow)
	{
		go.SetActive(isShow);
		EditorUtility.ClearDirty(go);//避免修改导致Scene需要保存

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
				strCursorInfo += "\r\n" + "(Ignoring Input)";

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
}
#endif
