using Threeyes.Data;
using UnityEngine;
namespace Threeyes.RuntimeSerialization
{
    /// <summary>
    /// 针对特定组件进行序列化
    /// </summary>
    [RequireComponent(typeof(RuntimeSerializable_GameObject))]
    public abstract partial class RuntimeSerializableComponent<TContainer, TPropertyBag> : RuntimeSerializableUnityObject<TContainer, TPropertyBag>, IRuntimeSerializableComponent<TContainer, TPropertyBag>
    where TContainer : Component
    where TPropertyBag : ComponentPropertyBag<TContainer>, new()
    {
        #region ID  标记该组件的唯一ID，便于绑定。
        public override Identity ID { get { return id; } set { } }//暂不允许设置
        [SerializeField] protected Identity id = new Identity();
        public virtual IComponentPropertyBag ComponentPropertyBag { get { return GetPropertyBag(); } }

#if UNITY_EDITOR
        void OnValidate() { RuntimeSerializationTool.EditorUpdateComponetID(this, ref id); }
#endif
        #endregion

        protected override TContainer GetContainerFunc()
        {
            if (this)//避免物体被销毁
                return GetComponent<TContainer>();

            return default(TContainer);
        }
    }

    public abstract class RuntimeSerializableBehaviour<TContainer, TPropertyBag> : RuntimeSerializableComponent<TContainer, TPropertyBag>
       where TContainer : Behaviour
        where TPropertyBag : BehaviourPropertyBag<TContainer>, new()
    { }

    public abstract class RuntimeSerializableMonoBehaviour<TContainer, TPropertyBag> : RuntimeSerializableBehaviour<TContainer, TPropertyBag>
   where TContainer : MonoBehaviour
    where TPropertyBag : MonoBehaviourPropertyBag<TContainer>, new()
    { }

    #region Define
    public interface IComponentPropertyBag : IPropertyBag, IIdentityHolder
    {
    }
    [System.Serializable]
    public class ComponentPropertyBag<TContainer> : UnityObjectPropertyBag<TContainer>, IComponentPropertyBag
       where TContainer : Component
    {
        //缓存唯一ID（PS：统一由RuntimeSerialization_GameObject管理，因为TContainer不一定包含ID）
        public Identity ID { get { return id; } set { id = value; } }
        public Identity id = new Identity();//如果为实例，则不为空
    }

    [System.Serializable]
    public class BehaviourPropertyBag<TContainer> : ComponentPropertyBag<TContainer>
         where TContainer : Behaviour
    {
        public bool enabled;

        public override void Init(TContainer container)
        {
            base.Init(container);
            enabled = container.enabled;
        }
        public override void Accept(ref TContainer container)
        {
            base.Accept(ref container);
            container.enabled = enabled;
        }
    }

    [System.Serializable]
    public class MonoBehaviourPropertyBag<TContainer> : BehaviourPropertyBag<TContainer>
        where TContainer : MonoBehaviour
    {
    }
    #endregion
}