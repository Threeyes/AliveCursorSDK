using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.UI
{
    /// <summary>
    /// 标记继承该接口的组件可以触发下拉菜单
    /// 用途：
    /// -方便Raycast检测
    /// </summary>
    public interface IContextMenuTrigger
    {
    }

    /// <summary>
    /// 返回下拉菜单信息
    /// </summary>
    public interface IContextMenuProvider : IContextMenuTrigger
    {
        /// <summary>
        /// 自定义对应的下拉菜单，方便Manager做多语言
        /// </summary>
        /// <returns></returns>
        List<ToolStripItemInfo> GetContextMenuInfo();
    }
}