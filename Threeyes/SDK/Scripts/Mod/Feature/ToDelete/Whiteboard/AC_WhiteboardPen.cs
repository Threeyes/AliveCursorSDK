using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Threeyes.Steamworks;
/// <summary>
/// Ref: https://git.fh-aachen.de/MakeItTrue2/VR/-/blob/ea8b3e6728db29fd229603b12b3a5f88c315fa54/unity-game/Assets/Scripts/WhiteBoard/WhiteboardPen.cs
/// 
/// Todo:
/// -增加笔刷样式（实现方式：调用whiteboard.SetPenColor设置笔刷块颜色时，要传入对应样式的矢量图或者矢量图计算公式（通常为Alpha通道图），将样式的每个像素点与笔刷的颜色相乘，得到的就是笔刷形状。绘制时要和原像素混合。提供保存图像的功能）
/// </summary>
[System.Obsolete("Use the class without AC_ prefix instead!", true)]
public class AC_WhiteboardPen : WhiteboardPen { }