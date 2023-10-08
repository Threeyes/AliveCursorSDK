using UnityEngine;
using NaughtyAttributes;
using Threeyes.Action;

/// <summary>
/// Manage all Input Action
/// </summary>
[CreateAssetMenu(menuName = AC_EditorDefinition.AssetMenuPrefix_Root_Mod_Behaviour_Input + "ActionCollection", fileName = "CursorInputActionCollection", order = 0)]
public class AC_SOCursorInputActionCollection : SOCollectionBase<AC_CursorInputType, SOActionBase>
{
    [Expandable]
    public SOActionBase soActionLeftButton;
    [Expandable]
    public SOActionBase soActionRightButton;
    [Expandable]
    public SOActionBase soActionMiddleButton;
    [Expandable]
    public SOActionBase soActionXButton1;
    [Expandable]
    public SOActionBase soActionXButton2;

    [Expandable]
    public SOActionBase soActionWheelScroll;

    [Expandable]
    public SOActionBase soActionMove;
    [Expandable]
    public SOActionBase soActionDrag;

    public override SOActionBase this[AC_CursorInputType en]
    {
        get
        {
            switch (en) //PS:switch比反射快
			{
				case AC_CursorInputType.LeftButtonDownUp:
                    return soActionLeftButton;
                case AC_CursorInputType.RightButtonDownUp:
                    return soActionRightButton;
                case AC_CursorInputType.MiddleButtonDownUp:
                    return soActionMiddleButton;
                case AC_CursorInputType.XButton1DownUp:
                    return soActionXButton1;
                case AC_CursorInputType.XButton2DownUp:
                    return soActionXButton2;

                case AC_CursorInputType.WheelScroll:
                    return soActionWheelScroll;

                case AC_CursorInputType.Move:
                    return soActionMove;
                case AC_CursorInputType.Drag:
                    return soActionDrag;

                default:
                    Debug.LogError(en + " Not Define!");
                    return null;
            }
        }
    }
}
