#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using UnityEditor;
/// <summary>
/// 缓存用户设置
/// </summary>
public class AC_SOAliveCursorSDKManager : SOInstacneBase<AC_SOAliveCursorSDKManager>
{
	#region Instance
	static string defaultName = "AliveCursorSDKSetting";
	static string pathInResources = "Threeyes";
	public static AC_SOAliveCursorSDKManager Instance { get { return GetOrCreateInstance(ref instance, defaultName, pathInResources); } }
	private static AC_SOAliveCursorSDKManager instance;
	#endregion

	#region Property & Field

	public AC_SOWorkshopItemInfo CurWorkshopItemInfo
	{
		get
		{ return curWorkshopItemInfo; }
		set
		{
			curWorkshopItemInfo = value;
			EditorUtility.SetDirty(Instance);
		}
	}

	//——AC_ItemManagerWindow——
	public bool ItemWindow_IsPreviewGif
	{
		get
		{
			return itemWindow_IsPreviewGif;
		}
		set
		{
			itemWindow_IsPreviewGif = value;
			EditorUtility.SetDirty(Instance);
		}
	}
	public bool HubSimulator_ShowAssistantGizmo
	{
		get
		{
			return hubSimulator_ShowAssistantGizmo;
		}
		set
		{
			hubSimulator_ShowAssistantGizmo = value;
			EditorUtility.SetDirty(Instance);
		}
	}
	public bool HubSimulator_ShowAssistantInfo
	{
		get
		{
			return hubSimulator_ShowAssistantInfo;
		}
		set
		{
			hubSimulator_ShowAssistantInfo = value;
			EditorUtility.SetDirty(Instance);
		}
	}
	[Expandable] [SerializeField] protected AC_SOWorkshopItemInfo curWorkshopItemInfo;

	[Header("ItemManagerWindow")]
	[SerializeField] protected bool itemWindow_IsPreviewGif = false;

	[Header("HubSimulator")]
	[SerializeField] protected bool hubSimulator_ShowAssistantGizmo = true;
	[SerializeField] protected bool hubSimulator_ShowAssistantInfo = true;

	#endregion
}
#endif
