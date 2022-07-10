using Newtonsoft.Json;
using Steamworks.Ugc;
using System.IO;
using System.Linq;
using UnityEngine;


public static class AC_WorkshopItemInfoFactory
{
    /// <summary>
    /// 
    /// PS：【EditorOnly】仅在UnityEditor Build Mod时调用
    /// </summary>
    /// <param name="sOWorkshopItemInfo"></param>
    public static AC_WorkshopItemInfo Create(AC_SOWorkshopItemInfo sOWorkshopItemInfo)
    {
        return new AC_WorkshopItemInfo()
        {
            //Set basic
            title = sOWorkshopItemInfo.Title,
            description = sOWorkshopItemInfo.Description,
            itemVisibility = sOWorkshopItemInfo.ItemVisibility,
            tags = sOWorkshopItemInfo.Tags,
            modFileRelatePath = AC_WorkshopItemInfo.ItemModFileName,
            previewFileRelatePath = sOWorkshopItemInfo.PreviewFilePath.NotNullOrEmpty() ? new FileInfo(sOWorkshopItemInfo.PreviewFilePath).Name : "",

            //Set runtime
            itemLocation = AC_WCItemLocation.UnityProject,
            //id=//PS:id可能尚未设置，所以暂不设置
            dirPath = sOWorkshopItemInfo.ItemDirPath,
        };
    }

    public static bool IsValidDir(string itemDirPath, AC_WCItemLocation itemLocation = AC_WCItemLocation.Downloaded)
    {
        string jsonPath = Path.Combine(itemDirPath, AC_WorkshopItemInfo.ItemInfoFileName);
        return File.Exists(jsonPath);
    }

    /// <summary>
    /// 通过打包后的Item所在目录，生成WorkshopItemInfo。
    /// </summary>
    /// <param name="itemDirPath"></param>
    /// <param name="itemLocation">有效值：Downloaded/UnityExported</param>
    /// <returns></returns>
    public static AC_WorkshopItemInfo Create(string itemDirPath, AC_WCItemLocation itemLocation = AC_WCItemLocation.Downloaded)
    {
        AC_WorkshopItemInfo inst = null;

        string jsonPath = Path.Combine(itemDirPath, AC_WorkshopItemInfo.ItemInfoFileName);
        if (File.Exists(jsonPath))
        {
            var result = File.ReadAllText(jsonPath);
            inst = JsonConvert.DeserializeObject<AC_WorkshopItemInfo>(result);//读取Dir目录的对应json文件并反序列化

            if (inst != null)
            {
                //Set runtime
                inst.itemLocation = itemLocation;
                if (itemLocation == AC_WCItemLocation.Downloaded)//已下载资源才有正确的ID
                {
                    inst.id = AC_WorkshopItemTool.GetId(itemDirPath);
                }
                else
                {
                    //(Todo:针对测试路径的Item，通过title生成唯一的ID(以-开头作为区分）（非必要，经查阅，没有能够生成唯一long的方法）
                }
                inst.dirPath = itemDirPath;

                //ToUpdate: 如果本地已经存在Item.json, 则从中获取fileSize
                DirectoryInfo directoryInfo = new DirectoryInfo(itemDirPath);
                if (directoryInfo.Exists)
                    inst.fileSize = directoryInfo.EnumerateFiles().Sum(file => file.Length);
            }
        }
        else
        {
            Debug.LogError($"Json file not exist in dir: {jsonPath}!");
        }
        return inst;
    }


    /// <summary>
    /// (PS:一般不需要使用，后期删掉)
    /// 通过Mod下载后的Json文件反序列化
    /// (因为是反序列化，所以只能通过静态方法生成）
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static AC_WorkshopItemInfo Create(Item item)
    {
        AC_WorkshopItemInfo inst = null;

        string itemDir = item.Directory;
        string jsonPath = Path.Combine(itemDir, AC_WorkshopItemInfo.ItemInfoFileName);
        if (File.Exists(jsonPath))
        {
            var result = File.ReadAllText(jsonPath);
            inst = JsonConvert.DeserializeObject<AC_WorkshopItemInfo>(result);//读取Dir目录的对应json文件并反序列化
            if (inst != null)
            {
                //Set runtime
                inst.id = item.Id.Value;
                inst.dirPath = itemDir;
                inst.itemLocation = AC_WCItemLocation.Downloaded;
            }
        }
        else
        {
            Debug.LogError($"Json file not exist for item {item.Title}!");
        }
        return inst;
    }
}