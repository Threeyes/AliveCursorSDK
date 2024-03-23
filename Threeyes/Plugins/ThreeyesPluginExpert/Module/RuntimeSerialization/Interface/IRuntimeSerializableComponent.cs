using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.RuntimeSerialization
{
    /// <summary>
    /// Mark Component as Serializable
    /// 
    /// PS：
    /// -自定义RuntimeSerialization的组件需要继承此接口
    /// </summary>
    public interface IRuntimeSerializableComponent : IRuntimeSerializableObject
    {
        public IComponentPropertyBag ComponentPropertyBag { get; }
    }

    public interface IRuntimeSerializableComponent<TContainer, TPropertyBag> : IRuntimeSerializableComponent
      where TContainer : Component
        where TPropertyBag : ComponentPropertyBag<TContainer>, new()
    { }
}