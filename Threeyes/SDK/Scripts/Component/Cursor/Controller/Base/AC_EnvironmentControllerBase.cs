using Threeyes.Config;
using UnityEngine.Events;
/// <summary>
/// Control Cursor Environment
/// 
/// PS:
/// 1.As the root gameobject in the scene
/// 2.You can modify more settings via RenderSettings API, but make sure to reset them OnDestroy
/// </summary>
public interface IAC_EnvironmentController : IAC_ModControllerHandler
{
	event UnityAction<bool> IsUseReflectionChanged;
	event UnityAction<bool> IsUseLightsChanged;
	event UnityAction<bool> IsUseSkyboxChanged;
}

public abstract class AC_EnvironmentControllerBase<TSOConfig, TConfig> : ConfigurableComponentBase<TSOConfig, TConfig>, IAC_EnvironmentController
		where TSOConfig : SOConfigBase<TConfig>
{

	//ModController override settings
	public event UnityAction<bool> IsUseLightsChanged;
	public event UnityAction<bool> IsUseReflectionChanged;
	public event UnityAction<bool> IsUseSkyboxChanged;
	public abstract bool IsUseLights { get; }
	public abstract bool IsUseReflection { get; }
	public abstract bool IsUseSkybox { get; }

	protected IAC_EnvironmentManager Manager { get { return AC_ManagerHolder.EnvironmentManager; } }

	public virtual void OnModControllerInit()
	{
		//Update self
		SetLights(IsUseLights);
		SetReflectionProbe(IsUseReflection);//Update ReflectionProbe's gameobject active state before skybox changes, or else the render may not update property
		SetSkybox(IsUseSkybox);
	}
	public virtual void OnModControllerDeinit() { }

	public virtual void SetLights(bool isUse)
	{
		IsUseLightsChanged.Execute(isUse);
	}
	public virtual void SetReflectionProbe(bool isUse)
	{
		IsUseReflectionChanged.Execute(isUse);
	}
	public virtual void SetSkybox(bool isUse)
	{
		IsUseSkyboxChanged.Execute(isUse);
	}
}