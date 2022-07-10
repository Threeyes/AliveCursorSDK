using UnityEngine;

public static class AC_InputTool
{
    public static AC_CursorInputType ConvertToInputType(AC_MouseButtons mouseButtons)
    {
        switch (mouseButtons)
        {
            case AC_MouseButtons.Left:
                return AC_CursorInputType.LeftButtonDownUp;
            case AC_MouseButtons.Right:
                return AC_CursorInputType.RightButtonDownUp;
            case AC_MouseButtons.Middle:
                return AC_CursorInputType.MiddleButtonDownUp;
            case AC_MouseButtons.XButton1:
                return AC_CursorInputType.XButton1DownUp;
            case AC_MouseButtons.XButton2:
                return AC_CursorInputType.XButton2DownUp;
            default:
                Debug.LogError(mouseButtons + " Not Define!");
                return AC_CursorInputType.None;
        }
    }
}
