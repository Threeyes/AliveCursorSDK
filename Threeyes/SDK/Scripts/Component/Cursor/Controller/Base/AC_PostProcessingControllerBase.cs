using Threeyes.Config;
using UnityEngine.Events;

public interface IAC_PostProcessingController : IAC_ModControllerHandler
{
	event UnityAction<bool> IsUsePostProcessingChanged;
}
public abstract class AC_PostProcessingControllerBase<TSOConfig, TConfig> : ConfigurableComponentBase<TSOConfig, TConfig>, IAC_PostProcessingController
	where TSOConfig : SOConfigBase<TConfig>
{
	public event UnityAction<bool> IsUsePostProcessingChanged;
	public abstract bool IsUsePostProcessing { get; }

	public virtual void OnModControllerInit()
	{
		SetPostProcessing(IsUsePostProcessing);
	}
	public virtual void OnModControllerDeinit()
	{
	}

	public virtual void SetPostProcessing(bool isUse)
	{
		IsUsePostProcessingChanged.Execute(isUse);
	}

}
