using System.Collections;
using System.Collections.Generic;
using Threeyes.GameFramework;
using UnityEngine;

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

    public static string Data_Save_SystemDirPath { get { return GameFramework_PathDefinition.Data_SaveDirPath + "/System"; } }//存储系统信息的根目录
    public static string Data_Save_System_CursorDirPath { get { return Data_Save_SystemDirPath + "/Cursor"; } }//存储系统光标信息的目录
}
