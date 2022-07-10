using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using Threeyes.Editor;
#endif

/// <summary>
/// Custom Behaviour when cursor state changed
/// </summary>
[AddComponentMenu(AC_EditorDefinition.ComponentMenuPrefix_AC_Cursor_Behaviour_State + "AC_CursorStateBehaviour", 0)]
public class AC_CursorStateBehaviour : AC_BehaviourBase,
    IAC_CursorState_ChangedHandler
{
    #region Property & Field
    public AC_CursorState CursorState { get { return cursorState; } set { cursorState = value; } }
    [SerializeField] protected AC_CursorState cursorState = AC_CursorState.None;

    public BoolEvent onStateEnterExit;//Triggered when the state enter/exit
    #endregion

    #region Callback

    bool isStateChanged = false;
    public void OnCursorStateChanged(AC_CursorStateInfo cursorStateInfo)
    {
        if (!cursorState.Has(cursorStateInfo.cursorState, true))
            return;

        isStateChanged = true;
        AC_CursorStateInfo.StateChange smbState = cursorStateInfo.stateChange;
        if (smbState == AC_CursorStateInfo.StateChange.Enter)
            Play();
        else if (smbState == AC_CursorStateInfo.StateChange.Exit)
            Stop();
        isStateChanged = false;
    }

    #endregion

    #region Inner Method

    protected override void PlayFunc()
    {
        base.PlayFunc();
        if (isStateChanged)
            onStateEnterExit.Invoke(true);
    }

    protected override void StopFunc()
    {
        base.StopFunc();
        if (isStateChanged)
            onStateEnterExit.Invoke(false);
    }

    #endregion

    #region Editor Method
#if UNITY_EDITOR

    //——MenuItem——
    static string instName = "CSB ";
    [UnityEditor.MenuItem(AC_EditorDefinition.HierarchyMenuPrefix_Cursor_Behaviour_State + "CursorStateBehaviour", false, 0)]
    public static void CreateActionEventPlayer_CursorStateBehaviour()
    {
        EditorTool.CreateGameObjectAsChild<AC_CursorStateBehaviour>(instName);
    }

    //——Hierarchy GUI——
    public override string ShortTypeName { get { return "CSB"; } }
    public override void SetHierarchyGUIProperty(StringBuilder sB)
    {
        base.SetHierarchyGUIProperty(sB);
        sbCache.Length = 0;
        if (cursorState == AC_CursorState.All)
        {
            sbCache.Append("All");
        }
        else if (cursorState != AC_CursorState.None)
        {
            sbCache.Append(cursorState);
        }
        AddSplit(sB, sbCache);
    }

    //——Inspector GUI——
    public override void SetInspectorGUIUnityEventProperty(GUIPropertyGroup group)
    {
        group.title = "Unity Event";
        group.listProperty.Add(new GUIProperty(nameof(onActiveDeactive)));

        group.listProperty.Add(new GUIProperty(nameof(onStateEnterExit), isEnable: cursorState != AC_CursorState.None));
    }
    public override void SetInspectorGUISubProperty(GUIPropertyGroup group)
    {
        base.SetInspectorGUISubProperty(group);
        group.listProperty.Add(new GUIProperty(nameof(cursorState)));
    }

#endif
    #endregion
}