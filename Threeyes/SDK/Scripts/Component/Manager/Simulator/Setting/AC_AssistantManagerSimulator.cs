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

		tfGizmoGroup.gameObject.SetActive(AC_SOAliveCursorSDKManager.Instance.HubSimulator_ShowAssistantGizmo);
		EditorUtility.ClearDirty(tfGizmoGroup.gameObject);//避免修改导致Scene需要保存

		tfInfoGroup.gameObject.SetActive(AC_SOAliveCursorSDKManager.Instance.HubSimulator_ShowAssistantInfo);
		EditorUtility.ClearDirty(tfInfoGroup.gameObject);//避免修改导致Scene需要保存
	}

	string strCursorInfo = "";
	void Update()
	{
		if (!Application.isPlaying)
			return;

		//#Info Group
		if (textCursorInfo.gameObject.activeInHierarchy)
		{
			textCursorInfo.text =
		$"SystemCursor: {(AC_ManagerHolder.SystemCursorManager.IsSystemCursorShowing ? "Show" : "Hide") + "_" + AC_ManagerHolder.SystemCursorManager.CurSystemCursorAppearanceType}" + "\r\n" +
		$"CursorSize: {AC_ManagerHolder.CommonSettingManager.CursorSize}" + "\r\n" +
		$"CursorState: {AC_ManagerHolder.StateManager.CurCursorState}"
		;
		}
	}
}
#endif
