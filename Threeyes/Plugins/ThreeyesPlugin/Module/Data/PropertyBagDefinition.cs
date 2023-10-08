using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
#if USE_JsonDotNet
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine.Events;
#endif
#if USE_NaughtyAttributes
using NaughtyAttributes;
#endif
/// <summary>
/// 定义组件的必要序列化字段
/// 
/// PS：只是临时定义，后续等RuntimeSceneSerialization完善后就替代为他们的方案
/// </summary>
namespace Threeyes.Data
{
    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public class PropertyBag
    {
        ///PS：可通过参数可在序列化时自动添加"$type"字段，但只能为该类的名称。(开启方式：https://www.codeproject.com/Articles/5284591/Adding-type-to-System-Text-Json-serialization-like)

        [HideInInspector] public string containerTypeName;//组件的类型（比较时可通过GetTypeName将传入类转成string，然后进行比较）

        public PropertyBag()
        {
        }
        //public virtual void AcceptBase(ref UnityEngine.Object container)
        //{

        //}

        #region Utility （Ref: Newtonsoft.Json.Utilities.ReflectionUtils）
        /// <summary>
        /// 功能：获取类型的名称（默认缩略名称）
        /// </summary>
        /// <param name="t"></param>
        /// <param name="assemblyFormat"></param>
        /// <param name="binder"></param>
        /// <returns></returns>
        public static string GetTypeName(Type t, TypeNameAssemblyFormatHandling assemblyFormat = TypeNameAssemblyFormatHandling.Simple, ISerializationBinder binder = null)
        {
            string fullyQualifiedTypeName = GetFullyQualifiedTypeName(t, binder);
            switch (assemblyFormat)
            {
                case TypeNameAssemblyFormatHandling.Simple:
                    return RemoveAssemblyDetails(fullyQualifiedTypeName);
                case TypeNameAssemblyFormatHandling.Full:
                    return fullyQualifiedTypeName;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        private static string GetFullyQualifiedTypeName(Type t, ISerializationBinder binder = null)
        {
            if (binder != null)
            {
                binder!.BindToName(t, out string assemblyName, out string typeName);
                return typeName + ((assemblyName == null) ? "" : (", " + assemblyName));
            }

            return t.AssemblyQualifiedName;
        }
        private static string RemoveAssemblyDetails(string fullyQualifiedTypeName)
        {
            StringBuilder stringBuilder = new StringBuilder();
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            foreach (char c in fullyQualifiedTypeName)
            {
                switch (c)
                {
                    case '[':
                        flag = false;
                        flag2 = false;
                        flag3 = true;
                        stringBuilder.Append(c);
                        break;
                    case ']':
                        flag = false;
                        flag2 = false;
                        flag3 = false;
                        stringBuilder.Append(c);
                        break;
                    case ',':
                        if (flag3)
                        {
                            stringBuilder.Append(c);
                        }
                        else if (!flag)
                        {
                            flag = true;
                            stringBuilder.Append(c);
                        }
                        else
                        {
                            flag2 = true;
                        }

                        break;
                    default:
                        flag3 = false;
                        if (!flag2)
                        {
                            stringBuilder.Append(c);
                        }

                        break;
                }
            }
            return stringBuilder.ToString();
        }
        #endregion
    }

    /// <summary>
    /// 适用于任意数据类
    /// </summary>
    /// <typeparam name="TContainer"></typeparam>
    [Serializable]
    public class PropertyBag<TContainer> : PropertyBag
          where TContainer : class
    {
        public PropertyBag()
        {
        }

        public virtual void Init(TContainer container)
        {
            //PS：需要保证会被调用到
            containerTypeName = GetTypeName(typeof(TContainer));
        }
        //public override void AcceptBase(ref UnityEngine.Object container)
        //{
        //    TContainer containerReal = container as TContainer;
        //    Accept(ref containerReal);
        //}
        /// <summary>
        /// Restore Comp value
        /// </summary>
        /// <param name="container"></param>
        public virtual void Accept(ref TContainer container)
        {
        }
    }

    [Serializable]
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


    [Serializable]
    public class MetadataBase
    {
        [JsonIgnore] public bool IsValid { get { return m_Guid.NotNullOrEmpty(); } }

        /// <summary>
        /// The guid of this prefab
        /// </summary>
        [JsonIgnore] public string Guid { get => m_Guid; set => m_Guid = value; }
#if USE_NaughtyAttributes
        [AllowNesting]
        [ReadOnly]
#endif
        [SerializeField] protected string m_Guid = "";

        public MetadataBase()
        {
        }
        public MetadataBase(string guid)
        {
            m_Guid = guid;
        }
    }

    /// <summary>
    /// 存储实例的唯一ID，常用于编辑器时已经存在的物体
    /// 
    /// PS：
    /// -确保生成的ID在该层级下唯一ID（因为通常反序列化时只跟同层级物体有关），参考（https://forum.unity.com/threads/generate-unique-id-on-object-creation.167991/）
    /// </summary>
    [Serializable]
    public class InstanceMetadata : MetadataBase
    {
        public InstanceMetadata() : base() { }

        [JsonConstructor]//指明使用该Constructor来反序列化（https://www.newtonsoft.com/json/help/html/JsonConstructorAttribute.htm）
        public InstanceMetadata(string guid) : base(guid)
        {
        }

        public static string NewGuid()
        {
            return System.Guid.NewGuid().ToString();
        }
    }


    /// <summary>
    /// 存储Prefab的唯一ID
    /// PS：声明为struct，避免为空
    /// 
    /// Ref:Unity.RuntimeSceneSerialization.Prefabs.PrefabMetadata
    /// </summary>
    [Serializable]
    public class PrefabMetadata : MetadataBase
    {
        public PrefabMetadata() : base() { }

        [JsonConstructor]//指明使用该Constructor来反序列化（https://www.newtonsoft.com/json/help/html/JsonConstructorAttribute.htm）
        public PrefabMetadata(string guid) : base(guid)
        {
        }
    }
    [Serializable]
    public class DataOption_GameObjectPropertyBag : DataOption
    {
        public DataOption_GameObjectPropertyBag()
        {
        }
    }

    /// <summary>
    /// ToAdd:
    /// -如果是Prefab，则额外存储对应的guid和fileId，后续通过 Mod的AssetPack 可以直接还原
    /// -Mod的AssetPack可在打包时扫描Mod文件夹并生成，并且以SO的形式打包（不能直接序列化，因为会丢失对Prefab物体的引用）
    /// </summary>
    [Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public class GameObjectPropertyBag : UnityObjectPropertyBag<GameObject>
    {
        //【Bug】：无法正常解序列化PrefabMetadata，可能要自己实现Resolver才能调用对应的解析器
        public PrefabMetadata prefabMetadata = new PrefabMetadata();//如果为Prefab，则不为空
        public InstanceMetadata instanceMetadata = new InstanceMetadata();//如果为实例，则不为空
        public string name = "GameObject";//PS：名称不能为空，否则会出现警告
        public int layer;
        public string tag = "Untagged";//默认值，避免出错
        public bool active;
        public int siblingIndex;//物体在同级中的顺序，主要用于重新生成时定位（不放在TransformPropertyBag是因为其组件非必须）

        //——以下通过RuntimeSerialization_GameObjectRuntimeSerialization_GameObject进行序列化——
        ///// <summary>
        ///// Warning:
        ///// -默认反序列化时不支持转为其子类，除非:
        ///// #1通过[JsonDerivedType]指明可用子类（https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/polymorphism?pivots=dotnet-7-0）
        ///// #2 【优先】【尚未完成】自定义Resolver，需要扫描Mod自定义的Resolver并进行注册，或者扫描PropertyBag并在解序列化时传入:https://github.com/dotnet/runtime/issues/63747
        ///// #3 第三方插件：https://jason-ge.medium.com/polymorphic-serialization-deserialization-in-system-text-json-dfd48345baae#:~:text=In%20Newtonsoft.Json%2C%20polymorphic%20serialization%20is%20automatically%20handled.%20Although,the%20nuget%20package%20JsonSubTypes%20to%20do%20the%20trick.
        ///// </summary>
        //public List<PropertyBag> serializedComponents = new List<PropertyBag>();

        public List<string> serializedComponents = new List<string>();

        public List<GameObjectPropertyBag> children = new List<GameObjectPropertyBag>();//ToAdd

        public GameObjectPropertyBag() { }
        public override void Init(GameObject container)
        {
            base.Init(container);
            name = container.name;
            layer = container.layer;
            tag = container.tag;
            active = container.activeSelf;
            siblingIndex = container.transform.GetSiblingIndex();
            ////Todo:如果container是Prefab，则搜索其信息
            ///Unity.RuntimeSceneSerialization的实现是加上PrefabMetadata
            //if (container)
        }
        public override void Accept(ref GameObject container)
        {
            base.Accept(ref container);
            container.name = name;
            container.layer = layer;
            container.tag = tag;
            container.SetActive(active);

            //#由开发者自行处理子物体的siblingIndex，因为可能当前生成一序列Prefab，需要延后设置排序
            //if (container.transform.parent)
            //    container.transform.SetSiblingIndex(siblingIndex);
        }
    }




    public interface IComponentPropertyBag { }
    [Serializable]
    public class ComponentPropertyBag<TContainer> : UnityObjectPropertyBag<TContainer>, IComponentPropertyBag
       where TContainer : Component
    {
    }
    [Serializable]
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
    [Serializable]
    public class MonoBehaviourPropertyBag<TContainer> : BehaviourPropertyBag<TContainer>
        where TContainer : MonoBehaviour
    {
    }

    /// <summary>
    /// Ref:  Unity.RuntimeSceneSerialization.Internal.TransformPropertyBagDefinition
    /// </summary>
    [Serializable]
    public class TransformPropertyBag : ComponentPropertyBag<Transform>
    {
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;

        public override void Init(Transform container)
        {
            base.Init(container);
            localPosition = container.localPosition;
            localRotation = container.localRotation;
            localScale = container.localScale;
        }

        public override void Accept(ref Transform container)
        {
            base.Accept(ref container);
            container.localPosition = localPosition;
            container.localRotation = localRotation;
            container.localScale = localScale;
        }

        public TransformPropertyBag() { }
    }


    [System.Serializable]
    public class GameObjectPropertyBagEvent : UnityEvent<GameObjectPropertyBag> { }


}