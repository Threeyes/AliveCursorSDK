using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// 功能：更改窗口尺寸
/// 使用方法：挂在窗口的对应拖拽UI元素中，然后panelRectTransform引用窗口组件
/// 注意：
/// 1.缩放基于Panel的Pivot（eg：如需要基于左上角缩放，则把Pivot设置为（0，1））
/// </summary>
[System.Obsolete("Use the class without AC_ prefix instead!", true)]
public class AC_UIResizePanel : UIResizePanel
{
    ///ToAdd：
    ///-变换屏幕后刷新
}