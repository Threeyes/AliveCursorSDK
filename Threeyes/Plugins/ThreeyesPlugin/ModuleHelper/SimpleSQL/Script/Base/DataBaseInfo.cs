using System;
using UnityEngine;
#if USE_SimpleSQL
using SimpleSQL;
#endif
/// <summary>
/// 数据库的信息，会在运行时自动插入
/// </summary>
[System.Serializable]
public class DataBaseInfo
{
    [SerializeField] private int id = 1;//用于UpdateTable的唯一标识
    [SerializeField] private string dBVersion = "1.0";//数据库版本，每个数据库都要有（格式eg:1.1)

#if USE_SimpleSQL
    [PrimaryKey]
#endif
    public int ID { get { return id; } set { id = value; } }
    public string DBVersion { get { return dBVersion; } set { dBVersion = value; } }
}
