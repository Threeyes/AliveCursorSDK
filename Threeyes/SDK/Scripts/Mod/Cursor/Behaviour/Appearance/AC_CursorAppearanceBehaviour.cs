using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using Threeyes.Editor;
#endif
[AddComponentMenu(AC_EditorDefinition.ComponentMenuPrefix_Root_Cursor_Behaviour_Appearance + "AC_CursorAppearanceBehaviour", 0)]
public class AC_CursorAppearanceBehaviour : AC_BehaviourBase,
	IAC_SystemCursor_AppearanceChangedHandler
{
	#region Property & Field

	public AC_SystemCursorAppearanceType CursorAppearanceType { get { return cursorAppearanceType; } set { cursorAppearanceType = value; } }//Desire type to listen
	[SerializeField] protected AC_SystemCursorAppearanceType cursorAppearanceType = AC_SystemCursorAppearanceType.None;

	//#Run Time
	protected AC_SystemCursorAppearanceType lastMatchCursorAppearanceType = AC_SystemCursorAppearanceType.None;
	protected bool lastSystemCursorShowingState = false;
	public BoolEvent onSystemCursorShowHide;//Triggered when SystemCursor show/hide
	public BoolEvent onAppearanceEnterExit;//Triggered when the target appearance enter/exit

	#endregion

	#region Callback

	bool isAppearanceChanged = false;
	public virtual void OnSystemCursorAppearanceChanged(bool isSystemCursorShowing, AC_SystemCursorAppearanceInfo systemCursorAppearanceInfo)
	{
		//#1 SystemCursorShowingState
		if (isSystemCursorShowing != lastSystemCursorShowingState)
		{
			onSystemCursorShowHide.Invoke(isSystemCursorShowing);
			lastSystemCursorShowingState = isSystemCursorShowing;
		}

		//#2 SystemCursorAppearanceType
		AC_SystemCursorAppearanceType curCursorAppearanceType = systemCursorAppearanceInfo.systemCursorAppearanceType;
		///Logic:
		///1.Change to desire appearance: Play
		///2. desire appearance exit: Stop
		isAppearanceChanged = true;
		if (isSystemCursorShowing && CursorAppearanceType.Has(curCursorAppearanceType, true))
		{
			//PS: Play will get invoked multi time if CursorAppearanceType has more than one value
			if (curCursorAppearanceType != lastMatchCursorAppearanceType)
			{
				Play();
				lastMatchCursorAppearanceType = curCursorAppearanceType;
			}
		}
		else
		{
			TryStop();
		}
		isAppearanceChanged = false;
	}

	void TryStop()
	{
		if (lastMatchCursorAppearanceType != AC_SystemCursorAppearanceType.None)
		{
			Stop();
			lastMatchCursorAppearanceType = AC_SystemCursorAppearanceType.None;
		}
	}

	#endregion

	#region Inner Method

	protected override void PlayFunc()
	{
		base.PlayFunc();
		if (isAppearanceChanged)
			onAppearanceEnterExit.Invoke(true);
	}

	protected override void StopFunc()
	{
		base.StopFunc();

		if (isAppearanceChanged)
			onAppearanceEnterExit.Invoke(false);
	}

	#endregion

	#region Editor Method
#if UNITY_EDITOR

	//——MenuItem——

	static string instName = "CAB ";
	[UnityEditor.MenuItem(AC_EditorDefinition.HierarchyMenuPrefix_Root_Cursor_Behaviour_Appearance + "CursorAppearanceBehaviour", false, 0)]
	public static void CreateActionEventPlayer_CursorAppearanceBehaviour()
	{
		EditorTool.CreateGameObjectAsChild<AC_CursorInputBehaviour>(instName);
	}

	//——Hierarchy GUI——
	public override string ShortTypeName { get { return "CAB"; } }
	public override void SetHierarchyGUIProperty(StringBuilder sB)
	{
		base.SetHierarchyGUIProperty(sB);
		sbCache.Length = 0;
		if (CursorAppearanceType == AC_SystemCursorAppearanceType.All)
		{
			sbCache.Append("All");
		}
		else if (CursorAppearanceType != AC_SystemCursorAppearanceType.None)
		{
			sbCache.Append(CursorAppearanceType);
		}
		AddSplit(sB, sbCache);
	}

	//——Inspector GUI——

	public override void SetInspectorGUIUnityEventProperty(GUIPropertyGroup group)
	{
		group.title = "Unity Event";
		group.listProperty.Add(new GUIProperty(nameof(onActiveDeactive)));

		group.listProperty.Add(new GUIProperty(nameof(onAppearanceEnterExit), isEnable: cursorAppearanceType != AC_SystemCursorAppearanceType.None));
	}

	public override void SetInspectorGUISubProperty(GUIPropertyGroup group)
	{
		base.SetInspectorGUISubProperty(group);
		group.listProperty.Add(new GUIProperty(nameof(cursorAppearanceType)));
	}

#endif
	#endregion
}
