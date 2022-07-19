#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
/// <summary>
/// 辅助Modder进行Cursor建模
/// </summary>
[ExecuteInEditMode]
public class AC_AssistantManagerSimulator : MonoBehaviour
{
	public List<AC_GizmoDrawer> listGizmoDrawer = new List<AC_GizmoDrawer>();
	private void OnEnable()
	{
		//根据需要选择是否显示
		foreach (var element in listGizmoDrawer)
		{
			element.gameObject.SetActive(AC_SOAliveCursorSDKManager.Instance.HubSimulator_UseAssistant);
			EditorUtility.ClearDirty(element.gameObject);//避免修改导致Scene需要保存

		}
	}
}
#endif