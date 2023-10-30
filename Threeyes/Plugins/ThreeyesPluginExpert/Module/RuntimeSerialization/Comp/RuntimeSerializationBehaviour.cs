using Newtonsoft.Json;
using Threeyes.Data;
using UnityEngine;
namespace Threeyes.RuntimeSerialization
{
    public interface IRuntimeSerializationBehaviour
    {
        public System.Type ContainerType { get; }

        //不一定需要通过PropertyBag来保存，方便自定义数据类
        //public Object BaseContainer { get; }
        //public PropertyBag BaseSerializePropertyBag { get; }

        public string OnSerialize();
        public void OnDeserialize(string content);
        //public void OnDeserializeBase(PropertyBag basePropertyBag);
    }

    /// <summary>
    /// Mark for Component
    /// </summary>
    public interface IRuntimeSerializationComponent : IRuntimeSerializationBehaviour { }

    /// <summary>
    /// 
    /// 原理：
    /// -参考NetworkBehaviour，只有挂载该组件的物体才会进行序列化
    /// Ref:
    /// -
    /// -GILES.pb_Scene...
    /// </summary>
    public abstract class RuntimeSerializationBehaviour : MonoBehaviour, IRuntimeSerializationBehaviour
    {
        public abstract System.Type ContainerType { get; }
        //public abstract Object BaseContainer { get; }
        //public abstract PropertyBag BaseSerializePropertyBag { get; }

        public abstract string OnSerialize();
        public abstract void OnDeserialize(string content);
        //public abstract void OnDeserializeBase(PropertyBag basePropertyBag);

        #region Test
        //[Multiline]
        //public string testSer;
        //public void TestSerializeDeserialize(bool isSer)
        //{
        //    if (isSer)
        //        TestSerialize();
        //    else
        //        TestDeserialize();
        //}
        //[ContextMenu("TestSerialize")]
        //public void TestSerialize()
        //{
        //    testSer = OnSerialize();
        //}
        //[ContextMenu("TestDeserialize")]
        //public void TestDeserialize()
        //{
        //    OnDeserialize(testSer);
        //}
        #endregion
    }

    public abstract class RuntimeSerializationBehaviour<TContainer, TPropertyBag> : RuntimeSerializationBehaviour
    where TContainer : Object
    where TPropertyBag : UnityObjectPropertyBag<TContainer>, new()
    {
        ///ToDo:
        ///-参考RTSS的实现，如果是自定义Item，可以自行实现（如直接定义Data并序列化）
        ///-Transform等组件待序列化字段参考：Unity.RuntimeSceneSerialization.Internal.TransformPropertyBagDefinition
        ///     -【需要注意的是，此类中反射PropertyBagDefinition在Unity初始化的时候就已经注册，可以直接使用，不需要重新定义】

        public override System.Type ContainerType { get { return typeof(TContainer); } }
        //public override Object BaseContainer { get { return Container; } }
        //public override PropertyBag BaseSerializePropertyBag { get { return GetSerializePropertyBag(); } }
        public virtual TContainer Container
        {
            get
            {
                if (!container)
                    container = GetContainerFunc();
                return container;
            }
            set
            {
                container = value;
            }
        }
        /// <summary>
        /// Json Format
        /// 
        /// PS:
        /// -因为Component是以string的形式保存，因此使用None可避免出现多余的转义字符方便阅读
        /// -GameObject是直接使用PD进行保存，所以不需要
        /// </summary>
        protected virtual Formatting formatting { get { return Formatting.None; } }

        protected TContainer container;//待绑定的容器
        protected abstract TContainer GetContainerFunc();
        public override string OnSerialize()
        {
            TPropertyBag propertyBag = GetSerializePropertyBag();
            return JsonConvert.SerializeObject(propertyBag, /*RuntimeSerializationTool.DefaultFormatting*/formatting, RuntimeSerializationTool.DefaultJsonSerializerSettings);
        }
        public virtual TPropertyBag GetSerializePropertyBag()
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

        public override void OnDeserialize(string content)
        {
            if (Container == null)
            {
                Debug.LogError($"Require [{typeof(TContainer)}] on GameObject [{gameObject}]!");
                return;
            }
            TPropertyBag propertyBag = default(TPropertyBag);
            if (content.NotNullOrEmpty())
            {
                propertyBag = DeserializePropertyBag(content);
            }
            OnDeserializeRaw(propertyBag);
        }

        //public override void OnDeserializeBase(PropertyBag basePropertyBag)
        //{
        //    if(basePropertyBag is TPropertyBag realPropertyBag)
        //    {
        //        OnDeserialize(realPropertyBag);
        //    }
        //}
        public virtual void OnDeserializeRaw(TPropertyBag propertyBag)
        {
            TContainer cotainer = Container;
            propertyBag?.Accept(ref cotainer);
        }

        public virtual TPropertyBag DeserializePropertyBag(string content)
        {
            return JsonConvert.DeserializeObject<TPropertyBag>(content);
        }
    }

    public interface IRuntimeSerializationComponent<TContainer, TPropertyBag> : IRuntimeSerializationComponent
      where TContainer : Component
        where TPropertyBag : ComponentPropertyBag<TContainer>, new()
    {
    }

    /// <summary>
    /// 针对特定组件进行序列化
    /// </summary>
    public abstract class RuntimeSerializationComponent<TContainer, TPropertyBag> : RuntimeSerializationBehaviour<TContainer, TPropertyBag>, IRuntimeSerializationComponent<TContainer, TPropertyBag>
    where TContainer : Component
    where TPropertyBag : ComponentPropertyBag<TContainer>, new()
    {
        protected override TContainer GetContainerFunc()
        {
            if (this)//避免物体被销毁
                return GetComponent<TContainer>();

            return default(TContainer);
        }
    }
}