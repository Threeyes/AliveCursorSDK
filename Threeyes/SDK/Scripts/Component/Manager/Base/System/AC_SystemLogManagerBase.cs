using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AC_SystemLogManagerBase<T> : AC_ManagerWithLifeCycleBase<T>, IAC_SystemLogManager
	where T : AC_SystemLogManagerBase<T>
{
	public abstract void Log(object message);
	public abstract void LogWarning(object message);
	public abstract void LogError(object message);
}
