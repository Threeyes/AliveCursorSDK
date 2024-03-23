using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Threeyes.UI
{
    [Serializable]
    /// <summary>
    /// 菜单栏最基础元件
    /// 
    /// Represents the abstract base class that manages events and layout for all the elements that a ToolStrip or ToolStripDropDown can contain.
    /// 
    ///ToAdd:
    ///-int Order(方便排序和归类，参考EventPlayer的Hierarchy菜单)
    ///
    /// PS:
    /// -因为是单独把数据抽出来，因此要在类名后增加Info
    /// Ref: [ToolStripItem]https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.toolstripitem?view=netframework-4.8
    /// </summary>
    public class ToolStripItemInfo
    {
        public string ToolTipText { get { return toolTipText; } set { toolTipText = value; } }
        public Texture Texture { get { return texture; } set { texture = value; } }//代替Image
        public string Text { get { return text; } set { text = value; } }//显示内容

        /// <summary>
        /// The order by which the menu items are displayed.
        /// Ref:https://docs.unity3d.com/ScriptReference/MenuItem-ctor.html
        /// 
        /// 功能：
        /// - If the integer value is greater than the other values, then the MenuItem script function is placed at the bottom of the list. 
        ///- The value of priority can also be used to manage the list of script functions into groups.（如果设置为相同值则归在一起）
        ///- When a priority argument is separated by more than 10, a separator line is created between two entries.（两个值之间相隔大于10，则使用分隔符分开）
        /// </summary>
        public int Priority { get { return priority; } set { priority = value; } }

        public event EventHandler Click { add { click += value; } remove { click -= value; } }
        EventHandler click;
        [SerializeField] UnityEvent onSelect = new UnityEvent();//用于可视化UI
        [SerializeField] private string toolTipText;
        [SerializeField] private Texture texture;
        [SerializeField] private string text;
        [SerializeField] private int priority;

        /// <summary>
        /// 
        /// PS：两个参数的值暂为空，请勿使用！
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void FireEvent(object sender, EventArgs e)
        {
            if (click != null)
                click.Invoke(sender, e);
            onSelect?.Invoke();
        }
        public ToolStripItemInfo()
        {
        }

        /// <summary>
        /// 单层菜单
        /// </summary>
        /// <param name="text"></param>
        /// <param name="onClick"></param>
        /// <param name="texture"></param>
        /// <param name="toolTipText"></param>
        public ToolStripItemInfo(string text, EventHandler onClick = null, int priority = 0, Texture texture = null, string toolTipText = null)
        {
            Text = text;
            if (onClick != null)
            {
                Click += onClick;
            }
            Priority = priority;
            Texture = texture;
            ToolTipText = toolTipText;
        }
    }

    /// <summary>
    /// 代表分割线
    /// 
    /// Ref: [ToolStripSeparator](https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.toolstripseparator?view=windowsdesktop-7.0)
    /// </summary>
    [Serializable]
    public class ToolStripSeparatorInfo : ToolStripItemInfo
    {
    }

}