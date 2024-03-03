using Newtonsoft.Json;
using Steamworks.Ugc;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Threeyes.Core;
using UnityEngine;
namespace Threeyes.Steamworks
{
    public interface IWorkshopItemInfoFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemDirPath"></param>
        /// <param name="itemLocation"></param>
        /// <returns>如果文件夹不存在或文件夹内的Mod文件格式错误，则返回null</returns>
        WorkshopItemInfo CreateBase(string itemDirPath, WSItemLocation itemLocation = WSItemLocation.Downloaded);
    }

    public abstract class WorkshopItemInfoFactory : IWorkshopItemInfoFactory
    {
        public abstract WorkshopItemInfo CreateBase(string itemDirPath, WSItemLocation itemLocation = WSItemLocation.Downloaded);
    }

    public class WorkshopItemInfoFactory<TInstance, TSOItemInfo, TItemInfo> : WorkshopItemInfoFactory, IWorkshopItemInfoFactory
        where TInstance : WorkshopItemInfoFactory<TInstance, TSOItemInfo, TItemInfo>, new()
        where TSOItemInfo : SOWorkshopItemInfo
        where TItemInfo : WorkshopItemInfo, new()
    {
        public static readonly TInstance Instance = new TInstance();

        public override WorkshopItemInfo CreateBase(string itemDirPath, WSItemLocation itemLocation = WSItemLocation.Downloaded)
        {
            return Create(itemDirPath, itemLocation);
        }

        /// <summary>
        /// 【EditorOnly】仅在UnityEditor Build Mod时调用，用于生成完整的ItemInfo用于序列化
        /// </summary>
        /// <param name="sOWorkshopItemInfo"></param>
        public virtual TItemInfo Create(TSOItemInfo sOWorkshopItemInfo)
        {
            TItemInfo inst = CreateFunc(sOWorkshopItemInfo);
            return inst;
        }

        protected virtual TItemInfo CreateFunc(TSOItemInfo sOWorkshopItemInfo)
        {
            return new TItemInfo()
            {
                //Set basic
                title = sOWorkshopItemInfo.Title,
                description = sOWorkshopItemInfo.Description,
                modFileRelatePath = sOWorkshopItemInfo.ItemModFileName,
                previewFileRelatePath = sOWorkshopItemInfo.PreviewFilePath.NotNullOrEmpty() ? new FileInfo(sOWorkshopItemInfo.PreviewFilePath).Name : "",
                itemVisibility = sOWorkshopItemInfo.ItemVisibility,
                tags = sOWorkshopItemInfo.Tags,

                //Set runtime
                itemLocation = WSItemLocation.UnityProject,
                //id=//PS:id可能尚未设置，所以暂不设置
                dirPath = sOWorkshopItemInfo.ItemDirPath,
            };
        }



        /// <summary>
        /// 【Runtime】通过读取打包后的Item所在目录的Json信息，生成WorkshopItemInfo。
        /// 用途：
        /// -通用的UI浏览器
        /// </summary>
        /// <param name="itemDirPath"></param>
        /// <param name="itemLocation">有效值：Downloaded/UnityExported</param>
        /// <returns></returns>
        public virtual TItemInfo Create(string itemDirPath, WSItemLocation itemLocation = WSItemLocation.Downloaded)
        {
            TItemInfo itemInfoInst = null;

            string jsonPath = WorkshopItemTool.GetItemJsonFileDir(itemDirPath);
            if (File.Exists(jsonPath))
            {
                var result = File.ReadAllText(jsonPath);
                itemInfoInst = JsonConvert.DeserializeObject<TItemInfo>(result);//读取Dir目录的对应json文件并反序列化

                if (itemInfoInst != null)
                {
                    //Set runtime
                    itemInfoInst.itemLocation = itemLocation;
                    if (itemLocation == WSItemLocation.Downloaded)//已下载资源才有正确的ID
                    {
                        itemInfoInst.id = WorkshopItemTool.GetId(itemDirPath);
                    }
                    else
                    {
                        //(Todo:针对测试路径的Item，通过title生成唯一的ID(以-开头作为区分）（非必要，经查阅，没有能够生成唯一long的方法）
                    }
                    itemInfoInst.dirPath = itemDirPath;

                    //ToUpdate: 如果本地已经存在Item.json, 则从中获取fileSize
                    DirectoryInfo directoryInfo = new DirectoryInfo(itemDirPath);
                    if (directoryInfo.Exists)
                        itemInfoInst.fileSize = directoryInfo.EnumerateFiles().Sum(file => file.Length);
                }
            }
            else
            {
                Debug.LogError($"Json file not exist in dir: {jsonPath}!");
            }
            return itemInfoInst;
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

    }
}