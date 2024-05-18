using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.RuntimeEditor
{
    /// <summary>
    /// 获取RuntimeHierarchy自定义的信息
    /// </summary>
    public interface IRuntimeHierarchyItemProvider
    {
        /// <summary>
        /// PS:
        /// -只设置需要重载的信息，其他字段设置为默认值即可
        /// -text：对应Title
        /// -Tooltip：如果不为空，
        /// 
        /// ToUpdate:
        /// -可以在GUIContent的基础上，定义一个更复杂的类
        /// </summary>
        /// <returns></returns>
        RuntimeHierarchyItemInfo GetRuntimeHierarchyItemInfo();
    }

    public class RuntimeHierarchyItemInfo
    {
        public string text;//主要显示文字（如果为空，则显示该物体名称）
        //public string tooltip;//悬浮文字上方后显示的内容（暂不需要，后期可激活）
        public string warningTips;//在右侧显示一个感叹号，悬浮显示文字（如果有多个，则叠加）

        public RuntimeHierarchyItemInfo()
        {
        }

        public RuntimeHierarchyItemInfo(string text = "", string warningTips = "")
        {
            this.text = text;
            this.warningTips = warningTips;
        }
    }
}