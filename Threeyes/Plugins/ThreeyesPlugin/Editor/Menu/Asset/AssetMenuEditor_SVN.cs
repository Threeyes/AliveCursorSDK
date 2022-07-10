#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Threeyes.EventPlayer;
using UnityEditor;
using UnityEngine;

namespace Threeyes.Editor
{
    /// <summary>
    /// SVN自动更新
    /// http://www.manew.com/thread-104992-1-1.html
    /// https://tortoisesvn.net/docs/release/TortoiseSVN_zh_CN/tsvn-automation.html#tsvn-automation-basics
    /// </summary>
    public static class AssetMenuEditor_SVN
    {
        private const int Priority = 3000;

        [MenuItem("Assets/SVN Tool/SVN 更新", false, Priority)]
        private static void SvnToolUpdate()
        {
            UpdateAtPaths(EditorTool.GetSelectionAssetPaths());

            //强制刷新相关设置
            SOEventPlayerSettingManager.Instance.RefreshDefine();//避免服务器的配置改动
        }

        [MenuItem("Assets/SVN Tool/SVN 更新(忽略外部引用)", false, Priority)]
        private static void SvnToolUpdateWithoutExternals()
        {
            //https://stackoverflow.com/questions/172018/when-updating-a-whole-projects-root-how-to-exclude-svn-externals-from-being-up
            UpdateAtPaths(EditorTool.GetSelectionAssetPaths(), "--ignore-externals");

            //强制刷新相关设置
            SOEventPlayerSettingManager.Instance.RefreshDefine();//避免服务器的配置改动
        }


        [MenuItem("Assets/SVN Tool/SVN 提交...", false, Priority + 1)]
        private static void SvnToolCommit()
        {
            CommitAtPaths(EditorTool.GetSelectionAssetPaths());
        }
        [MenuItem("Assets/SVN Tool/显示日志", false, Priority + 2)]
        private static void SvnToolLog()
        {
            List<string> assetPaths = EditorTool.GetSelectionAssetPaths();
            if (assetPaths.Count == 0) { return; }
            // 显示日志，只能对单一资产           
            string arg = "/command:log /closeonend:0 /path:\"";
            arg += assetPaths[0];
            arg += "\"";
            SvnCommandRun(arg);
        }

        [MenuItem("Assets/SVN Tool/全部更新", false, Priority + 11)]
        private static void SvnToolAllUpdate()
        {
            // 往上两级，包括数据配置文件     
            string arg = "/command:update /closeonend:0 /path:\"";
            arg += "..";
            arg += "\"";
            SvnCommandRun(arg);
        }

        [MenuItem("Assets/SVN Tool/全部日志", false, Priority + 12)]
        private static void SvnToolAllLog()
        {
            // 往上两级，包括数据配置文件         
            string arg = "/command:log /closeonend:0 /path:\"";
            arg += "..";
            arg += "\"";
            SvnCommandRun(arg);
        }

        /// <summary>        
        /// SVN更新指定的路径    
        /// 路径示例：Assets/1.png      
        /// </summary>      
        /// <param name="assetPaths"></param>     
        public static void UpdateAtPath(string assetPath)
        {
            List<string> assetPaths = new List<string>();
            assetPaths.Add(assetPath);
            UpdateAtPaths(assetPaths);
        }

        /// <summary>        
        ///  SVN更新指定的路径      
        ///  路径示例：Assets/1.png      
        ///  </summary>       
        ///  <param name="assetPaths"></param>   
        public static void UpdateAtPaths(List<string> assetPaths, string extraArg = "")
        {
            if (assetPaths.Count == 0)
            { return; }
            string arg = "/command:update ";
            if (extraArg.NotNullOrEmpty())
                arg += extraArg + " ";

            arg += "/closeonend:0 ";//don't close the dialog automatically

            arg += "/path:\"";
            for (int i = 0; i < assetPaths.Count; i++)
            {
                var assetPath = assetPaths;
                if (i != 0)
                { arg += "*"; }
                arg += assetPath[i];
            }
            arg += "\"";
            //Debug.Log(arg);

            SvnCommandRun(arg);
        }

        /// <summary>      
        /// SVN提交指定的路径       
        /// 路径示例：Assets/1.png        
        /// </summary>        
        /// <param name="assetPaths"></param>      
        public static void CommitAtPaths(List<string> assetPaths, string logmsg = null)
        {
            if (assetPaths.Count == 0)
            { return; }
            string arg = "/command:commit /closeonend:0 /path:\"";
            for (int i = 0; i < assetPaths.Count; i++)
            {
                var assetPath = assetPaths;
                if (i != 0)
                { arg += "*"; }
                arg += assetPath[i];
            }
            arg += "\""; if (!string.IsNullOrEmpty(logmsg)) { arg += " /logmsg:\"" + logmsg + "\""; }
            SvnCommandRun(arg);
        }

        
        /// <summary>     
        /// SVN命令运行     
        /// </summary>     
        /// <param name="arg"></param> 
        private static void SvnCommandRun(string arg)
        {
            string workDirectory = Application.dataPath.Remove(Application.dataPath.LastIndexOf("/Assets", StringComparison.Ordinal));
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { UseShellExecute = false, CreateNoWindow = true, FileName = "TortoiseProc", Arguments = arg, WorkingDirectory = workDirectory });

        }
    }
}
#endif