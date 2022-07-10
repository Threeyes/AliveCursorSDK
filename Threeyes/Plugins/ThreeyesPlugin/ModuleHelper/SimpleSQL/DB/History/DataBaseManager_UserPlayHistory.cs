using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if USE_JsonDotNet
using Newtonsoft.Json;
#endif
#if USE_SimpleSQL
using SimpleSQL;
#endif

/// <summary>
/// 用户使用历史
/// PS：这是项目管理的数据类，需要与考核的数据类同时写入，保证统一性（可通过传参）
/// （第三方程序的数据库都需要继承该类，以保证统一性。子类的命名规范为UserPlayHistory_项目名）
/// 
/// 实现方式：
/// 1.项目管理必须要记录，考核程序完成一次记录后保存信息到本地数据表中，然后通过传递FinishTime通知项目管理，让项目管理获取考核程序数据表的通用信息（通过sql语句获取UserPlayHistory的相关字段）
/// 
/// ToAdd：退出前将DB备份到持久化目录
/// ToAdd:通过UID查询到用户名（如果查不到（如用户被删掉）那就默认用UID）
/// ToAdd:默认.byte没有数据表.首次运行先创建表，这样能避免更改表后出错
/// </summary>

public class DataBaseManager_UserPlayHistory : DataBaseManagerBase<DataBaseManager_UserPlayHistory, UserPlayHistoryInfo>
{

#if USE_SimpleSQL
    public List<UserPlayHistoryInfo> debugListData = new List<UserPlayHistoryInfo>();
    public UserPlayHistoryInfo debugData = new UserPlayHistoryInfo();

    //便于通过数据类生成表
    [ContextMenu("TryCreateTable")]
    public void TryCreateTable()
    {
        CreateTable();
    }

    #region 分模块得分

    //——分模块得分——
    ///插入流程:
    ///1.InitHistory(程序开始时调用）
    ///2.AddPlayRecord（每个模块完成后调用）
    ///3.Insert（程序完成后调用）

    /// <summary>
    /// #1 初始化新记录实例
    /// </summary>
    public void InitPlayHistory()
    {
        UserLoginInfo userLoginInfo = DataBaseManager_UserLogin.Instance.CurData ?? UserLoginInfo.defaultInst;
        if (userLoginInfo == null)
        {
            Debug.LogError("Cur UserLoginInfo is null!");
            return;
        }

        UserPlayHistoryInfo userHistory = new UserPlayHistoryInfo()
        {
            UnionID = userLoginInfo.UnionID,

            //自身的信息
            AppName = Application.productName,
            AppRawName = Application.productName,
            StartTime = DateTime.Now
        };
        CurData = userHistory;

        Debug.Log("#1 InitPlayHistory");
    }

    /// <summary>
    /// #2 插入子模块记录
    /// </summary>
    /// <param name="modulePlayRecord"></param>
    public void AddModulePlayRecord(UserPlayHistoryInfo.ModulePlayRecord modulePlayRecord)
    {
        if (CurData == null)
        {
            Debug.LogError("CurData is null! 请先调用InitPlayHistory！");
            return;
        }
        CurData.ListModulePlayRecord.Add(modulePlayRecord);

        Debug.Log("#2 AddModulePlayRecord: " + modulePlayRecord.ModuleName + " and score: " + modulePlayRecord.ModuleScore);
    }

    /// <summary>
    /// #3 插入记录到数据库
    /// </summary>
    public void InsertPlayHistory()
    {
        if (CurData == null)
        {
            Debug.LogError("CurData is null! 请先调用InitPlayHistory！");
            return;
        }
        CurData.FinishTime = DateTime.Now;
        Insert(CurData);
        Debug.Log("InsertPlayHistory for: " + CurData.UnionID);
    }

    #endregion
    public override int Insert(UserPlayHistoryInfo data)
    {
        int result = base.Insert(data);
        CurData = data;
        //Todo:刷新列表
        return result;
    }

    public override List<UserPlayHistoryInfo> QueryAllWithOrder()
    {
        return QueryAll().OrderByDescending(d => d.FinishTime).ToList();
    }

#endif
}

/// <summary>
/// 用户使用信息
/// </summary>
[Serializable]
public class UserPlayHistoryInfo : SerializationDeSerializationData<UserPlayHistoryInfo>
{
    [PrimaryKey, AutoIncrement]
    public int ID { get; set; }
    public string UnionID { get { return unionid; } set { unionid = value; } }
    public float Score { get { return score; } set { score = Mathf.Clamp(value, 0, 100); } }
    public string AppName { get { return appName; } set { appName = value; } }
    public string AppRawName { get { return appRawName; } set { appRawName = value; } }
    public DateTime StartTime { get { return startTime; } set { startTime = value; } }
    public DateTime FinishTime { get { return finishTime; } set { finishTime = value; } }
    [MaxLength(1000000000)]//设置text的最大长度
    public string SerializedListModulePlayRecord
    {
        get
        {
#if USE_JsonDotNet
            return JsonConvert.SerializeObject(ListModulePlayRecord);//实时序列化
#else
            return "";
#endif
        }
        set
        {
#if USE_JsonDotNet
            ListModulePlayRecord = new List<ModulePlayRecord>();
            try
            {
                ListModulePlayRecord = JsonConvert.DeserializeObject<List<ModulePlayRecord>>(value);
            }
            catch (Exception e)
            {
                Debug.LogError("DeserializeObject  failed! The origin data is:\r\n" + value + "\r\n" + e);
            }
#endif
        }
    }

    [SerializeField] private string unionid;//用户统一标识（类似身份证号）
    [SerializeField] private float score;//总得分[0,100]
    [SerializeField] private string appName;//应用名称（如果是项目管理方存储，应使用其DisplayName代替）
    [SerializeField] private string appRawName;//应用原名，方便回溯
    [SerializeField] private DateTime startTime;//开始时间
    [SerializeField] private DateTime finishTime;//完成时间
    [Ignore] public List<ModulePlayRecord> ListModulePlayRecord = new List<ModulePlayRecord>();//本应用中，所有子模块的得分记录（通过序列化方式存储，而不是直接存储到DB中）


    public static UserPlayHistoryInfo defaultInst = new UserPlayHistoryInfo() { unionid = UserLoginInfo.defaultInst.UnionID };
    public static UserPlayHistoryInfo Deserialize(string content)
    {
        return defaultInst.DeserializeFunc(content);
    }

    /// <summary>
    /// 获取用户相关信息，可能返回null
    /// </summary>
    /// <returns></returns>
    public UserLoginInfo GetUserLoginInfo()
    {
#if USE_SimpleSQL
        return DataBaseManager_UserLogin.Instance.QueryByUID(UnionID);
#else
        return null;
#endif
    }

    /// <summary>
    /// 玩家在应用子模块的记录
    /// </summary>
    [Serializable]
    public class ModulePlayRecord
    {
        public string ModuleName { get { return moduleName; } set { moduleName = value; } }
        public float ModuleScore { get { return moduleScore; } set { moduleScore = Mathf.Clamp(value, 0, 100); } }
        public string Description { get { return description; } set { description = value; } }
        public DateTime FinishTime { get { return finishTime; } set { finishTime = value; } }

        [SerializeField] private string moduleName;//本模块的名称（如：消防标识学习）
        [SerializeField] [Range(0, 100)] private float moduleScore;//本模块得分
        [SerializeField] private string description;//模块描述（如：本模块的目的是教导学生如何识别标识）
        [SerializeField] private DateTime finishTime;//完成时间
    }
}