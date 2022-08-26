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
	/// Active/DeActive SmearEffect (Achieve by set camera's ClearFlags to Nothing)  
	/// 
	/// PS:
	/// 1.Remember to turn it off when it's done
	/// </summary>
	/// <param name="isSet"></param>
	void SetSmearEffectActive(bool isSet);
}
