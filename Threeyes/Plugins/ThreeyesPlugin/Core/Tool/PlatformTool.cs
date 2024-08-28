using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.Core
{
    /// <summary>
    /// 常用的调试信息，运行后可用
    /// 
    /// ToUpdate:改名为PlatformTool
    /// </summary>
    public static class PlatformTool
    {
        /// <summary>
        /// In the Build Settings dialog there is a check box called "Development Build".
        /// </summary>
        public static bool IsDebugBuild => Debug.isDebugBuild;
        /// <summary>
        /// Editor or DebugBuild
        /// </summary>
        /// <value></value>
        public static bool IsEditorOrDebugBuild => Application.isEditor || !Application.isEditor && IsDebugBuild;

        public static bool IsRuntimeDebugBuild => !Application.isEditor && IsDebugBuild;

        //运行时+移动端
        public static bool IsRuntimeMobileBuild
        {
            get
            {
#if !UNITY_EDITOR
                return Application.isMobilePlatform;
#else
                return false;
#endif
            }
        }
        public static bool IsRuntimeAndroidBuild//运行时+安卓端
        {
            get
            {
#if !UNITY_EDITOR && UNITY_ANDROID
                return true;
                //return Application.platform == RuntimePlatform.Android;
#else
                return false;
#endif
            }
        }
        public static bool IsRuntimeIOSBuild//运行时+IOS端
        {
            get
            {
#if !UNITY_EDITOR && UNITY_IOS
                return true;
#else
                return false;
#endif
            }
        }

        public static bool GetEditorMode(bool extraBool, string validDebugTips = null)
        {
            //编辑器模式/打包后的DebugBuild模式
            bool isMode = Application.isEditor && extraBool;
            if (isMode && validDebugTips != null)
            {
                Debug.Log("[EditorMode] " + validDebugTips);
            }
            return isMode;
        }

        /// <summary>
        /// 检查当前是否为Debug状态（满足条件：前提是isDebug为true，然后是编辑器 || 真机+DebugBuild）
        /// </summary>
        /// <param name="extraBool"></param>
        /// <param name="validDebugTips">进入debug模式时的提示</param>
        /// <returns></returns>
        public static bool GetDebugMode(bool extraBool, string validDebugTips = null)
        {
            //编辑器模式/打包后的DebugBuild模式
            bool isDebugMode = (Application.isEditor || !Application.isEditor && IsDebugBuild) && extraBool;

            if (isDebugMode && validDebugTips != null)
            {
                Debug.Log("[DebugMode] " + validDebugTips);
            }

            return isDebugMode;
        }
    }
}