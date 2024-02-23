using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Threeyes.Core;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Threeyes.Steamworks
{
    public static class Steamworks_PathDefinition
    {
        #region  ——Common——
        /// <summary>
        /// The Root Folder for streamingAssetsPath
        /// </summary>
        public static string StreamingAssetsFolder { get { return PathTool.ConvertToSystemFormat(Application.streamingAssetsPath); } }
        public static string appNameWithoutExtension { get { return SORuntimeSettingManager.Instance.productName; } }
        public static string appName { get { return appNameWithoutExtension + ".exe"; } }
        public static string appFilePath
        {
            get
            {

                if (Application.isEditor)
                {
                    //ToUpdate：放在SO中作为调试字段
                    return "E:\\Unity Group\\Work Group\\App Group\\AliveCursor\\Build Group\\Threeyes\\AliveCursor\\" + appName;
                }
                else
                {
                    return PathTool.ConvertToSystemFormat(PathTool.GetParentDirectory(Application.dataPath).FullName + "\\" + appName);
                }
            }
        }

        public static string licenseFilePath { get { return StreamingAssetsFolder + @"\Licenses\LicenseCollection.md"; } }

        public static string logFilePath
        {
            get
            {
                string appData = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);

                return $@"{appData}\..\LocalLow\{Application.companyName}\{Application.productName}\Player.log";
            }
        }

        /// <summary>
        /// The folder for Windows Notify icons
        /// </summary>
        public static string SystemNotifyIconsFolder { get { return StreamingAssetsFolder + @"\System\NotifyIcons"; } }


        #endregion

        #region  ——Editor——

        public static readonly string ItemRootDirName = "Items";
        /// <summary>
        /// [Unity Editor]:存放所有Item的父文件夹
        /// </summary>
        public static string ItemParentDirPath
        {
            get
            {
                return Application.dataPath + "/" + ItemRootDirName;
            }
        }
        #endregion

        #region ——Save——
        ///
        ///PS：
        ///1.暂不支持以SteamID作为细分的存储，因为需求不多，避免增加复杂性
        ///2.对应的云存储文件夹为Item（SteamCloud存储路径（https://partner.steamgames.com/doc/features/cloud)）
        /// 目录实例：
        /// Data
        /// ——Save
        /// ————Setting【Program Setting】
        /// ————Log【存储各Mod的Log文件】
        /// ——————（Mod ID）
        /// ————Item【存储Mod的持久化数据】
        /// ——————（Mod ID）
        /// ————————Persistent

        /// <summary>
        /// 存储实时生成数据的根目录
        /// </summary>
        static string DataPath
        {
            get
            {
                string parentDir = PathTool.ProjectDirPath;

                //打包后将Data文件夹放到AC上级目录中。优点：上传程序到Steam时不会将该目录的测试文件进行上传
                if (!Application.isEditor)
                {
                    parentDir = PathTool.GetParentDirectory(parentDir).FullName;
                }

                return parentDir + "/" + dataFolderName;
            }
        }
        public static readonly string dataFolderName = "Data";
        public static readonly string persistentFolderName = "Persistent";

        public static string Data_SaveDirPath { get { return DataPath + "/Save"; } }//存储需保存信息的目录

        public static string Data_Save_LogDirPath { get { return Data_SaveDirPath + "/Log"; } }//Item对应Log文件的根目录
        public static string Data_Save_SettingDirPath { get { return Data_SaveDirPath + "/Setting"; } }//存储项目设置的根目录
        public static string Data_Save_ItemDirPath { get { return Data_SaveDirPath + "/Item"; } }//Item的根目录（通过SteamCloud同步）
        public static string Data_Save_ItemLocalDirPath { get { return Data_SaveDirPath + "/Item_Local"; } }//本地Item的根目录（类似系统的AppData\Local，只存储在本地）

        #endregion

        #region ——Export——

        internal static bool UseModUploaderAsUnityExported { get; set; }//使用ModUploader作为导出路径
        /// <summary>
        /// AliveCursor_ModUploader对应的Item文件夹
        /// </summary>
        public static string ModUploader_ExportItemRootDirPath
        {
            get
            {
                string productName = SORuntimeSettingManager.Instance.productName;
#if UNITY_EDITOR
                if (UseModUploaderAsUnityExported)
                    return PathTool.ProjectDirPath + @$"\..\{productName}_ModUploader\Export\Items";
                return ExportItemRootDirPath;//本项目路径
#else
			return PathTool.ProjectDirPath + @$"\..\..\..\{productName}_ModUploader\Export\Items";
#endif
            }
        }

        /// <summary>
        /// （外部）存放所有打包Item的父文件夹
        /// </summary>
        public static string ExportItemRootDirPath
        {
            get
            {
                if (Application.isEditor)
                    return PathTool.ProjectDirPath + "/Export/" + ItemRootDirName;
                else
                {

                    return @$"D:\Unity Group\{Application.productName}\Export\Items";//ToUpdate：改为本盘符的目录，或者直接报错
                }
            }
        }

        /// <summary>
        /// The folder for BuiltIn Items
        /// </summary>
        public static string BuiltInItemsFolder { get { return StreamingAssetsFolder + @"\BuiltInItems"; } }

        /// <summary>
        /// BuiltIn Default Item's Path
        /// </summary>
        public static string DeafultWorkshopItemPath { get { return BuiltInItemsFolder + @"\Default"; } }

        #endregion

        #region Utility

        /// <summary>
        /// 是否为该项目Assets内部的文件
        /// </summary>
        /// <param name="absFilePath"></param>
        /// <returns></returns>
        public static bool IsProjectAssetFile(string absFilePath)
        {
            return IsFileOfDir(absFilePath, Application.dataPath);
        }

        /// <summary>
        /// 检查文件是否为文件夹的子文件
        /// </summary>
        /// <param name="absFilePath"></param>
        /// <param name="absDirPath"></param>
        /// <returns></returns>
        public static bool IsFileOfDir(string absFilePath, string absDirPath)
        {
            //统一转为C#格式，避免分隔符不统一
            FileInfo fileInfoSelected = new FileInfo(absFilePath);
            DirectoryInfo directoryInfoItemPath = new DirectoryInfo(absDirPath);

            return fileInfoSelected.FullName.Contains(directoryInfoItemPath.FullName);
        }

        /// <summary>
        /// 将绝对路径转为局部路径
        /// </summary>
        /// <param name="fileAbsPath"></param>
        /// <returns></returns>
        public static string GetFileLocalPath(string fileAbsPath)
        {
            FileInfo fileInfoSelected = new FileInfo(fileAbsPath);
            string filelocalPath = fileInfoSelected.FullName.Remove(new DirectoryInfo(PathTool.ProjectDirPath).FullName + Path.DirectorySeparatorChar);//获取其在项目中的局部路径。PS:C# Path统一使用的是'\'分割
            return filelocalPath;
        }


        /// <summary>
        /// 通过绝对路径加载文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileAbsPath"></param>
        /// <returns>If the asset is not found, returns Null</returns>
        public static T LoadAssetAtAbsPath<T>(string fileAbsPath) where T : Object
        {
            T inst = default;

#if UNITY_EDITOR

            FileInfo fileInfoSelected = new FileInfo(fileAbsPath);
            string filelocalPath = fileInfoSelected.FullName.Remove(new DirectoryInfo(PathTool.ProjectDirPath).FullName + Path.DirectorySeparatorChar);//获取其在项目中的局部路径。PS:C# Path统一使用的是'\'分割                                                                                                                                                          
            //Debug.Log("Get local path: " + filelocalPath);
            inst = AssetDatabase.LoadAssetAtPath<T>(filelocalPath);
#else
        Debug.LogError("Not working on runtime!");
#endif
            return inst;
        }

        #endregion
    }
}