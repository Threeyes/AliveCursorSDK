using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.Steamworks
{
    /// <summary>
    /// 用于序列化，可通过SOWorkshopItemInfo/Item转化
    /// </summary>
    [System.Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class WorkshopItemInfo
    {
        //#Basic Info
        [JsonProperty] public string title;
        [JsonProperty] public string description;
        [JsonProperty] public string modFileRelatePath;//局部的Mod文件路径
        [JsonProperty] public string previewFileRelatePath;//局部预览图路径                                   
        [JsonProperty] public WSItemVisibility itemVisibility = WSItemVisibility.Public;
        [JsonProperty] public string[] tags;//PS:因为自定义枚举最终呈现的形式都是tags，所以不需要存储枚举，以免后续有更改


        //#Runtime Info
        public WSItemLocation itemLocation = WSItemLocation.Downloaded;//标识Mod位置，便于区分
        public ulong id;//Item ID
        public string dirPath;//Item文件夹路径
        public long fileSize;//文件总大小

        //#Build Info
        protected readonly string ItemModName_Scene = "Scene";//Scene文件的名称
        public static string LcoalUgcItemFileName { get { return "UgcItem.json"; } }
        public static string ItemInfoFileName { get { return "ItemInfo.json"; } }//序列化的WorkshopItemInfo
        public virtual string ItemModName { get { return ItemModName_Scene; } }//打包后的Mod名称，子类可根据类型自定义
        public virtual string ItemModFileName { get { return ItemModName + ".umod"; } }//Mod文件

        public WorkshopItemInfo()
        {
        }
        /// <summary>
        /// 通过SOItemInfo生成后，需要调用此方法
        /// </summary>
        public void Init()
        {
            ///PS：因为该字段因为是基于itemType设置，所以要延后初始化
            modFileRelatePath = ItemModFileName;//固定位置
        }
    }

    public class WorkshopItemInfo<T> : WorkshopItemInfo
        where T : WorkshopItemInfo<T>, new()
    {
        public static readonly T Default = new T();//功能：用于获取实例中的override字段    
    }
}
