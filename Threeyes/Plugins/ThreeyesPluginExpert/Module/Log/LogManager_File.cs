using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Threeyes.Core;
using UnityEngine;

namespace Threeyes.Log
{

    /// <summary>
    /// 功能：
    /// （在Mod切换时、string缓存已满、程序退出前）保存Log文件
    /// 
    /// 格式：
    ///     文件名：【时间戳】.log
    ///     
    /// PS:该Log仅针对每个Item，输出基础的信息；需要查看UMod加载时的信息或堆栈信息等，可以直接查看PlayerLog文件
    ///
    /// </summary>
    public class LogManager_File : LogManagerBase<LogManager_File>
    {
        ///// ToUpdate：
        ///// 1.直接继承ILogHandler，然后通过Debug.logger设置为new Logger(new FileDebugLogHandler())，让用户输出时，自动链接自身的功能。其中ILogger就有开关、筛选等功能）[V2]
        ///// 2.可以开关
        //public class FileDebugLogHandler : ILogHandler
        //{
        //}

        static string strNowTime_FileFormat { get { return DateTime.Now.ToString(DateTimeTool.fileFormat); } }
        static string strNowTime_LogFormat { get { return DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"); } }
        const string strNewLine = "\r\n";
        const int maxLogByteSize = 5242880;//每个Log file的大小上限为5MB=5*2^20 (按每个英文占1个字节）
        const int stringBuilderCapacity = 1024;//默认大小


        protected bool isLogging = false;
        StringBuilder stringBuilder;
        string dirPath;
        public override void Create(string tempdirPath)
        {
            dirPath = tempdirPath;
            if (stringBuilder == null)
                stringBuilder = new StringBuilder(stringBuilderCapacity);
            else
                stringBuilder.Clear();
            isLogging = true;
        }

        public override void StopLog()
        {
            isLogging = false;
        }
        public override void Save(bool isStopLog = true)
        {
            if (!isLogging || stringBuilder.Length == 0)
                return;
            try
            {
                string fileName = strNowTime_FileFormat + ".log";//以保存时间作为名称
                PathTool.GetOrCreateDir(dirPath);
                string filePath = Path.Combine(dirPath, fileName);
                File.WriteAllText(filePath, stringBuilder.ToString());
            }
            catch (Exception e)
            {
                //PS：可能是文件名过长、文件名非法
                Debug.LogError("SaveLog error: \r\n" + e);
            }

            if (isStopLog)
            {
                StopLog();
            }
            stringBuilder.Clear();//清空
        }

        public override void Log(object message)
        {
            if (!isLogging)
                return;
            Debug.Log(message);
            Append(LogType.Log, message);
        }
        public override void LogWarning(object message)
        {
            if (!isLogging)
                return;
            Debug.LogWarning(message);
            Append(LogType.Warning, message);
        }
        public override void LogError(object message)
        {
            if (!isLogging)
                return;
            Debug.LogError(message);
            Append(LogType.Error, message);
        }

        void Append(LogType logType, object message)
        {
            stringBuilder
                .Append("[").Append(strNowTime_LogFormat).Append("] ")
                .Append("[").Append(logType).Append("] ").
                Append(message).Append(strNewLine);
            TrySaveIfFull();
        }

        /// <summary>
        /// 如果内存储存的Log超过上限，就立即存储当前记录
        /// </summary>
        void TrySaveIfFull()
        {
            if (stringBuilder.Length > maxLogByteSize)
                Save(false);
        }
    }
}