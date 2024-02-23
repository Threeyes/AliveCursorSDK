using UnityEngine;
using Threeyes.Core;
#if USE_NaughtyAttributes
using NaughtyAttributes;
#endif
namespace Threeyes.Config
{
    public abstract class ConfigurableComponentBase<TConfig> : MonoBehaviour, IConfigurableComponent<TConfig>
    {
        public virtual TConfig Config { get { return defaultConfig; } }

        public TConfig DefaultConfig { get { return defaultConfig; } set { defaultConfig = value; } }
        [Header("Config")]
        [SerializeField] protected TConfig defaultConfig;//Default config
    }

    /// <summary>
    /// Component with optional and configable SO
    ///
    /// </summary>
    /// <typeparam name="TSOConfig"></typeparam>
    /// <typeparam name="TConfig"></typeparam>
    public abstract class ConfigurableComponentBase<TSOConfig, TConfig> : ConfigurableComponentBase<TConfig>, IConfigurableComponent<TSOConfig, TConfig>
    where TSOConfig : SOConfigBase<TConfig>
    {
        public override TConfig Config
        {
            get
            {
                return SOOverrideConfig ? SOOverrideConfig.config : DefaultConfig;//不使用config字段缓存引用，因为有可能会出现uMod加载并反序列化后导致数据对不上的问题
            }
        }

        public TSOConfig SOOverrideConfig { get { return soOverrideConfig; } set { soOverrideConfig = value; } }
#if USE_NaughtyAttributes
        [Expandable]
#endif
        [SerializeField] protected TSOConfig soOverrideConfig;//Override config
    }

    public abstract class ConfigurableComponentBase<TComp, TSOConfig, TConfig> : ConfigurableComponentBase<TSOConfig, TConfig>
        where TComp : Component
        where TSOConfig : SOConfigBase<TConfig>
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

    public abstract class ConfigurableInstanceBase<T, TSOConfig, TConfig> : ConfigurableComponentBase<TSOConfig, TConfig>, ISetInstance
        where T : ConfigurableInstanceBase<T, TSOConfig, TConfig>
        where TSOConfig : SOConfigBase<TConfig>
    {
        public static T Instance { get { return instance; } protected set { instance = value; } }
        private static T instance;
        bool isInit = false;
        public virtual void SetInstance()
        {
            if (!isInit)
            {
                SetInstanceFunc();
            }
        }

        protected virtual void SetInstanceFunc()
        {
            Instance = this as T;
            isInit = true;
        }
    }
}