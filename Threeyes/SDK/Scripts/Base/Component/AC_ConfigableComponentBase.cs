using NaughtyAttributes;
using UnityEngine;
/// <summary>
/// Component with configable SO
///
/// Todo：将常用的实现提炼到该类中（如Awake、OnDestroy等）
/// </summary>
/// <typeparam name="TSOConfig"></typeparam>
/// <typeparam name="TConfig"></typeparam>
public abstract class AC_ConfigableComponentBase<TSOConfig, TConfig> : MonoBehaviour, IAC_ConfigableComponent<TSOConfig, TConfig>
	where TSOConfig : AC_SOConfigBase<TConfig>
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
	[Expandable] [SerializeField] protected TSOConfig soOverrideConfig;//Override config
}

public abstract class AC_ConfigableComponentBase<TComp, TSOConfig, TConfig> : AC_ConfigableComponentBase<TSOConfig, TConfig>
	where TComp : Component
	where TSOConfig : AC_SOConfigBase<TConfig>
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

/// <summary>
/// Component with configable SO, along with different type of update method
/// </summary>
/// <typeparam name="TComp"></typeparam>
/// <typeparam name="TSOConfig"></typeparam>
/// <typeparam name="TConfig"></typeparam>
public abstract class AC_ConfigableUpdateComponentBase<TComp, TSOConfig, TConfig> : AC_ConfigableComponentBase<TComp, TSOConfig, TConfig>
	where TComp : Component
	where TSOConfig : AC_SOConfigBase<TConfig>
{
	public UpdateMethodType updateMethodType = UpdateMethodType.Late;
	protected float DeltaTime
	{
		get
		{
			switch (updateMethodType)
			{
				case UpdateMethodType.Default:
				case UpdateMethodType.Late:
					return Time.deltaTime;
				case UpdateMethodType.Fixed:
					return Time.fixedDeltaTime;
				default:
					Debug.LogError(updateMethodType + " Not Define!");
					return 0;
			}
		}
	}

	protected virtual void Update()
	{
		if (updateMethodType != UpdateMethodType.Default)
			return;
		UpdateFunc();
	}
	protected virtual void LateUpdate()
	{
		if (updateMethodType != UpdateMethodType.Late)
			return;
		UpdateFunc();
	}
	protected virtual void FixedUpdate()
	{
		if (updateMethodType != UpdateMethodType.Fixed)
			return;
		UpdateFunc();
	}

	protected virtual void UpdateFunc()
	{
		//PS:可能会新增方法
	}
}