using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if USE_KingNetwork
using KingNetwork.Shared.Interfaces;
#endif
/// <summary>
/// 测试程序 （客户端）
/// (ToUpdate:改用Mirror的传送事件方式）
/// </summary>
public class ClientManager_Examine : ClientManagerBase<ClientManager_Examine, ColyuPacketType>
{
    DataBaseManager_UserPlayHistory dataBaseManager_UserPlayHistoryInst { get { return DataBaseManager_UserPlayHistory.Instance; } }

    /// <summary>
    /// 发送指定信息到服务器
    /// 
    /// </summary>
    public void SendCurPlayHistoryToServer()
    {
#if USE_KingNetwork

        if (!dataBaseManager_UserPlayHistoryInst)
        {
            Debug.LogError("找不到DataBaseManager_UserPlayHistory单例！");
            return;
        }

        //#2 发送当前UserPlayHistoryInfo
        if (!client.IsValid(true))
            return;
        UserPlayHistoryInfo userPlayHistory = dataBaseManager_UserPlayHistoryInst.CurData;
        if (userPlayHistory.IsNull())
        {
            Debug.LogError("UserPlayHistoryInfo.CurData is null! 请确定有无初始化数据!");
            return;
        }
        else
        {
            SendMessageToServer(ColyuPacketType.DB_UserPlayHistory, userPlayHistory.GetSerializeString());
        }
#endif
    }

#if USE_KingNetwork
    protected override void OnMessageReceivedFunc(ColyuPacketType en, IKingBufferReader reader)
    {
        switch (en)
        {
            //缓存登录信息
            case ColyuPacketType.DB_UserLoginInfo:
                var managerInst = DataBaseManager_UserLogin.Instance;
                if (managerInst)
                {
                    string serializedInfo = reader.ReadString();
                    UserLoginInfo userLoginInfo = UserLoginInfo.Deserialize(serializedInfo);
                    managerInst.InsertOrUpdate(userLoginInfo);//插入用户信息（就算考核程序重新载入，也会自动加载该用户
                    DebugLog("Client receive from server and save DB_UserLoginInfo " + userLoginInfo.UnionID + " complete.");
                }
                break;
        }
    }
#endif
}
