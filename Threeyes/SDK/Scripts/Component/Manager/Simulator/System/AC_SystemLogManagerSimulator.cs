using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AC_SystemLogManagerSimulator : AC_SystemLogManagerBase<AC_SystemLogManagerSimulator>
{
	public override void Log(object message)
	{
		Debug.Log(message);
	}
	public override void LogWarning(object message)
	{
		Debug.LogWarning(message);
	}
	public override void LogError(object message)
	{
		Debug.LogError(message);
	}
}
