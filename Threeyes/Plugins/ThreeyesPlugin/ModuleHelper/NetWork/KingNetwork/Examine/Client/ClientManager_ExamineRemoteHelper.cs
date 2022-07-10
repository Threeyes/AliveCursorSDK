using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientManager_ExamineRemoteHelper : ComponentRemoteHelperBase<DataBaseManager_UserPlayHistory>
{
    public override DataBaseManager_UserPlayHistory ManagerInstance { get { return DataBaseManager_UserPlayHistory.Instance; } }   
    public ClientManager_Examine clientManagerInst { get { return ClientManager_Examine.Instance; } }

    public ValueChanger_Float valueChangerScore;//得分对应的ValueChanger

    //——各模块得分——
    /// <summary>
    /// 当前模块的信息，后期会插入数据库中，开发者需要修改该数据中除Score以外的字段
    /// </summary>
    [Header("Module")]
    public UserPlayHistoryInfo.ModulePlayRecord curModulePlayRecord = new UserPlayHistoryInfo.ModulePlayRecord();


    /// <summary>
    /// #1 初始化新记录实例
    /// </summary>
    public void InitPlayHistory()
    {
#if USE_SimpleSQL
        ManagerInstance.InitPlayHistory();
#endif
    }

    /// <summary>
    /// #2 插入子模块记录
    /// </summary>
    public void AddModulePlayRecord()
    {
#if USE_SimpleSQL
        if (!valueChangerScore)
        {
            Debug.LogError("需要先设置valueChangerScore的值!");
            return;
        }
        AddModulePlayRecord(valueChangerScore.CurValue);
#endif
    }

    /// <summary>
    /// #2 插入子模块记录
    /// </summary>
    public void AddModulePlayRecord(float moduleScore)
    {
#if USE_SimpleSQL
        curModulePlayRecord.ModuleScore = moduleScore;
        curModulePlayRecord.FinishTime = DateTime.Now;
        ManagerInstance.AddModulePlayRecord(curModulePlayRecord);
#endif
    }

    /// <summary>
    /// #3 插入记录到数据库，并回传给服务器
    /// </summary>
    public void InsertPlayHistoryAndSendToServer()
    {
#if USE_SimpleSQL
        ManagerInstance.InsertPlayHistory();
        clientManagerInst.SendCurPlayHistoryToServer();
#endif
    }

    ////——仅计算总分（ToDelete）——
    ///// <summary>
    ///// 使用当前的ValueChanger值
    ///// </summary>
    //public void InsertPlayHistoryAndSendToServer()
    //{
    //    if (!valueChangerScore)
    //    {
    //        Debug.LogError("valueChangerScore is null!");
    //        return;
    //    }

    //    InsertPlayHistoryAndSendToServer(valueChangerScore.CurValue);
    //}

    //public void InsertPlayHistoryAndSendToServer(string score)
    //{
    //    float result = score.TryParse<float>();
    //    if (result != default(float))
    //        InsertPlayHistoryAndSendToServer(result);
    //    else
    //    {
    //        Debug.LogError("string 转换失败！");
    //    }
    //}

    //public void InsertPlayHistoryAndSendToServer(float score)
    //{
    //    clientManagerInst.InsertPlayHistoryAndSendToServer(score);
    //}

}
