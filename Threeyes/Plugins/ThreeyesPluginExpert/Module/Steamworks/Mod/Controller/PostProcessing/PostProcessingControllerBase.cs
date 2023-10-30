using Threeyes.Config;
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