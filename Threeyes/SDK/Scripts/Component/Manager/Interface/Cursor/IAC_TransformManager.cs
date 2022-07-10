using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IAC_TransformManager : IAC_Manager_ModInitHandler, IManagerWithController<IAC_TransformController>
{
	Vector3 CursorBaseScale { get; set; }
}
