using UnityEngine;
using Threeyes.Data;

namespace Threeyes.RuntimeSerialization
{
    /// <summary>
    /// 辅助继承UnityObject的类实例（GameObject或Component）进行 序列化/反序列化
    /// </summary>
    /// <typeparam name="TContainer"></typeparam>
    /// <typeparam name="TPropertyBag"></typeparam>
    public abstract partial class RuntimeSerializableUnityObject<TContainer, TPropertyBag> : RuntimeSerializableObject
    where TContainer : Object
    where TPropertyBag : UnityObjectPropertyBag<TContainer>, new()//必须要有new()，否则创建时会报错
    {
        ///ToDo:
        ///-参考RTSS的实现，如果是自定义Item，可以自行实现（如直接定义Data并序列化）
        ///-Transform等组件待序列化字段参考：Unity.RuntimeSceneSerialization.Internal.TransformPropertyBagDefinition
        ///     -【需要注意的是，此类中反射PropertyBagDefinition在Unity初始化的时候就已经注册，可以直接使用，不需要重新定义】

        public override System.Type ContainerType { get { return typeof(TContainer); } }
        //public override Object BaseContainer { get { return Container; } }
        public virtual TContainer Container
        {
            get
            {
                if (!container)
                    container = GetContainerFunc();
                return container;
            }
        }

        protected TContainer container;//待绑定的容器
        protected abstract TContainer GetContainerFunc();


        /// <summary>
        /// 将实例转成PropertyBag 
        /// </summary>
        /// <returns></returns>
        public virtual TPropertyBag GetPropertyBag()
        {
            //PS:泛型构造函数只能调用无参，只能通过以下方式初始化
            TPropertyBag propertyBag = new TPropertyBag();
            if (Container == null)
            {
                Debug.LogError($"Require [{typeof(TContainer)}] on GameObject [{gameObject}]!");
            }
            else
            {
                propertyBag.Init(Container);
            }
            return propertyBag;
        }
    }

    #region
    [System.Serializable]
    public class UnityObjectPropertyBag<TContainer> : PropertyBag<TContainer>
        where TContainer : UnityEngine.Object
    {
        public HideFlags hideFlags;

        public override void Init(TContainer container)
        {
            base.Init(container);
            hideFlags = container.hideFlags;
        }
        public override void Accept(ref TContainer container)
        {
            base.Accept(ref container);
            container.hideFlags = hideFlags;
        }
    }
    #endregion
}