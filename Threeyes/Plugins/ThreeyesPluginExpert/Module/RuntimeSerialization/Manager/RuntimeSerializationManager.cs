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
        public GameObject preMissingPrefabDummy;//代替丢失prefab的占位物体，通常仅显示，不包含任何有效组件或序列化组件。如果需要可被选中，可以加上RuntimeEditorSelectable组件
    }
}