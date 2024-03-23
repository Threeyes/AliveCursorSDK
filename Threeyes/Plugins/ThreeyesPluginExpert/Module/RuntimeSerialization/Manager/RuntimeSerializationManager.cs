using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Threeyes.Core;
namespace Threeyes.RuntimeSerialization
{
    /// <summary>
    /// 管理统一的序列化配置配置
    /// </summary>
    public class RuntimeSerializationManager : InstanceBase<RuntimeSerializationManager>
    {
        /// <summary>
        /// 
        /// ToUpdate:
        /// -为其添加一个MissingInfo组件（不需要持久化，仅用于提供可识别来源的信息）（可以在RuntimeEditor上查看详细信息）（主要是查看RS_GO里保存的）
        /// </summary>
        public GameObject preMissingPrefabDummy;//代替丢失prefab的占位物体，通常仅显示，不包含任何有效组件或序列化组件。如果需要可被选中，可以加上RuntimeEditorSelectable组件
    }
}