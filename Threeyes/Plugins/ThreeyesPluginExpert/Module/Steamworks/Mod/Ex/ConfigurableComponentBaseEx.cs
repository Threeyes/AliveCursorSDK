using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Threeyes.Config;
using Threeyes.Core;
using Threeyes.Data;
using Threeyes.Localization;
using Threeyes.Persistent;
using Threeyes.RuntimeEditor;
using Threeyes.RuntimeSerialization;
using Threeyes.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Threeyes.Steamworks
{
    //——PS：因为以下组件是ThreeyesPlugin、ThreeyesPluginExpert及Json等库的组合实现，所以暂不放在ThreeyesPlugin/Module/Config中，而是作为具体实现——
    //ToUpdate:后期放到一个通用的父级区域，方便使用
    /// <summary>
    /// 功能：
    /// -支持Mod接口
    /// -支持 RuntimeEdit/序列化 的自定义组件
    /// -提供Config数据
    /// 
    /// Warning：
    /// -因为物体不一定需要RuntimeSerializable，所以该组件不使用[RequireComponent(typeof(RuntimeSerializable_GameObject))]来强制序列化，而是有需要再自行增加
    /// -如果使用RTEdit的相关接口：需要在该物体上挂载RS_GO，暂时仅支持defaultConfig字段，因为TSOConfig需要参考UIField_SO一样进行备份和还原（还需要考虑多个组件引用同一个SO的情况）
    /// 
    /// RTS执行顺序：
    /// -Awake
    /// -PropertyBag.Accept
    /// -InitRuntimeEditable
    /// -UpdateSetting
    /// </summary>
    /// <typeparam name="TContainer">可以是子类自身</typeparam>
    /// <typeparam name="TSOConfig"></typeparam>
    /// <typeparam name="TConfig"></typeparam>
    /// <typeparam name="TPropertyBag"></typeparam>
    public abstract class ConfigurableComponentBase<TContainer, TSOConfig, TConfig, TPropertyBag> : ConfigurableComponentBase<TSOConfig, TConfig>
        , IModHandler
        , IRuntimeSerializableComponent
        , IRuntimeEditable
        where TContainer : Component, IConfigurableComponent<TConfig>
        where TSOConfig : SOConfigBase<TConfig>
        where TConfig : SerializableComponentConfigInfoBase, new()
        where TPropertyBag : ConfigurableComponentPropertyBagBase<TContainer, TConfig>, new()
    {
        #region Unity Method
        protected virtual void Awake()
        {
            ///ToFix: 
            ///-如果物体没有激活，那么可能就不会调用该Awake方法， 导致MaterialController.UpdateSetting方法访问cacheDefaultConfig等字段报错)
            ///     PS：
            ///     -UMod包加载可能无此问题，因此物体生成后默认都是显示的
            ///     -可以通过hasSaveDefaultConfig字段判断是否已经初始化
            TrySaveDefaultConfig();//保存模型通过Prefab初始化的数据
            Config.actionPersistentChanged += OnPersistentChanged;
        }
        protected virtual void OnDestroy()
        {
            Config.actionPersistentChanged -= OnPersistentChanged;
        }
        #endregion

        #region IModHandler
        public virtual void OnModInit()
        {
            UpdateSetting();
        }
        public virtual void OnModDeinit() { }
        protected virtual void OnPersistentChanged(PersistentChangeState persistentChangeState)
        {
            ///PS:
            ///-因为用户每次更改配置都会调用该设置，如果是繁重且不需要重复调用的操作（如生成Mesh），则需要将此类操作单独处理（参考AudioVisualizer_IcoSphere）
            UpdateSetting();
        }

        /// <summary>
        /// Update setting base on config info
        /// 
        /// PS:
        /// -如果Config里面都是实时相关字段，则不需要调用
        /// </summary>
        public virtual void UpdateSetting() { }
        #endregion

        #region IRuntimeSerializableComponent

        #region ID  标记该组件的唯一ID，便于绑定。
        public Identity ID { get { return id; } set { } }
        [SerializeField] protected Identity id = new Identity();
#if UNITY_EDITOR
        void OnValidate() { RuntimeSerializationTool.EditorUpdateComponetID(this, ref id); }
#endif
        #endregion

        public Type ContainerType { get { return GetType(); } }
        public IComponentPropertyBag ComponentPropertyBag { get { return GetSerializePropertyBag(); } }
        public virtual string Serialize()
        {
            TPropertyBag propertyBag = GetSerializePropertyBag();
            return RuntimeSerializationTool.SerializeObject(propertyBag);
        }
        public virtual TPropertyBag GetSerializePropertyBag()
        {
            TPropertyBag propertyBag = new TPropertyBag();
            propertyBag.Init(this as TContainer);//Warning：需要使用as转为真实类型，确保containerTypeName会被初始化。具体逻辑在Init中实现
            return propertyBag;
        }
        public virtual void Deserialize(string content, IDeserializationOption baseOption = null)
        {
            TPropertyBag propertyBag = default(TPropertyBag);
            if (content.NotNullOrEmpty())
            {
                propertyBag = JsonConvert.DeserializeObject<TPropertyBag>(content);
            }
            DeserializeFunc(propertyBag);
        }
        public virtual void DeserializeBase(IComponentPropertyBag basePropertyBag, IDeserializationOption baseOption = null)
        {
            if (basePropertyBag is TPropertyBag realPropertyBag)
            {
                DeserializeFunc(realPropertyBag);
            }
        }
        public virtual void DeserializeFunc(TPropertyBag propertyBag)
        {
            TrySaveDefaultConfig();//在反序列化前先保存配置

            TContainer inst = this as TContainer;
            propertyBag?.Accept(ref inst);
        }
        #endregion

        #region IRuntimeEditable
        public FilePathModifier FilePathModifier { get; set; }
        public virtual bool IsRuntimeEditable { get { return true; } }
        public virtual string RuntimeEditableDisplayName { get { return runtimeEditableDisplayName; } }

        public List<RuntimeEditableMemberInfo> GetRuntimeEditableMemberInfos()
        {
            List<ToolStripItemInfo> listInfo = new List<ToolStripItemInfo>()
            {
            new ToolStripItemInfo(LocalizationManagerHolder.LocalizationManager.GetTranslationText("Browser/ItemInfo/Common/Reset"), (obj, arg) => ResetDefaultConfig())
            };
            return new List<RuntimeEditableMemberInfo>()
                {
                    new RuntimeEditableMemberInfo(this, this.GetType(), nameof(defaultConfig), listContextItemInfo:listInfo)//PS:RTEdit暂时只支持字段，所以只能对defaultConfig进行操作。后期等RT支持属性后，再改为Config
                };
        }
        [Header("RuntimeEditable")]
        [SerializeField] string runtimeEditableDisplayName;//方便用户自定义

        /* [SerializeField] */
        protected TConfig cacheDefaultConfig = null;//缓存组件序列化之前的DefaultConfig，方便还原 (Warning：不应该标记为[SerializeField]，否则UMod还原会导致其在Awake的值被刷掉)

        public virtual void InitRuntimeEditable(FilePathModifier filePathModifier)
        {
            //——ToTest：看UMod加载后数据会不会出错（比如说OnPersistentChanged因为注册了序列化前的字段导致无法被调用），如果出错就取消以下注释进行延迟修改（可以研究一下UMod的反序列化是整个类实例替换还是对可序列化字段进行替换，如果是后者则Action不会被替换）。需要注意的是，因为是由AD_SerializableItemControllerBase管理所有IRuntimeEditable的初始化，所以以下代码可以直接放到AD_SerializableItemControllerBase上——

            //    if (UModTool.IsUModGameObject(this))
            //        CoroutineManager.StartCoroutineEx(IEInit(filePathModifier));
            //    else
            //        InitFunc(filePathModifier);
            //}
            //IEnumerator IEInit(FilePathModifier filePathModifier)
            //{
            //    yield return null;//等待UMod初始化完成
            //    yield return null;//等待UMod初始化完成
            //    InitFunc(filePathModifier);//等UMod反序列化完成后再监听Config
            //}
            //protected virtual void InitFunc(FilePathModifier filePathModifier)
            //{

            //#1 初始化参数
            FilePathModifier = filePathModifier;

            //#2 重新监听的PD事件
            RuntimeEditableRegisterDataEvent();

            ///#3 模拟PD的初始化流程，通知data更新相关字段，以及读取外部资源，通过回调OnPersistentChanged调用UpdateSetting方法
            PersistentObjectTool.ForceInit(defaultConfig, FilePathModifier.ParentDir);
        }

        /// <summary>
        /// （RuntimeEditable）重新注册PD的回调
        /// </summary>
        protected virtual void RuntimeEditableRegisterDataEvent()
        {
            ///PS:
            ///-以下事件可能在Awake会进行初次监听，因此需要先-再+，确保回调不会被多次监听。换而言之，可以把Awake的相关事件进行重新注册
            ///-RuntimeEditable暂时不支持SO，仅支持defaultConfig，后续优化（因为物体可能引用同一个SO，需要先克隆SO再进行编辑）
            defaultConfig.actionPersistentChanged -= OnPersistentChanged;
            defaultConfig.actionPersistentChanged += OnPersistentChanged;
        }

        protected bool hasSaveDefaultConfig = false;
        /// <summary>
        /// 尝试保存DefaultConfig，便于UIRuntimeEdit重置
        /// 调用位置：
        /// -Awake
        /// -Deserialize之前（因为该方法可能在生成预制物实例后就立即调用从而优先于Awake，所以也要尝试调用）
        /// </summary>
        void TrySaveDefaultConfig()
        {
            if (hasSaveDefaultConfig)
                return;

            ///Todo:
            ///-备份defaultConfig，便于重置和比较
            cacheDefaultConfig = UnityObjectTool.DeepCopy(defaultConfig);

            hasSaveDefaultConfig = true;
        }
        void ResetDefaultConfig()
        {
            if (cacheDefaultConfig == null)
            {
                Debug.LogError($"{nameof(cacheDefaultConfig)} not init!");
                return;
            }
            PersistentObjectTool.ForceInit(defaultConfig, FilePathModifier.ParentDir, cacheDefaultConfig);
        }
        #endregion
    }

    public abstract class ConfigurableComponentBase<TComp, TContainer, TSOConfig, TConfig, TPropertyBag> : ConfigurableComponentBase<TContainer, TSOConfig, TConfig, TPropertyBag>
        , IModHandler
        , IRuntimeSerializableComponent
        , IRuntimeEditable
        where TComp : Component
        where TContainer : Component, IConfigurableComponent<TConfig>
        where TSOConfig : SOConfigBase<TConfig>
        where TConfig : SerializableComponentConfigInfoBase, new()
        where TPropertyBag : ConfigurableComponentPropertyBagBase<TContainer, TConfig>, new()
    {
        //Cache for easy access
        public virtual TComp Comp
        {
            get
            {
                if (!comp)
                    comp = GetCompFunc();
                return comp;
            }
            set
            {
                comp = value;
            }
        }
        public TComp comp;
        protected virtual TComp GetCompFunc()
        {
            if (this)//avoid gameobject get destroyed
                return GetComponent<TComp>();

            return default(TComp);
        }
    }

    #region Define
    [Serializable]
    [PersistentChanged(nameof(OnPersistentChanged))]
    public class SerializableComponentConfigInfoBase : SerializableDataBase
    {
        [JsonIgnore] public UnityAction<PersistentChangeState> actionPersistentChanged;

        #region Callback
        void OnPersistentChanged(PersistentChangeState persistentChangeState)
        {
            actionPersistentChanged.Execute(persistentChangeState);
        }
        #endregion
    }

    /// <summary>
    /// 继承了IConfigurableComponent的相关PropertyBab
    /// 仅作为数据的序列化/反序列化容器，不做其他初始化操作（如设置enabled），组件需要自行继承
    /// </summary>
    /// <typeparam name="TContainer"></typeparam>
    /// <typeparam name="TConfig"></typeparam>
    [System.Serializable]
    [JsonObject(MemberSerialization.Fields)]
    public abstract class ConfigurableComponentPropertyBagBase<TContainer, TConfig> : ComponentPropertyBag<TContainer>
        where TContainer : Component, IConfigurableComponent<TConfig>//ConfigurableComponentBase<TConfig>
        where TConfig : class, new()
    {
        public TConfig data = new TConfig();//避免报null错

        public override void Init(TContainer container)
        {
            base.Init(container);
            data = container.DefaultConfig;
        }
        public override void Accept(ref TContainer container)
        {
            base.Accept(ref container);
            ////Warning：不应该直接复制引用，而是克隆其子类，否则因为InitRuntimeEditable绑定的是defaultConfig的回调，而以下代码又使用数据实例替换，导致无法正常监听（因为Awake监听了原defaultConfig，而InitRuntimeEditable监听的是新数据实例，从而导致OnPersistentChanged被多次调用）（该注释保留作为警示）
            //if (data != null)
            //    container.DefaultConfig = data;

            //ToUpdate:应该不包括Asset引用，避免替换了默认引用

            if (data != null)
                UnityObjectTool.CopyFields(data, container.Config, funcCopyFilter: PersistentObjectTool.FieldsCopyFilter_ExcludeUnityObject);//复制全部字段，(不包括Texture等Object引用，否则会替换TextureShaderProperty等默认值）
        }
    }
    #endregion
}