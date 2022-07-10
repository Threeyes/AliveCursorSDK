using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAC_SystemLogManager
{
	void Log(object message);
	void LogWarning(object message);
	void LogError(object message);
}
