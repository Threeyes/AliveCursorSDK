using Threeyes.Config;
using UnityEngine;
using UnityEngine.Events;

namespace Threeyes.GameFramework
{
    /// <summary>
    /// Control Cursor Environment
    /// 
    /// PS:
    /// 1.As the root gameobject in the scene
    /// 2.You can modify more settings via RenderSettings API, but make sure to reset them OnDestroy
    /// </summary>
    public interface IEnvironmentController : IModControllerHandler
    {
        Light SunSourceLight { get; }
        int SkyboxControllerCount { get; }
        SkyboxController ActiveSkyboxController { get; }
        void RegisterSkyboxController(SkyboxController skyboxController);
        void UnRegisterSkyboxController(SkyboxController skyboxController);

        bool RefreshReflectionProbe();
    }
    public interface ISOEnvironmentControllerConfig
    {
    }

    public abstract class EnvironmentControllerBase<TSOConfig, TConfig> : ConfigurableComponentBase<TSOConfig, TConfig>, IEnvironmentController
        where TSOConfig : SOConfigBase<TConfig>, ISOEnvironmentControllerConfig
        where TConfig : new()
    {
        public abstract Light SunSourceLight { get; }

        public virtual void OnModControllerInit() { }
        public virtual void OnModControllerDeinit() { }

        public abstract int SkyboxControllerCount { get; }
        public abstract SkyboxController ActiveSkyboxController { get; }
        public abstract void RegisterSkyboxController(SkyboxController skyboxController);
        public abstract void UnRegisterSkyboxController(SkyboxController skyboxController);

        public abstract bool RefreshReflectionProbe();
    }
}