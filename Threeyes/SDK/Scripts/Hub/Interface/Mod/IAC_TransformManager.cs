using System.Collections;
using System.Collections.Generic;
using Threeyes.GameFramework;
using UnityEngine;
using UnityEngine.Events;

public interface IAC_TransformManager : IHubManagerModInitHandler, IHubManagerWithController<IAC_TransformController>
{
	Vector3 CursorBaseScale { get; set; }
}
