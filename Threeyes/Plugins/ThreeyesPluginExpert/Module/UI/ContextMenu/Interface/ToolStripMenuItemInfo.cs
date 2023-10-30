
using System;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.UI
{
    /// <summary>
    /// 存储一系列下拉框
    /// Provides basic functionality for controls that display a ToolStripDropDown when a ToolStripDropDownButton, ToolStripMenuItem, or ToolStripSplitButton control is clicked.
    /// Ref：
    /// - https://learn.microsoft.com/en-us/dotnet/desktop/winforms/controls/toolstrip-control-architecture?view=netframeworkdesktop-4.8
    /// - https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.toolstripdropdownitem?view=windowsdesktop-7.0
    /// </summary>
    public class ToolStripDropDownItemInfo : ToolStripItemInfo
    {
        public virtual bool HasDropDownItemInfo { get { return listDropDownItemInfo.Count > 0; } }

        public List<ToolStripItemInfo> ListDropDownItemInfo { get { return listDropDownItemInfo; } set { listDropDownItemInfo = value; } }
        private List<ToolStripItemInfo> listDropDownItemInfo = new List<ToolStripItemInfo>();//PS:不能序列化，否则会出现无穷显示的问题（后期可通过特殊方式保存）


        public ToolStripDropDownItemInfo()
        {
        }
        public ToolStripDropDownItemInfo(string text, EventHandler onClick = null, int priority = 0, Texture texture = null, string toolTipText = null)
            : base(text, onClick, priority, texture, toolTipText)
        {
        }
        public ToolStripDropDownItemInfo(string text, ToolStripItemInfo[] dropDownItems, int priority = 0, Texture texture = null)
         : this(text, onClick: null, priority, texture)
        {
            if (dropDownItems != null)
                ListDropDownItemInfo.AddRange(dropDownItems);
        }
    }

    /// <summary>
    /// 包含子菜单的元件
    /// 
    /// Represents a selectable option displayed on a MenuStrip or ContextMenuStrip.
    /// Ref: [ToolStripMenuItem] https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.toolstripmenuitem?view=netframework-4.8
    /// 
    /// PS：
    /// -单独成为一个数据类，方便独立提供给Modder使用
    /// </summary>
    public class ToolStripMenuItemInfo : ToolStripDropDownItemInfo
    {
        public ToolStripMenuItemInfo()
        {
        }

        public ToolStripMenuItemInfo(string text, EventHandler onClick = null, int priority = 0, Texture texture = null, string toolTipText = null) : base(text, onClick, priority, texture, toolTipText)
        {
        }
        public ToolStripMenuItemInfo(string text, ToolStripItemInfo[] dropDownItems, int priority = 0, Texture texture = null)
       : base(text, dropDownItems, priority, texture)
        {
        }
    }
}