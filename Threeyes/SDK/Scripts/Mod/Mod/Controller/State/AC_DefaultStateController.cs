using NaughtyAttributes;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using Threeyes.Action;
using Threeyes.Config;
using Threeyes.Core;
using Threeyes.Core.Editor;
using UnityEngine;
/// <summary>
/// Control cursor state action
/// </summary>
[AddComponentMenu(AC_EditorDefinition.ComponentMenuPrefix_Root_Mod_Controller + "AC_DefaultStateController")]
public class AC_DefaultStateController : AC_StateControllerBase<AC_SODefaultStateControllerConfig, AC_DefaultStateController.ConfigInfo>
{
    #region Property & Field
    ///PS：
    ///1.Manager 到Controller之间可以通过AC_CursorStateInfoEx进行传送，Manager与监听者只能通过AC_CursorStateInfo传送
    ///2.对应的Action只能更改场景中持续存在的实例的值，而且不同State控制的值也有要求，避免场景切换后导致控制丢失（如SystemCursorManager及TransformManager）【通过都是在Working状态下切换的，因此问题不大】
    ///3.保留GoActionTarget，便于通过Holder监听事件

    //控制单元的缩放值（Modder可自行更改为其他实现）
    GameObject GoActionTarget { get { return goActionTarget; } }
    [SerializeField] protected GameObject goActionTarget;

    //Runtime
    [SerializeField] protected AC_CursorState curCursorState = AC_CursorState.None;
    #endregion

    #region Callback
    public override void OnModControllerInit()
    {
        base.OnModControllerInit();
        curCursorState = AC_ManagerHolder.StateManager.CurCursorState;
        //ToAdd:调用SetState
    }
    #endregion

    #region Override

    public override void SetState(AC_CursorStateInfoEx cursorStateInfo)
    {
        var colloection = Config.soCursorStateActionCollection;
        if (!colloection)
            return;

        //Convert SMBFrameData into SOAction
        AC_CursorState cursorState = cursorStateInfo.cursorState;
        curCursorState = cursorState;
        var curSoAction = colloection[cursorState];

        AC_CursorStateInfo.StateChange smbState = cursorStateInfo.stateChange;
        if (smbState == AC_CursorStateInfo.StateChange.Enter)
            curSoAction?.Enter(true, goActionTarget, actOnComplete: cursorStateInfo.actOnComplete);
        else if (smbState == AC_CursorStateInfo.StateChange.Exit)
            curSoAction?.Enter(false, goActionTarget, actOnComplete: cursorStateInfo.actOnComplete);
    }
    public override bool IsCurStateActionComplete(ActionState actionState)
    {
        var colloection = Config.soCursorStateActionCollection;
        if (!colloection)
            return true;
        SOActionBase curSOAction = colloection[curCursorState];
        if (curSOAction == null)
            return true;

        object receiver;
        ObjectID objectID = curSOAction.GetObjectID(GoActionTarget, out receiver);
        return curSOAction.IsComplete(objectID, actionState);
    }


    #endregion

    #region Define
    [System.Serializable]
    public class ConfigInfo : SerializableDataBase
    {
        [JsonIgnore][Expandable] public AC_SOCursorStateActionCollection soCursorStateActionCollection;
    }
    #endregion

    #region Editor Method
#if UNITY_EDITOR
    //——MenuItem——
    static string instName = "DefaultStateController";
    [UnityEditor.MenuItem(AC_EditorDefinition.HierarchyMenuPrefix_Root_Mod_Controller_State + "Default", false)]
    public static void CreateInst()
    {
        EditorTool.CreateGameObjectAsChild<AC_DefaultStateController>(instName);
    }
#endif
    #endregion
}