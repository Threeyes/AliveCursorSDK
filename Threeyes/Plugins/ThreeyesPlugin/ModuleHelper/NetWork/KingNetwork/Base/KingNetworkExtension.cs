using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if USE_KingNetwork
using KingNetwork.Client;
using KingNetwork.Server;
using KingNetwork.Server.Interfaces;
using KingNetwork.Shared;
#endif

#if USE_KingNetwork
public static class KingNetworkExtension
{
 public static bool IsValid(this KingServer server, bool logIFNotValid = false)
    {
        if (server == null)
        {
            if (logIFNotValid)
                Debug.LogError("The Server is null!");
            return false;
        }
        return true;
    }
    public static bool IsValid(this KingClient client, bool logIFNotValid = false)
    {
        if (client == null)
        {
            if (logIFNotValid)
                Debug.LogError("The Client is null!");
            return false;
        }
        else if (!client.HasConnected)
        {
            if (logIFNotValid)
                Debug.LogError("The Client 未连接！!");
            return false;
        }
        return true;
    }

    //以下方法传递带枚举信息:

    //——Server——
    public static void SendMessageToAllEx<TPacketEnum>(this KingServer server, TPacketEnum customPacketType, string content)
          where TPacketEnum : IConvertible
    {
        if (server.IsValid(true))
            SendMessageFuncEx(customPacketType, content, (writer) => server.SendMessageToAll(writer));
    }

    /// <summary>
    /// 向客户端发送数据
    /// </summary>
    public static void SendMessageEx<TPacketEnum>(this IClient clientToSend, TPacketEnum customPacketType, string content)
    where TPacketEnum : IConvertible
    {
        SendMessageFuncEx(customPacketType, content, (writer) => clientToSend.SendMessage(writer));
    }

    //——Client——

    public static void SendMessageEx<TPacketEnum>(this KingClient client, TPacketEnum customPacketType, string content)
where TPacketEnum : IConvertible
    {
        if (client.IsValid(true))
            SendMessageFuncEx(customPacketType, content, (writer) => client.SendMessage(writer));
    }

    #region Utility

    /// <summary>
    /// 通用的发送消息
    /// </summary>
    /// <typeparam name="TPacketEnum">继承于byte的枚举类型</typeparam>
    /// <param name="customPacketType"></param>
    /// <param name="content"></param>
    /// <param name="actWrite"></param>
    static void SendMessageFuncEx<TPacketEnum>(TPacketEnum customPacketType, string content, UnityAction<KingBufferWriter> actWrite)
           where TPacketEnum : IConvertible
    {
        //new Thread(() =>
        //{
        //Thread.Sleep(1000);

        using (var writer = KingBufferWriter.Create())
        {
            try
            {
                writer.Write(customPacketType);//#1（先在头部写入信息类型，参考 https://github.com/Mun1z/KingNetwork/blob/master/examples/Console/SimpleExample/KingNetwork.SimpleExample.Server/Program.cs ）
                writer.Write(content);//#2写入内容
                actWrite.Execute(writer);//#3发送
            }
            catch (Exception e)
            {
                Debug.LogError("SendMessageFunc with error:\r\n" + e);
            }
        }
        //}).Start();

    }

    #endregion


    //ToAdd:不带枚举的信息
}

#endif
