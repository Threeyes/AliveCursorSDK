using Threeyes.Data;
using UnityEngine;
using Newtonsoft.Json;
namespace Threeyes.RuntimeSerialization
{
    /// <summary>
    /// 可序列化的UnityObject类
    /// </summary>
    public interface IRuntimeSerializableObject : IRuntimeSerializable, IIdentityHolder { }

    /// <summary>
    /// 辅助指定Object（GameObject或组件）进行 序列化/反序列化
    /// 
    /// 原理：
    /// -参考 Mirror.NetworkBehaviour，只有挂载该组件的物体才会进行序列化
    /// Ref:GILES.pb_Scene...
    /// </summary>
    public abstract class RuntimeSerializableObject : MonoBehaviour, IRuntimeSerializableObject
    {
        public abstract Identity ID { get; set; }//UID，用于反序列化时匹配。参考RTSGO，避免同级有多个相同组件，导致序列化失败。
        public abstract System.Type ContainerType { get; }
        //public abstract Object BaseContainer { get; }
        //public abstract PropertyBag BasePropertyBag { get; }

        public abstract string Serialize();
        public abstract void Deserialize(string content, IDeserializationOption baseOption = null);
        public abstract void DeserializeBase(IComponentPropertyBag basePropertyBag, IDeserializationOption baseOption = null);

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

    /// <summary>
    /// 辅助继承UnityObject的类实例（GameObject或Component）进行 序列化/反序列化
    /// </summary>
    /// <typeparam name="TContainer"></typeparam>
    /// <typeparam name="TPropertyBag"></typeparam>
    public abstract class RuntimeSerializableUnityObject<TContainer, TPropertyBag> : RuntimeSerializableObject
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
        /// <summary>
        /// Json Format
        /// 
        /// PS:
        /// -Component：以string的形式保存，因此使用None可避免出现多余的转义字符，方便阅读
        /// </summary>
        protected virtual Formatting Formatting { get { return Formatting.None; } }

        protected TContainer container;//待绑定的容器
        protected abstract TContainer GetContainerFunc();
        public override string Serialize()
        {
            TPropertyBag propertyBag = GetPropertyBag();
            return RuntimeSerializationTool.SerializeObject(propertyBag, Formatting);
        }

        public override void Deserialize(string content, IDeserializationOption baseOption = null)
        {
            if (Container == null)
            {
                Debug.LogError($"Deserialize Require [{typeof(TContainer)}] on GameObject [{gameObject}]!");
                return;
            }
            TPropertyBag propertyBag = default(TPropertyBag);
            if (content.NotNullOrEmpty())
            {
                propertyBag = DeserializePropertyBag(content);
                DeserializeFunc(propertyBag, baseOption);
            }
            else
            {
                Debug.LogError($"Deserialization Content for {ContainerType} is null!");
            }
        }

        public override void DeserializeBase(IComponentPropertyBag basePropertyBag, IDeserializationOption baseOption = null)
        {
            if (basePropertyBag is TPropertyBag realPropertyBag)
                DeserializeFunc(realPropertyBag, baseOption);
        }
        protected virtual void DeserializeFunc(TPropertyBag propertyBag, IDeserializationOption baseOption = null)
        {
            TContainer cotainer = Container;
            propertyBag?.Accept(ref cotainer);
        }

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
        /// <summary>
        /// 将序列化后的内容转成PropertyBag
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        protected virtual TPropertyBag DeserializePropertyBag(string content)
        {
            return RuntimeSerializationTool.DeserializeObject<TPropertyBag>(content);
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