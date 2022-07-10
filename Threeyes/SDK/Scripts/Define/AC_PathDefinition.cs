using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class AC_PathDefinition
{
    //——Save文件夹——
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
    /// ————System【System info】
    /// ——————Cursor
    /// ————————CursorTheme.json

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

    public static string Data_Save_ItemDirPath { get { return Data_SaveDirPath + "/Item"; } }//Item的根目录

    public static string Data_Save_SystemDirPath { get { return Data_SaveDirPath + "/System"; } }//存储系统信息的根目录
    public static string Data_Save_System_CursorDirPath { get { return Data_Save_SystemDirPath + "/Cursor"; } }//存储系统光标信息的目录

    //——Editor——

    static readonly string ItemRootDirName = "Items";
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


    //——Export——

    /// <summary>
    /// （外部）存放所有打包Item的父文件夹
    /// </summary>
    public static string ExportItemRootDirPath
    {
        get
        {
            if (Application.isEditor)
                return PathTool.ProjectDirPath + "/Export/" + ItemRootDirName;
            return @"D:\Unity Group\AliveCursor\Export\Items";
        }
    }

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
        T inst = default(T);

#if UNITY_EDITOR

        FileInfo fileInfoSelected = new FileInfo(fileAbsPath);
        string filelocalPath = fileInfoSelected.FullName.Remove(new DirectoryInfo(PathTool.ProjectDirPath).FullName + Path.DirectorySeparatorChar);//获取其在项目中的局部路径。PS:C# Path统一使用的是'\'分割                                                                                                                                                          //Debug.Log("Get local path: " + filelocalPath);
        inst = AssetDatabase.LoadAssetAtPath<T>(filelocalPath);
#else
        Debug.LogError("Not working on runtime!");
#endif
        return inst;
    }

    #endregion
}
