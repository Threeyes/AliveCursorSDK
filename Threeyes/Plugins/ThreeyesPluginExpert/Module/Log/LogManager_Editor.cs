using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Conditional = System.Diagnostics.ConditionalAttribute;

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
            LogStatic(message);
        }
        public override void LogWarning(object message)
        {
            LogWarningStatic(message);
        }
        public override void LogError(object message)
        {
            LogErrorStatic(message);
        }

        //[Conditional("ENABLE_LOG")]
        static public void LogStatic(object message)
        {
            Debug.Log(message);
        }
        //[Conditional("ENABLE_LOG")]
        static public void LogWarningStatic(object message)
        {
            Debug.LogWarning(message);
        }
        //[Conditional("ENABLE_LOG")]
        static public void LogErrorStatic(object message)
        {
            Debug.LogError(message);
        }

    }
}