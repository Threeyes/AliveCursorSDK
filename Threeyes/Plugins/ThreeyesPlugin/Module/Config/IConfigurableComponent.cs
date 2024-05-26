using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.Config
{
    public interface IConfigurableComponent<TConfig>
    {
        /// <summary>
        /// Get first valid Config 
        /// </summary>
        TConfig Config { get; }

        /// <summary>
        /// The default config
        /// </summary>
        TConfig DefaultConfig { get; set; }
    }

    /// <summary>
    /// Component that use SO to save config
    /// </summary>
    /// <typeparam name="TSOConfig"></typeparam>
    /// <typeparam name="TConfig"></typeparam>
    public interface IConfigurableComponent<TSOConfig, TConfig> : IConfigurableComponent<TConfig>
        where TSOConfig : SOConfigBase<TConfig>
        where TConfig : new()
    {
        TSOConfig SOOverrideConfig { get; set; }

        //#Template, Just copy and rename
        //public TConfig Config
        //{
        //    get
        //    {
        //        if (config == null)
        //            config = SOOverrideConfig ? SOOverrideConfig.config : DefaultConfig;
        //        return config;
        //    }
        //}
        //public TConfig DefaultConfig { get { return defaultConfig; } set { defaultConfig = value; } }
        //public TSOConfig SOOverrideConfig { get { return soOverrideConfig; } set { soOverrideConfig = value; } }
        //[Header("Config")]
        //[SerializeField] protected TConfig defaultConfig;//Default config
        //[Expandable] [SerializeField] protected TSOConfig soOverrideConfig;//Override config
    }
}