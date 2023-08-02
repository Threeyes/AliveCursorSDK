using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.Log
{
    public interface ILogManager
    {
        void Create(string tempdirPath);
        void Save(bool isStopLog = true);
        void StopLog();
        void Log(object message);
        void LogWarning(object message);
        void LogError(object message);
    }
}