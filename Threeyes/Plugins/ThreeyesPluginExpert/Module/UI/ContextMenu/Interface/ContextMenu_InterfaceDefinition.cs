using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.UI
{
    ///命名规范：
    ///-Provider和 Resolver 的区别（https://stackoverflow.com/questions/73186774/what-is-the-difference-between-provider-and-resolver）：
    ///     -Provider是创建并提供内容
    ///     -Resolver是基于传入的数据，获取数据内特定的内容（即对传入数据做解析）（常见与Manager需要实现的接口）

    //——Trigger——

    /// <summary>
    /// 标记继承该接口的组件可以触发下拉菜单
    /// 用途：
    /// -方便Raycast检测
    /// -由对应的Manager生成右键菜单信息，适用于涉及多语言的情况
    /// </summary>
    public interface IContextMenuTrigger { }

    /// <summary>
    /// 返回下拉菜单信息
    /// </summary>
    public interface IContextMenuProvider : IContextMenuTrigger
    {
        /// <summary>
        /// 自定义对应的下拉菜单，方便Manager做多语言
        /// </summary>
        /// <returns></returns>
        List<ToolStripItemInfo> GetContextMenuInfos();
    }

    /// <summary>
    /// 返回真正自定义Trigger的物体
    /// 
    /// 用途：
    /// -目标重定位，比如通过UI链接到真正包含右键菜单的物体
    /// </summary>
    public interface IContextMenuTarget : IContextMenuTrigger
    {
        GameObject GetTarget();
    }
    /// <summary>
    /// 返回指向目标的Trigger
    /// 
    /// 用途：
    /// -目标重定位，比如通过UI链接到真正包含右键菜单的物体
    /// 
    /// Todo：
    /// -可以选择是返回gameObject，还是返回List<IContextMenuTrigger>(可以通过ContextMenuManager提供的，通过gameObject获取其IContextMenuTrigger的静态方法)
    /// </summary>
    public interface IContextMenuTriggerProvider
    {
        List<IContextMenuTrigger> GetIContextMenuTriggers();//目标可能有多个Trigger，所以返回List
    }

    //——Resolver (For Manager)——
    ///PS：
    ///-以下接口不提炼通用基接口，是因为Manager不一定继承Component，不能通过GetComponents获取其所有接口


    /// <summary>
    /// Provide content menu for giving element
    /// </summary>
    public interface IContextMenuResolver
    {
        /// <summary>
        /// Resolves Menus for a given object.
        /// 
        /// PS:命名参考Newtown.Json.IContractResolver
        /// 
        /// </summary>
        /// <returns></returns>
        List<ToolStripItemInfo> CreateContextMenuInfos(List<IContextMenuTrigger> contextMenuTriggers);
    }

    /// <summary>
    /// Provide Global content menu (Only activate  when there is no valid IContextMenu Resolver)
    /// </summary>
    public interface IGlobalContextMenuResolver
    {
        /// <summary>
        /// Resolves Menus for a given object.
        /// </summary>
        /// <returns></returns>
        List<ToolStripItemInfo> CreateGlobalContextMenuInfos();
    }
}