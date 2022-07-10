using Newtonsoft.Json;
using Steamworks.Ugc;
using System.IO;
using System.Linq;
using UnityEngine;


public static class AC_WorkshopItemInfoFactory
{
    /// <summary>
    /// 
    /// PS����EditorOnly������UnityEditor Build Modʱ����
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
            //id=//PS:id������δ���ã������ݲ�����
            dirPath = sOWorkshopItemInfo.ItemDirPath,
        };
    }

    public static bool IsValidDir(string itemDirPath, AC_WCItemLocation itemLocation = AC_WCItemLocation.Downloaded)
    {
        string jsonPath = Path.Combine(itemDirPath, AC_WorkshopItemInfo.ItemInfoFileName);
        return File.Exists(jsonPath);
    }

    /// <summary>
    /// ͨ��������Item����Ŀ¼������WorkshopItemInfo��
    /// </summary>
    /// <param name="itemDirPath"></param>
    /// <param name="itemLocation">��Чֵ��Downloaded/UnityExported</param>
    /// <returns></returns>
    public static AC_WorkshopItemInfo Create(string itemDirPath, AC_WCItemLocation itemLocation = AC_WCItemLocation.Downloaded)
    {
        AC_WorkshopItemInfo inst = null;

        string jsonPath = Path.Combine(itemDirPath, AC_WorkshopItemInfo.ItemInfoFileName);
        if (File.Exists(jsonPath))
        {
            var result = File.ReadAllText(jsonPath);
            inst = JsonConvert.DeserializeObject<AC_WorkshopItemInfo>(result);//��ȡDirĿ¼�Ķ�Ӧjson�ļ��������л�

            if (inst != null)
            {
                //Set runtime
                inst.itemLocation = itemLocation;
                if (itemLocation == AC_WCItemLocation.Downloaded)//��������Դ������ȷ��ID
                {
                    inst.id = AC_WorkshopItemTool.GetId(itemDirPath);
                }
                else
                {
                    //(Todo:��Բ���·����Item��ͨ��title����Ψһ��ID(��-��ͷ��Ϊ���֣����Ǳ�Ҫ�������ģ�û���ܹ�����Ψһlong�ķ�����
                }
                inst.dirPath = itemDirPath;

                //ToUpdate: ��������Ѿ�����Item.json, ����л�ȡfileSize
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
    /// (PS:һ�㲻��Ҫʹ�ã�����ɾ��)
    /// ͨ��Mod���غ��Json�ļ������л�
    /// (��Ϊ�Ƿ����л�������ֻ��ͨ����̬�������ɣ�
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
            inst = JsonConvert.DeserializeObject<AC_WorkshopItemInfo>(result);//��ȡDirĿ¼�Ķ�Ӧjson�ļ��������л�
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