using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Threeyes.Core;
using Threeyes.GameFramework;
using UnityEngine;
using UnityEngine.Events;

#region State

[System.Serializable]
/// <summary>
/// State for cursor
/// </summary>
public class AC_CursorStateInfo : DataObjectBase
{
    public AC_CursorState cursorState;
    public StateChange stateChange;

    public AC_CursorStateInfo(AC_CursorState cursorState, StateChange state)
    {
        this.cursorState = cursorState;
        this.stateChange = state;
    }

    /// <summary>
    /// Enumeration specifying a change in CursorState state
    /// </summary>
    public enum StateChange
    {
        None = 0,

        Enter = 1 << 0,//正在进入
        //Update = 1 << 1,//PS：仅用于状态判断（ToUpdate：Update应该是一个单独的状态，不要跟Enter、Exit放一起
        Exit = 1 << 2,//正在退出

        All = ~0
    }

    public override string ToString()
    {
        return $"{stateChange}: {cursorState}";
    }
}

//（PS：因为action出错容易导致程序卡死，因此仅供Manager类使用）
public class AC_CursorStateInfoEx : AC_CursorStateInfo
{
    public UnityAction actOnComplete;
    public AC_CursorStateInfoEx(AC_CursorState cursorState, StateChange state, UnityAction actOnComplete = null) : base(cursorState, state)
    {
        this.actOnComplete = actOnComplete;
    }
}
#endregion

#region SystemCursor
public class AC_SystemCursorAppearanceInfo
{
    public static readonly AC_SystemCursorAppearanceInfo None = new AC_SystemCursorAppearanceInfo() { systemCursorAppearanceType = AC_SystemCursorAppearanceType.None };

    public AC_SystemCursorAppearanceType systemCursorAppearanceType;//光标类型

    //Runtime
    public StateChange stateChange;

    public AC_SystemCursorAppearanceInfo()
    {
    }

    public AC_SystemCursorAppearanceInfo(AC_SystemCursorAppearanceType systemCursorAppearanceType)
    {
        this.systemCursorAppearanceType = systemCursorAppearanceType;
    }

    public AC_SystemCursorAppearanceInfo(AC_SystemCursorAppearanceInfo other)
    {
        this.systemCursorAppearanceType = other.systemCursorAppearanceType;
        this.stateChange = other.stateChange;
    }


    //ToAdd:增加对应光标的Texture2D，方便用户使用

    #region Compare
    public override bool Equals(object obj)
    {
        if (obj is AC_SystemCursorAppearanceInfo other)
        {
            return other.systemCursorAppearanceType == systemCursorAppearanceType;
        }
        return false;
        //return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static bool operator ==(AC_SystemCursorAppearanceInfo x, AC_SystemCursorAppearanceInfo y)
    {
        //可避免空引用：  https://stackoverflow.com/questions/4219261/overriding-operator-how-to-compare-to-null
        if (ReferenceEquals(x, null))
        {
            return ReferenceEquals(y, null);
        }

        return x.Equals(y);
    }

    public static bool operator !=(AC_SystemCursorAppearanceInfo x, AC_SystemCursorAppearanceInfo y)
    {
        return !x == y;
    }

    public static implicit operator bool(AC_SystemCursorAppearanceInfo exists)
    {
        return !(exists == null);
    }
    #endregion

    /// <summary>
    /// Enumeration specifying a change in CursorAppearance state
    /// </summary>
    public enum StateChange
    {
        None = 0,

        Enter = 1 << 0,
        //Update = 1 << 1,//PS：仅用于状态判断（ToUpdate：Update应该是一个单独的状态，不要跟Enter、Exit放一起
        Exit = 1 << 2,

        All = ~0
    }
}

#endregion

#region SystemInput

/// <summary>
/// Provides data for the Mouse events.
///
/// Ref:System.Windows.Forms.MouseEventArgs
/// </summary>
public class AC_MouseEventArgs : EventArgs
{
    public const int MouseWheelScrollDelta = 120;//Ref: SystemInformation.MouseWheelScrollDelta

    private readonly AC_MouseButtons button;

    private readonly int clicks;

    private readonly int x;

    private readonly int y;

    private readonly int delta;

    /// <summary>
    /// Gets which mouse button was pressed.
    /// </summary>
    public AC_MouseButtons Button => button;

    /// <summary>
    /// Gets the number of times the mouse button was pressed and released.
    /// </summary>
    public int Clicks => clicks;

    /// <summary>
    /// Gets the x-coordinate of the mouse during the generating mouse event.
    /// </summary>
    public int X => x;

    /// <summary>
    /// Gets the y-coordinate of the mouse during the generating mouse event.
    /// </summary>
    public int Y => y;

    /// <summary>
    /// Gets a signed count of the number of detents the mouse wheel has rotated, multiplied by the WHEEL_DELTA constant. A detent is one notch of the mouse wheel.
    /// </summary>
    public int Delta => delta;

    /// <summary>
    /// Gets the location of the mouse during the generating mouse event.
    /// A Point that contains the x- and y- mouse coordinates, in pixels, relative to the upper-left corner of the control
    /// [basic on the main display (eg: in multi-display mode, the value may be less than 0)]
    /// </summary>
    public AC_Point Location => new AC_Point(x, y);

    //——[Custom】——

    /// <summary>
    /// Mouse scroll value, the result = (1 for scroll up or -1 for scroll down) * scroll frequency
    /// 
    /// PS:针对普通（非高精度）鼠标，每次滚动的单位值为1，快速滚动可达2及以上
    /// </summary>
    public float DeltaScroll
    {
        //注意：正常鼠标滚轴每次滚动的幅度为15度，对应值是120f。高精度光标滚动幅度会变小，返回值也会相应变小。
        //Most mouse types work in steps of 15 degrees, in which case the delta value is a multiple of 120; i.e., 120 units * 1/8 = 15 degrees.
        //A positive value indicates that the wheel was rotated forward (away from the user); a negative value indicates that the wheel was rotated backward (toward the user).
        //https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.mouseeventargs.delta?view=windowsdesktop-5.0
        //https://stackoverflow.com/questions/7753123/why-is-wheeldelta-120
        //https://devblogs.microsoft.com/oldnewthing/20130123-00/?p=5473
        get { return Delta / MouseWheelScrollDelta; }
    }

    /// <summary>
    /// The location delta since last change
    /// </summary>
    public AC_Point DeltaLocation;

    /// <summary>
    /// The location percentage since last change base on current Display（Divided by current display's size in pixels）
    /// </summary>
    public Vector2 DeltaLocationPercent { get { return new Vector2((float)DeltaLocation.X / Screen.width, (float)DeltaLocation.Y / Screen.height); } }

    public AC_MouseEventArgs(AC_MouseButtons button, int clicks, int x, int y, int delta)
    {
        this.button = button;
        this.clicks = clicks;
        this.x = x;
        this.y = y;
        this.delta = delta;
    }
}

/// <summary>
/// Provides extended data for the MouseClickExt and MouseMoveExt events.
/// </summary>
public class AC_MouseEventExtArgs : AC_MouseEventArgs
{
    //#ExtArg
    /// <summary>
    /// True if event contains information about wheel scroll.
    /// </summary>
    public bool WheelScrolled => Delta != 0;

    /// <summary>
    /// True if event signals a click. False if it was only a move or wheel scroll.
    /// </summary>
    public bool Clicked => Clicks > 0;

    /// <summary>
    /// True if event signals mouse button down.
    /// </summary>
    public bool IsMouseButtonDown { get; private set; }

    /// <summary>
    /// True if event signals mouse button up.
    /// </summary>
    public bool IsMouseButtonUp { get; private set; }

    /// <summary>
    /// True if event signals mouse drag started. (left button held down whilst moving more than the system drag threshold).
    /// </summary>
    public bool IsDragStart { get; set; }

    /// <summary>
    /// True if event signals mouse drag finished.
    /// </summary>
    public bool IsDragFinish { get; set; }

    /// <summary>
    /// Gets the time when this event occurred.
    /// </summary>
    public int Timestamp { get; private set; }

    public AC_MouseEventExtArgs(AC_MouseButtons buttons, int clicks, AC_Point point, int delta,
        //#ExtArg
        bool isMouseButtonDown = false,
        bool isMouseButtonUp = false,
        int timestamp = 0
        ) : base(buttons, clicks, point.X, point.Y, delta)
    {
        //#ExtArg
        IsMouseButtonDown = isMouseButtonDown;
        IsMouseButtonUp = isMouseButtonUp;

        Timestamp = timestamp;
    }
}

/// <summary>
/// The Point structure defines the X- and Y- coordinates of a point.(https://docs.microsoft.com/en-us/dotnet/api/system.drawing.point?view=net-6.0)
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct AC_Point
{
    //The Point structure defines the X- and Y- coordinates of a point.
    public int X { get; set; }
    //Specifies the Y-coordinate of the point.
    public int Y { get; set; }
    public AC_Point(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }
    public override string ToString()
    {
        return ToString(null, null);
    }
    public string ToString(string format)
    {
        return ToString(format, null);
    }

    public string ToString(string format, IFormatProvider formatProvider)
    {
        if (string.IsNullOrEmpty(format))
        {
            format = "F0";
        }

        if (formatProvider == null)
        {
            formatProvider = CultureInfo.InvariantCulture.NumberFormat;
        }

        return string.Format("({0}, {1})", X.ToString(format, formatProvider), Y.ToString(format, formatProvider));
    }
    public static AC_Point operator +(AC_Point a, AC_Point b)
    {
        return new AC_Point(a.X + b.X, a.Y + b.Y);
    }
    public static AC_Point operator -(AC_Point a, AC_Point b)
    {
        return new AC_Point(a.X - b.X, a.Y - b.Y);
    }
    public static AC_Point operator *(AC_Point a, AC_Point b)
    {
        return new AC_Point(a.X * b.X, a.Y * b.Y);
    }
    public static AC_Point operator /(AC_Point a, AC_Point b)
    {
        return new AC_Point(a.X / b.X, a.Y / b.Y);
    }
    public static AC_Point operator -(AC_Point a)
    {
        return new AC_Point(0 - a.X, 0 - a.Y);
    }
    public static AC_Point operator *(AC_Point a, int d)
    {
        return new AC_Point(a.X * d, a.Y * d);
    }
    public static AC_Point operator *(int d, AC_Point a)
    {
        return new AC_Point(a.X * d, a.Y * d);
    }
    public static AC_Point operator /(AC_Point a, int d)
    {
        return new AC_Point(a.X / d, a.Y / d);
    }
    public static bool operator ==(AC_Point a, AC_Point b)
    {
        return a.X == b.X && a.Y == b.Y;
    }
    public static bool operator !=(AC_Point a, AC_Point b)
    {
        return !(a == b);
    }
    public bool Equals(AC_Point other)
    {
        return other.X == X && other.Y == Y;
    }
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (obj.GetType() != typeof(AC_Point)) return false;
        return Equals((AC_Point)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (X * 397) ^ Y;
        }
    }

}

/// <summary>
///  Mouse button
/// （Ref: https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.mousebuttons?view=windowsdesktop-6.0）
/// </summary>
[System.Flags]
public enum AC_MouseButtons
{
    Left = 0x100000,//1048576
    None = 0x0,
    Right = 0x200000,//2097152
    Middle = 0x400000,
    XButton1 = 0x800000,//The first XButton (XBUTTON1) on Microsoft IntelliMouse Explorer was pressed. (Back Key)
    XButton2 = 0x1000000//The second XButton (XBUTTON2) on Microsoft IntelliMouse Explorer was pressed.(Forward Key)
}

#endregion

#region SystemWindow

/// <summary>
/// Provides data for the Window events.
/// </summary>
public class AC_WindowEventExtArgs : WindowEventExtArgs
{
    //ToAdd：独特的信息
    public AC_WindowEventExtArgs(Cause cause, StateChange stateChange) : base(cause, stateChange)
    {
    }
}
#endregion
