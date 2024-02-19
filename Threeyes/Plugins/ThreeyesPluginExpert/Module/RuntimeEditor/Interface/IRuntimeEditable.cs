using System;
using System.Collections.Generic;
using Threeyes.Data;
using Threeyes.UI;

namespace Threeyes.RuntimeEditor
{
    /// <summaryde>
    /// Mark an class as Editable on Runtime
    /// 
    /// ToUpdate:
    /// -添加自定义的下拉菜单方法
    /// </summary>
    public interface IRuntimeEditable : IFilePathModifierHolder
    {
        ///// <summary>
        ///// 用途：RuntimeEdit时，设置对应路径
        ///// </summary>
        //FilePathModifier FilePathModifier { get; set; }

        /// <summary>
        /// 便于动态返回是否可以编辑的状态
        /// </summary>
        bool IsRuntimeEditable { get; }

        /// <summary>
        /// [Optional] Display name, will be type name if null
        /// </summary>
        string RuntimeEditableDisplayName { get; }

        /// <summary>
        /// 初始化IRuntimeEditable相关字段，以及对应的数据
        /// </summary>
        /// <param name="filePathModifier"></param>
        void InitRuntimeEditable(FilePathModifier filePathModifier);

        /// <summary>
        /// 返回需要序列化指定实例的Member信息清单，用于后续运行时编辑数据
        /// 
        /// PS:
        /// -因为Modder无法使用Reflection，所以需要[nameof(XXX)]返回成员的名称
        /// -可以通过返回其他相关联组件（如- URP相机的UniversalAdditionalCameraData）的Member，实现在一个组件中统一管理其他的数据
        /// </summary>
        /// <returns></returns>
        List<RuntimeEditableMemberInfo> GetRuntimeEditableMemberInfos();
    }

    /// <summary>
    /// Info for member
    /// </summary>
    public class RuntimeEditableMemberInfo
    {
        public object obj;//The object whose member value will be returned. For non-static fields, obj should be an instance of a class that inherits or declares the field.（静态类可空）
        public Type objType;//Type of obj
        public string memberName;//name of member
        public string displayName = null;//[Optional] Display name
        public List<ToolStripItemInfo> listContextItemInfo = new List<ToolStripItemInfo>();//[Optional] Contextmenu for this member
        public RuntimeEditableMemberInfo() { }

        public RuntimeEditableMemberInfo(object obj, Type objType, string memberName, string displayName = null, List<ToolStripItemInfo> listContextItemInfo = null)
        {
            this.obj = obj;
            this.objType = objType;
            this.memberName = memberName;
            this.displayName = displayName;
            this.listContextItemInfo = listContextItemInfo;
        }
    }
}