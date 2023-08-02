using Newtonsoft.Json;
using Steamworks.Ugc;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Threeyes.Steamworks
{
    public interface IWorkshopItemInfoFactory
    {
        WorkshopItemInfo CreateBase(SOWorkshopItemInfo sOWorkshopItemInfo);
        WorkshopItemInfo CreateBase(string itemDirPath, WSItemLocation itemLocation = WSItemLocation.Downloaded);
    }
    public class WorkshopItemInfoFactory<TInstance, TSOItemInfo, TItemInfo> : IWorkshopItemInfoFactory
        where TInstance : WorkshopItemInfoFactory<TInstance, TSOItemInfo, TItemInfo>, new()
        where TSOItemInfo : SOWorkshopItemInfo
        where TItemInfo : WorkshopItemInfo, new()
    {
        public static readonly TInstance Instance = new TInstance();

        public WorkshopItemInfoFactory()
        {
            SteamworksTool.RegistManagerHolder(this);
        }

        public WorkshopItemInfo CreateBase(SOWorkshopItemInfo sOWorkshopItemInfo)
        {
            return Create((TSOItemInfo)sOWorkshopItemInfo);
        }
        public WorkshopItemInfo CreateBase(string itemDirPath, WSItemLocation itemLocation = WSItemLocation.Downloaded)
        {
            return Create(itemDirPath, itemLocation);
        }


        /// <summary>
        /// 
        /// PS：【EditorOnly】仅在UnityEditor Build Mod时调用
        /// </summary>
        /// <param name="sOWorkshopItemInfo"></param>
        public virtual TItemInfo Create(TSOItemInfo sOWorkshopItemInfo)
        {
            return new TItemInfo()
            {
                //Set basic
                title = sOWorkshopItemInfo.Title,
                description = sOWorkshopItemInfo.Description,
                itemVisibility = sOWorkshopItemInfo.ItemVisibility,
                tags = sOWorkshopItemInfo.Tags,
                previewFileRelatePath = sOWorkshopItemInfo.PreviewFilePath.NotNullOrEmpty() ? new FileInfo(sOWorkshopItemInfo.PreviewFilePath).Name : "",

                //Set runtime
                itemLocation = WSItemLocation.UnityProject,
                //id=//PS:id可能尚未设置，所以暂不设置
                dirPath = sOWorkshopItemInfo.ItemDirPath,
            };
        }

        /// <summary>
        /// 通过打包后的Item所在目录，生成WorkshopItemInfo。
        /// </summary>
        /// <param name="itemDirPath"></param>
        /// <param name="itemLocation">有效值：Downloaded/UnityExported</param>
        /// <returns></returns>
        public virtual TItemInfo Create(string itemDirPath, WSItemLocation itemLocation = WSItemLocation.Downloaded)
        {
            TItemInfo inst = null;

            string jsonPath = GetItemJsonFileDir(itemDirPath);
            if (File.Exists(jsonPath))
            {
                var result = File.ReadAllText(jsonPath);
                inst = JsonConvert.DeserializeObject<TItemInfo>(result);//读取Dir目录的对应json文件并反序列化

                if (inst != null)
                {
                    //Set runtime
                    inst.itemLocation = itemLocation;
                    if (itemLocation == WSItemLocation.Downloaded)//已下载资源才有正确的ID
                    {
                        inst.id = WorkshopItemTool.GetId(itemDirPath);
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
        public virtual TItemInfo Create(Item item)
        {
            TItemInfo inst = null;

            string itemDir = item.Directory;
            string jsonPath = Path.Combine(itemDir, WorkshopItemInfo.ItemInfoFileName);
            if (File.Exists(jsonPath))
            {
                var result = File.ReadAllText(jsonPath);
                inst = JsonConvert.DeserializeObject<TItemInfo>(result);//读取Dir目录的对应json文件并反序列化
                if (inst != null)
                {
                    //Set runtime
                    inst.id = item.Id.Value;
                    inst.dirPath = itemDir;
                    inst.itemLocation = WSItemLocation.Downloaded;
                }
            }
            else
            {
                Debug.LogError($"Json file not exist for item {item.Title}!");
            }
            return inst;
        }

        public static bool IsValidItemDir(string itemDirPath)
        {
            return File.Exists(GetItemJsonFileDir(itemDirPath));
        }
        public static string GetItemJsonFileDir(string itemDirPath)
        {
            if (itemDirPath.IsNullOrEmpty())
                return "";
            return Path.Combine(itemDirPath, WorkshopItemInfo.ItemInfoFileName);
        }
    }
}