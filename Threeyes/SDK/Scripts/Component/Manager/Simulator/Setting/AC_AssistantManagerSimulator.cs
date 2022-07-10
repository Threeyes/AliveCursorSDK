#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
/// <summary>
/// 辅助Modder进行Cursor建模
/// </summary>
[ExecuteInEditMode]
public class AC_AssistantManagerSimulator : MonoBehaviour
{
	private void OnEnable()
	{
		//根据需要选择是否显示
		gameObject.SetActive(AC_SOAliveCursorSDKManager.Instance.HubSimulator_UseAssistant);
		EditorUtility.ClearDirty(gameObject);//避免修改导致Scene需要保存
	}
}
#endif