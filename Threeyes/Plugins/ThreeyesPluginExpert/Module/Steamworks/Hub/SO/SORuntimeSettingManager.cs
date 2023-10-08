using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
namespace Threeyes.Steamworks
{
    /// <summary>
    /// 存储Steamworks插件的运行时配置：
    /// -项目相关信息
    /// -SDK名称
    /// -接口代码前缀等
    /// -Package ID
    /// PS:
    /// -放在SDK目录下，一般禁止用户修改
    /// </summary>
    [CreateAssetMenu(menuName = Steamworks_EditorDefinition.AssetMenuPrefix_Root + "RuntimeSetting", fileName = "SteamworksRuntimeSetting")]//Warning:这里的名称不能有符号（如.），否则会报错。
    public sealed class SORuntimeSettingManager : SOInstacneBase<SORuntimeSettingManager, SORuntimeSettingManagerInfo>
    {
        ///ToAdd:
        ///-部分非通用子类，可以通过名称组合获得其对应的类名，并通过反射获得其实例（如AC_ManagerHolder）
        public string SimulatorSceneName { get { return productName + "Hub_Simulator"; } }

        /// <summary>
        /// 项目全称（无空格）。如：AliveCursor
        /// </summary>
        public string productName;
        /// <summary>
        /// 项目缩写，可用于接口标识。如：AC
        /// </summary>
        public string productNameForShort;
        /// <summary>
        /// Steam中该应用的唯一id
        /// </summary>
        public uint steamAppID;

        public string sDKIdentifier = "";//SDK的Package ID。如：com.threeyes.alivecursor.sdk

    }
    public class SORuntimeSettingManagerInfo : SOInstacneInfo
    {
        public override string pathInResources { get { return "Threeyes"; } }
        public override string defaultName { get { return "SteamworksRuntimeSetting"; } }
    }
}