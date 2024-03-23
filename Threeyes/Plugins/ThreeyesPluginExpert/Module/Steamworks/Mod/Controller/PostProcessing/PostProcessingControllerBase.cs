using Threeyes.Config;
using Threeyes.Core;
using UnityEngine.Events;

namespace Threeyes.Steamworks
{
    public interface IPostProcessingController : IModControllerHandler
    {
        event UnityAction<bool> IsUsePostProcessingChanged;
    }
    public interface ISOPostProcessingControllerConfig
    {
    }

    /// <summary>
    /// 针对Mod场景的全局PPController
    /// </summary>
    /// <typeparam name="TSOConfig"></typeparam>
    /// <typeparam name="TConfig"></typeparam>
    public abstract class PostProcessingControllerBase<TSOConfig, TConfig> : ConfigurableComponentBase<TSOConfig, TConfig>, IPostProcessingController
        where TSOConfig : SOConfigBase<TConfig>, ISOPostProcessingControllerConfig
    {
        public event UnityAction<bool> IsUsePostProcessingChanged;
        public abstract bool IsUsePostProcessing { get; }

        public virtual void OnModControllerInit()
        {
            UpdateSetting(IsUsePostProcessing);
        }
        public virtual void OnModControllerDeinit()
        {
        }

        public virtual void UpdateSetting(bool isUse)
        {
            IsUsePostProcessingChanged.Execute(isUse);
        }
    }
}