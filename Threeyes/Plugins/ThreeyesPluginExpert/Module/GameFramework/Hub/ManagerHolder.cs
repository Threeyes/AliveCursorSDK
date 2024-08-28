using System.Collections.Generic;
using System;

namespace Threeyes.GameFramework
{
    /// <summary>
    /// 存放通用的接口，供通用插件使用
    /// 
    /// PS：
    /// -通过在初始化实例时调用SteamworksTool.RegistManagerHolder(this)注册
    /// -同一实例可能在其他ManagerHolder中多次初始化，适用于不同的代码访问范围（如EnvironmentManager）
    /// </summary>
    public static class ManagerHolder
    {
        public static Func<List<IHubManagerModPreInitHandler>> GetListManagerModPreInitOrder;//Get each Manager to preinit mod by order

        public static Func<List<IHubManagerModInitHandler>> GetListManagerModInitOrder;//Get each Manager to init mod by order

        //——System——
        public static IHubSystemAudioManager SystemAudioManager { get; internal set; }

        //——Mod——
        public static IHubSceneManager SceneManager { get; internal set; }
        public static IHubEnvironmentManager EnvironmentManager { get; internal set; }
    }
}