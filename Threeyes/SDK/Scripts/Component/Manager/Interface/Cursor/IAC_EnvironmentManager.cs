using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAC_EnvironmentManager : IAC_Manager_ModInitHandler, IManagerWithController<IAC_EnvironmentController>
{
	/// <summary>
	/// The main camera
	/// </summary>
	Camera MainCamera { get; }

	/// <summary>
	/// Schedules an update of the environment cubemap
	/// </summary>
	void DynamicGIUpdateEnvironment();

	/// <summary>
	/// Active/DeActive global ReflectionProbe
	/// </summary>
	/// <param name="isActive"></param>
	void SetReflectionProbeActive(bool isActive);

	/// <summary>
	/// Active/DeActive SmearEffect (Achieve by set camera's ClearFlags to Nothing)  
	/// 
	/// PS:
	/// 1.Remember to turn it off when it's done
	/// </summary>
	/// <param name="isSet"></param>
	void SetSmearEffectActive(bool isSet);
}
