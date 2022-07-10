using System;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 读写Ini文件
/// 注意：
/// ——如果没有该文件，会自动生成
/// </summary>
public class IniSetting
{
    string mINIFileName = Application.dataPath + "/../config.ini";

    /// <summary>
    /// 初始化Ini路径
    /// </summary>
    /// <param name="filePath"></param>
    public IniSetting(string filePath)
    {
        mINIFileName = filePath;
    }

    public bool GetBool(string Key, string sectionName, bool defaultValue = false)
    {
        bool value = defaultValue;

        UnityAction<INIParser> action =
            (ini) =>
            {
                if (!ini.IsKeyExists(sectionName, Key))
                    ini.WriteValue(sectionName, Key, defaultValue);
                value = ini.ReadValue(sectionName, Key, defaultValue);
            };
        OpenAndReadINI(mINIFileName, action);
        Debug.LogWarning(Key + "  " + value);
        return value;
    }

    //sectionName：分类
    //Key:名
    //defaultValue：默认值
    public string GetString(string Key, string sectionName, string defaultValue = "")
    {
        string value = defaultValue;

        UnityAction<INIParser> action =
            (ini) =>
            {
                if (!ini.IsKeyExists(sectionName, Key))
                    ini.WriteValue(sectionName, Key, defaultValue);
                value = ini.ReadValue(sectionName, Key, defaultValue);
            };

        OpenAndReadINI(mINIFileName, action);
        return value;
    }

    public float GetFloat(string Key, string sectionName, float defaultValue = 0)
    {
        float value = defaultValue;

        UnityAction<INIParser> action =
            (ini) =>
            {
                if (!ini.IsKeyExists(sectionName, Key))
                    ini.WriteValue(sectionName, Key, defaultValue);
                value = (float)ini.ReadValue(sectionName, Key, defaultValue);
            };
        OpenAndReadINI(mINIFileName, action);
        Debug.LogWarning(Key + "  " + value);
        return value;

    }

    public int GetInt(string Key, string sectionName, int defaultValue = 0)
    {
        int value = defaultValue;

        UnityAction<INIParser> action =
            (ini) =>
            {
                if (!ini.IsKeyExists(sectionName, Key))
                    ini.WriteValue(sectionName, Key, defaultValue);
                value = ini.ReadValue(sectionName, Key, defaultValue);
            };
        OpenAndReadINI(mINIFileName, action);
        //Debug.LogWarning(Key + "  " + value);
        return value;
    }


    public void WriteString(string Key, string sectionName, string defaultValue = "")
    {
        UnityAction<INIParser> action =
            (ini) =>
            {
                ini.WriteValue(sectionName, Key, defaultValue);
            };
        OpenAndReadINI(mINIFileName, action);
    }

    public void WriteInt(string Key, string sectionName, int defaultValue = 0)
    {
        UnityAction<INIParser> action =
            (ini) =>
            {
                ini.WriteValue(sectionName, Key, defaultValue);
            };
        OpenAndReadINI(mINIFileName, action);
    }

    public void WriteBool(string Key, string sectionName, bool defaultValue = false)
    {
        UnityAction<INIParser> action =
            (ini) =>
            {
                ini.WriteValue(sectionName, Key, defaultValue);
            };
        OpenAndReadINI(mINIFileName, action);
    }

    public void WriteFloat(string Key, string sectionName, float defaultValue = 0)
    {
        UnityAction<INIParser> action =
            (ini) =>
            {
                ini.WriteValue(sectionName, Key, defaultValue);
            };
        OpenAndReadINI(mINIFileName, action);
    }

    static void OpenAndReadINI(string path, UnityAction<INIParser> actionRead)
    {
        try
        {
            if (!File.Exists(path))
            {
                DirectoryInfo dirParent = new DirectoryInfo(path).Parent;
                dirParent.Create();
                File.Create(path).Close();
                Debug.LogWarning("Create Success:" + path);
            }
            //else
            {
                INIParser iniParser = new INIParser();
                iniParser.Open(path);

                actionRead(iniParser);

                iniParser.Close();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("INIFile Not Found Or Create:\r\n" + path + "\r\n" + e);
        }
    }
}