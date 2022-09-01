using com.zibra.liquid.Manipulators;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using Threeyes.Persistent;
using UnityEngine;
using UnityEngine.Events;
using static com.zibra.liquid.Manipulators.ZibraLiquidEmitter;

public class AC_ZibraLiquidEmitterController : AC_ConfigableComponentBase<ZibraLiquidEmitter, AC_SOZibraLiquidEmitterControllerConfig, AC_ZibraLiquidEmitterController.ConfigInfo>
	, IAC_ModHandler
{
	#region Unity Method
	private void Awake()
	{
		Config.actionPersistentChanged += OnPersistentChanged;
	}
	private void OnDestroy()
	{
		Config.actionPersistentChanged -= OnPersistentChanged;
	}
	#endregion

	#region  Callback
	public void OnModDeinit()
	{
		UpdateSetting();
	}
	public void OnModInit() { }
	void OnPersistentChanged(PersistentChangeState persistentChangeState)
	{
		if (persistentChangeState == PersistentChangeState.Load)
			return;
		UpdateSetting();
	}
	void UpdateSetting()
	{
		Comp.VolumePerSimTime = Config.volumePerSimTime;
		Comp.InitialVelocity = Config.initialVelocity;
		Comp.PositionClampBehavior = Config.positionClampBehavior;
	}
	#endregion


	#region Define
	[Serializable]
	[PersistentChanged(nameof(ConfigInfo.OnPersistentChanged))]
	public class ConfigInfo : AC_SerializableDataBase
	{
		[JsonIgnore] public UnityAction<PersistentChangeState> actionPersistentChanged;

		[Tooltip("Emitted volume per simulation time unit")] [Min(0.0f)] public float volumePerSimTime = 0.125f;
		[Tooltip("Initial velocity of newly created particles")] public Vector3 initialVelocity = new Vector3(0, 0, 0);
		[Tooltip("Controls what whether effective position of emitter will clamp to container bounds.")] public ClampBehaviorType positionClampBehavior = ClampBehaviorType.Clamp;

		#region Callback
		void OnPersistentChanged(PersistentChangeState persistentChangeState)
		{
			actionPersistentChanged.Execute(persistentChangeState);
		}
		#endregion
	}
	#endregion
}