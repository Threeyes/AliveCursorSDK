using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.Config
{
    /// <summary>
    /// Component that use SO to save config
    /// </summary>
    /// <typeparam name="TSOConfig"></typeparam>
    /// <typeparam name="TConfig"></typeparam>
    public interface IConfigurableComponent<TSOConfig, TConfig>
        where TSOConfig : SOConfigBase<TConfig>
    {
        TConfig Config { get; }
        TConfig DefaultConfig { get; }
        TSOConfig SOOverrideConfig { get; }

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