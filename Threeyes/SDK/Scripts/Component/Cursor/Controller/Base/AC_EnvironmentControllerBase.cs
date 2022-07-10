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
    event UnityAction<bool> IsOverrideLightsChanged;
    event UnityAction<bool> IsOverrideSkyboxChanged;

    void InitLights(bool isOverride);
    bool InitSkybox(bool isOverride);
    void InitReflectionProbe(bool isUse);
}

public abstract class AC_EnvironmentControllerBase<TSOConfig, TConfig> : AC_ConfigableComponentBase<TSOConfig, TConfig>, IAC_EnvironmentController
        where TSOConfig : AC_SOConfigBase<TConfig>
{
    public event UnityAction<bool> IsOverrideLightsChanged;
    public event UnityAction<bool> IsOverrideSkyboxChanged;
    public abstract bool IsOverrideLights { get; }
    public abstract bool IsOverrideSkybox { get; }
    public abstract bool IsUseReflection { get; }

    protected IAC_EnvironmentManager Manager { get { return AC_ManagerHolder.EnvironmentManager; } }

    public virtual void OnModControllerInit()
    {
        //Update self
        InitReflectionProbe(IsUseReflection);//Update ReflectionProbe's state first
        InitLights(IsOverrideLights);
        InitSkybox(IsOverrideSkybox);

        //Notify Manager to Update Hub.DefaultEnvironmentController
        NotifyIsOverrideLightsChanged(IsOverrideLights);
        NotifyIsOverrideSkyboxChanged(IsOverrideSkybox);
    }
    public virtual void OnModControllerDeinit() { }

    public abstract void InitReflectionProbe(bool isUse);
    public abstract void InitLights(bool isOverride);
    public abstract bool InitSkybox(bool isOverride);

    protected void NotifyIsOverrideLightsChanged(bool isOverride)
    {
        IsOverrideLightsChanged.Execute(isOverride);
    }
    protected void NotifyIsOverrideSkyboxChanged(bool isOverride)
    {
        IsOverrideSkyboxChanged.Execute(isOverride);
    }
    protected virtual void DynamicGIUpdateEnvironment()
    {
        Manager.DynamicGIUpdateEnvironment();
        //Create your own Reflection solution here (eg: ReflectionProbe)
    }
}
