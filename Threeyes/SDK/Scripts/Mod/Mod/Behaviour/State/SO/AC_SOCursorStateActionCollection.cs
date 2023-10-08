using NaughtyAttributes;
using Threeyes.Action;
using UnityEngine;
/// <summary>
/// Manage all Cursor State Action
/// 
/// PS:
/// 此脚本可用于通用的更改光标跟随，从而节省大量脚本
/// 
/// Warning： 
/// 1.因为会影响整体光标的State，且渐变等需求非必要，暂时作为内部方法。用户有需要可以自行监听并实现对应方法
/// 2.SOAction要求：当前State.Exit要在一帧内执行完毕，或者是与下一State控制不同属性（如当前控制位置，下一控制缩放），避免影响下一State.Enter
/// </summary>
[CreateAssetMenu(menuName = AC_EditorDefinition.AssetMenuPrefix_Root_Mod_Behaviour_State + "ActionCollection", fileName = "CursorStateActionCollection")]
public class AC_SOCursorStateActionCollection : SOCollectionBase<AC_CursorState, SOActionBase>
{
    [Expandable]
    public SOActionBase soActionEnter;
    [Expandable]
    public SOActionBase soActionExit;

    [Expandable]
    public SOActionBase soActionShow;
    [Expandable]
    public SOActionBase soActionHide;

    [Expandable]
    public SOActionBase soActionWorking;
    [Expandable]
    public SOActionBase soActionStandBy;
    [Expandable]
    public SOActionBase soActionBored;//Use AC_SOAction_Empty by default, because AC is now free!

	public override SOActionBase this[AC_CursorState en]
    {
        get
        {
            switch (en)
            {
                case AC_CursorState.None:
                case AC_CursorState.All:
                    return null;
                case AC_CursorState.Enter:
                    return soActionEnter;
                case AC_CursorState.Exit:
                    return soActionExit;
                case AC_CursorState.Show:
                    return soActionShow;
                case AC_CursorState.Hide:
                    return soActionHide;
                case AC_CursorState.Working:
                    return soActionWorking;
                case AC_CursorState.StandBy:
                    return soActionStandBy;
                case AC_CursorState.Bored:
                    return soActionBored;
                default:
                    Debug.LogError(en + " Not Define!");
                    return null;
            }
        }
    }
}