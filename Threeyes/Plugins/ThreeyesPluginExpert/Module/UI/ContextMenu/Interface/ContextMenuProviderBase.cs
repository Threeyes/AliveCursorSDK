using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.UI
{
    /// <summary>
    /// 提供下拉菜单信息
    /// </summary>
    public abstract class ContextMenuProviderBase : MonoBehaviour, IContextMenuProvider
    {
        public abstract List<ToolStripItemInfo> GetContextMenuInfos();
    }
}