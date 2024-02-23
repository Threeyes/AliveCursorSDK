using System.Collections;
using System.Collections.Generic;
using Threeyes.Core;
using Threeyes.Steamworks;
using UnityEngine;

namespace Threeyes.Log
{
    public abstract class LogManagerBase<T> : InstanceBase<T>
        , ILogManager
        where T : LogManagerBase<T>
    {
        public abstract void Create(string tempdirPath);
        public abstract void Save(bool isStopLog = true);
        public abstract void StopLog();
        public abstract void Log(object message);
        public abstract void LogWarning(object message);
        public abstract void LogError(object message);
    }
}