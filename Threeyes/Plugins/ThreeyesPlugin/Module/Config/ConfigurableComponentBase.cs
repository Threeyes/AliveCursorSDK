using UnityEngine;
#if USE_NaughtyAttributes
using NaughtyAttributes;
#endif
namespace Threeyes.Config
{
    /// <summary>
    /// Component with optional and configable SO
    ///
    /// </summary>
    /// <typeparam name="TSOConfig"></typeparam>
    /// <typeparam name="TConfig"></typeparam>
    public abstract class ConfigurableComponentBase<TSOConfig, TConfig> : MonoBehaviour, IConfigurableComponent<TSOConfig, TConfig>
    where TSOConfig : SOConfigBase<TConfig>
    {
        public TConfig Config
        {
            get
            {
                if (config == null)
                    config = SOOverrideConfig ? SOOverrideConfig.config : DefaultConfig;
                return config;
            }
        }
        protected TConfig config;

        public TConfig DefaultConfig { get { return defaultConfig; } set { defaultConfig = value; } }
        public TSOConfig SOOverrideConfig { get { return soOverrideConfig; } set { soOverrideConfig = value; } }
        [Header("Config")]
        [SerializeField] protected TConfig defaultConfig;//Default config
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