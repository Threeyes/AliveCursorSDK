using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.Log
{
    /// <summary>
    /// 编辑器下的调试器
    /// </summary>
    public class LogManager_Editor : LogManagerBase<LogManager_Editor>
    {
        public override void Create(string tempdirPath)
        {
        }
        public override void Save(bool isStopLog = true)
        {
            //Do nothing
        }

        public override void StopLog()
        {
            //Do nothing
        }
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
}