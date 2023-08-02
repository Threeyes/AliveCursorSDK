using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Threeyes.Log;

namespace Threeyes.Steamworks
{
    /// <summary>
    /// 存放通用的接口，便用通用插件使用
    /// 
    /// PS：
    /// -同一实例可能在其他ManagerHolder中多次初始化
    /// </summary>
    public static class ManagerHolder
    {
        public static ILogManager LogManager { get; internal set; }

        //——System——
        public static IHubSystemAudioManager SystemAudioManager { get; internal set; }

        //——Mod——
        public static IHubSceneManager SceneManager { get; internal set; }
        public static IHubEnvironmentManager EnvironmentManager { get; internal set; }
    }
}