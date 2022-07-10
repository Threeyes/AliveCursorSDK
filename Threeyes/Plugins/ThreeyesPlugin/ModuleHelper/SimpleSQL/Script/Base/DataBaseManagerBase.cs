using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using System.IO;

#if USE_SimpleSQL
using SimpleSQL;
#endif

/// <summary>
/// 管理 SimpleSQLManager
/// </summary>
public class DataBaseManagerBase
    <TInst> : InstanceBase<TInst>
   where TInst : DataBaseManagerBase<TInst>
{
    public readonly string defaultFileName = "MainDataBase.sqlite";//MainDataBase.sqlite
    /// <summary>
    /// 存储在本地的指定位置
    /// </summary>
    public bool isSaveToRelateDir = true;//存在本地的相对路径(Data/DataBase)
    public ExternalFileLocation externalFileLocation = ExternalFileLocation.CustomData;

    [Header("DataBaseInfo")]
    [SerializeField] protected DataBaseInfo curDataBaseInfo = new DataBaseInfo();//当前项目的数据库信息，开发者手动修改其参数
    protected DataBaseInfo savedDataBaseInfo = new DataBaseInfo();//数据库中的信息


#if USE_SimpleSQL

    public SimpleSQLManager comp;//待绑定的组件
    public SimpleSQLManager Comp
    {
        get
        {
            if (!comp)
                comp = GetComponent<SimpleSQLManager>();
            return comp;
        }
        set
        {
            comp = value;
        }
    }

    public override void SetInstance()
    {
        base.SetInstance();

        InitPath();
    }

    protected virtual void Start()
    {
        UpdateDBInfo();//开始前更新数据库版本等信息
    }

    protected void InitPath()
    {
        if (Comp.overridePathMode == SimpleSQLManager.OverridePathMode.RelativeToPersistentData)//持久化路径
        {
            if (Comp.overrideBasePath.NotNullOrEmpty())//需要提前保证文件夹存在
            {
                string dbPath = PathTool.GetOrCreateDir(Path.Combine(Application.persistentDataPath, Comp.overrideBasePath));
                Debug.Log(this.GetType() + " DB Path:\r\n" + dbPath);
            }
        }
        else//绝对路径
        {
            if (isSaveToRelateDir)//运行时设置
            {
                //设置路径为实时获取的相对路径
                string dbPath = PathTool.GetPath(externalFileLocation, PathDefinition.dataBaseFolderName, false);
                PathTool.GetOrCreateDir(dbPath);
                Comp.overrideBasePath = dbPath;

                Debug.Log("DB Path:\r\n" + dbPath);
            }
        }
    }

    /// <summary>
    /// 更新数据库的信息
    /// </summary>
    protected virtual void UpdateDBInfo()
    {
        try
        {
            Comp.CreateTable<DataBaseInfo>();//创建表(PS:会避免重复建表）

            //ToAdd:因为不是必须Table，所以要增加判断是否有该Table
            savedDataBaseInfo = Comp.QueryAll<DataBaseInfo>().FirstOrDefault();
            if (savedDataBaseInfo == null)//首次创表
            {
                //插入初值
                Comp.Insert(curDataBaseInfo);
                Debug.Log("初始化DataBaseInfo");
            }
            else
            {
                if (savedDataBaseInfo.DBVersion != curDataBaseInfo.DBVersion)
                {
                    //Todo:判断两个版本的大小
                    float fCurVersion = curDataBaseInfo.DBVersion.TryParse<float>();
                    float fSavedVersion = savedDataBaseInfo.DBVersion.TryParse<float>();
                    bool isNewerVersion = false;
                    if (fCurVersion != default(float) && fSavedVersion != default(float))
                    {
                        isNewerVersion = fCurVersion > fSavedVersion;
                    }
                    else
                    {
                        Debug.LogError("DBVersion转换失败! 请检查DBVersion的格式是否正确");
                        return;
                    }
                    if (isNewerVersion)
                    {
                        UpdateDBInfoFunc(savedDataBaseInfo.DBVersion, curDataBaseInfo.DBVersion);
                        Debug.Log("更新DataBaseInfo.DBVersion: " + savedDataBaseInfo.DBVersion + "->" + curDataBaseInfo.DBVersion);
                        savedDataBaseInfo.DBVersion = curDataBaseInfo.DBVersion;
                        Comp.UpdateTable(savedDataBaseInfo, typeof(DataBaseInfo));
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("UpdateDBInfo with error: \r\n" + e);
        }
    }

    protected virtual void UpdateDBInfoFunc(string lastVersion, string curVersion)
    {
        //在这里更新表内容，具体参考SimpleSQL UserManual.pdf的（13.2 Upgrade Path）
    }
#endif
}

/// <summary>
/// 绑定主要数据类
/// </summary>
/// <typeparam name="TInst"></typeparam>
/// <typeparam name="TDBData"></typeparam>
public class DataBaseManagerBase<TInst, TDBData> : DataBaseManagerBase<TInst>
    where TInst : DataBaseManagerBase<TInst, TDBData>
    where TDBData : new()
{

    /// <summary>
    /// 当前正在处理的数据（如当前用户）
    /// </summary>
    public virtual TDBData CurData
    {
        get { return curData; }
        set
        {
            curData = value;
            debugCurData = value;
            onCurDataUpdate.Invoke();
        }
    }


    [Header("Managed Data")]
    //Warning:该数据类不能标识为[Serializable]，因为要用它是否为null来判断数据是否有效
    protected TDBData curData = default(TDBData);//默认是Null
    [SerializeField] protected TDBData debugCurData = new TDBData();//可视化显示当前数据

    /// <summary>
    /// 当前数据有更新（但未存入数据库）
    ///     常用于:刷新对应UI
    /// </summary>
    public UnityEvent onCurDataUpdate;

    /// <summary>
    /// 保存的数据有更新
    ///     触发条件：Insert、Update数据
    ///     常用于：重新获取所有清单并刷新UI
    /// </summary>
    public UnityEvent onSavedDataUpdate;

#if USE_SimpleSQL
    #region DB Utility（PS：尽量用以下方法，因为会调用对应的Event）(应为内部方法，外部调用不需要了解数据库语句）

    //——查询——
    public virtual List<TDBData> QueryAll()
    {
        return Comp.QueryAll<TDBData>();
    }

    /// <summary>
    /// 根据时间、ID等进行排序的全部信息
    /// </summary>
    /// <returns></returns>
    public virtual List<TDBData> QueryAllWithOrder()
    {
        return QueryAll();
    }

    //——创表——

    /// <summary>
    /// 创表
    /// 适用于：
    ///     1.如果不想在.bytes文件中手动创建Table，可以调用这个方法生成对应Table，然后替换原.byte
    /// </summary>
    public virtual void CreateTable()
    {
        Comp.CreateTable<TDBData>();
    }

    //——修改数据——


    /// <summary>
    /// 针对标记为[PrimaryKey]的唯一数据（如用户信息），保证只有一条且最新
    /// </summary>
    /// <param name="data"></param>
    public virtual void InsertOrUpdate(TDBData data)
    {
        //PS:对应的SQL语句为INSERT ... ON DUPLICATE KEY UPDATE（https://blog.csdn.net/ghsau/article/details/23557915）

        int totalUpdated = UpdateTable(data);//尝试更新
        if (totalUpdated == 0)//数据库中不存在该数据，需要新增
        {
            Insert(data);
            //Debug.Log("Data Inserted");
        }
        else
        {
            //Debug.Log("Data Updated");
        }
    }

    /// <summary>
    /// 插入数据
    /// （Warning：遇到重复且带有[PrimaryKey]的数据会报错，此时应使用InsertOrUpdate代替
    /// </summary>
    /// <param name="data"></param>
    public virtual int Insert(TDBData data)
    {
        onSavedDataUpdate.Execute();
        int result = 0;
        try
        {
            //PS:该方法容易报错
            Comp.Insert(data);
        }
        catch (Exception e)
        {
            Debug.LogError("Insert with error:\r\n" + e);
        }
        return result;
    }

    /// <summary>
    ///  Updates all of the columns of a table using the specified object except for its primary key. The object is required to have a primary key.
    ///  PS：不能直接叫Update，会与Mono冲突
    /// </summary>
    /// <param name="data"></param>
    /// <returns>The number of rows updated.</returns>
    public virtual int UpdateTable(TDBData data)
    {
        onSavedDataUpdate.Execute();
        return Comp.UpdateTable(data);
    }

    #endregion
#endif
}


public static class SimpleSQLManagerExtension
{
#if USE_SimpleSQL
    public static List<TData> QueryAll<TData>(this SimpleSQLManager simpleSQLManager)
    where TData : new()
    {
        string typeName = typeof(TData).Name;
        string sql = "SELECT * FROM " + typeName;
        List<TData> listCacheData = simpleSQLManager.Query<TData>(sql);
        return listCacheData;
    }


    ///常用SQL命令：
    /// 查询所有表：select name from sqlite_master where type='table' order by name

#endif
}