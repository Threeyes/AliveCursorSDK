using System.Collections.Generic;
namespace Threeyes.UI
{
    /// <summary>
    /// 挂在指定物体上，UIContextMenuManager通过IContextMenuProvider接口即可获取到该组件的菜单信息
    /// </summary>
    public class ContextMenuProvider : ContextMenuProviderBase
    {
        public List<ToolStripItemInfo> curListItemInfo = new List<ToolStripItemInfo>();

        public void SetContextMenuInfo(List<ToolStripItemInfo> listItemInfo)
        {
            curListItemInfo = listItemInfo;
        }

        public override List<ToolStripItemInfo> GetContextMenuInfos()
        {
            return curListItemInfo;
        }
    }
}