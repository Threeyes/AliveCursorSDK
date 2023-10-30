using Threeyes.Config;
using UnityEngine.Events;

namespace Threeyes.Steamworks
{
    /// <summary>
    /// Control Cursor Environment
    /// 
    /// PS:
    /// 1.As the root gameobject in the scene
    /// 2.You can modify more settings via RenderSettings API, but make sure to reset them OnDestroy
    /// </summary>
    public interface IEnvironmentController : IModControllerHandler { }
    public interface ISOEnvironmentControllerConfig
    {
    }

    public abstract class EnvironmentControllerBase<TSOConfig, TConfig> : ConfigurableComponentBase<TSOConfig, TConfig>, IEnvironmentController
            where TSOConfig : SOConfigBase<TConfig>, ISOEnvironmentControllerConfig
    {
        public virtual void OnModControllerInit() { }
        public virtual void OnModControllerDeinit() { }
    }
}