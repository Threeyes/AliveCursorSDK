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
        public static string UgcItemFileName { get { return "UgcItem.json"; } }//通过UGC接口查询的Item信息 文件名
        public static string ItemInfoFileName { get { return "ItemInfo.json"; } }//序列化的WorkshopItemInfo 文件名
  
        public WorkshopItemInfo()
        {
        }
    }

    public class WorkshopItemInfo<T> : WorkshopItemInfo
        where T : WorkshopItemInfo<T>, new()
    {
        public static readonly T Default = new T();//功能：用于获取实例中的override字段    
    }
}
