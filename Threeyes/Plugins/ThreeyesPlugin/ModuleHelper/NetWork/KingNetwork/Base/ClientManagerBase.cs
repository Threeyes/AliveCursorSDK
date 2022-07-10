using System;
#if USE_KingNetwork
using KingNetwork.Client;
using KingNetwork.Shared.Interfaces;
#endif

public abstract class ClientManagerBase<TInst> : ServerClientBase<TInst>
        where TInst : ClientManagerBase<TInst>
{
    public string ip = "127.0.0.1";

    void Start()
    {
        if (isAutoConnect)
            Connect(true);
    }
    void OnApplicationQuit()
    {
        Connect(false);
    }

    public override void Connect(bool isConnect)
    {
#if USE_KingNetwork
        if (isConnect)
        {
            if (client != null && client.HasConnected)
                return;

            //PS:客户端不需要开启线程，因为本身就已经开启了
            //Bug:因为不是我方开启的线程，所以这个调用无效，针对Unity的类型（UnityEvent）等无法调用，委托正常调用
            //解决办法：后期直接引入源码，在其基础上调用ExecuteInMainThread
            client = new KingClient();
            client.MessageReceivedHandler = OnMessageReceivedRaw;
            client.Connect(ip, (ushort)port, networkListenerType);
        }
        else
        {
            if (client != null)
            {
                // disconnect from the server when we are done
                client.Disconnect();
            }
        }
#endif
    }


#if USE_KingNetwork
    protected KingClient client;

    void OnMessageReceivedRaw(IKingBufferReader reader)
    {
        Action action = () => OnMessageReceived(reader);
        action.ExecuteInMainThread();

    }
    protected virtual void OnMessageReceived(IKingBufferReader reader)
    {
        string receivedInfo = reader.ReadString();
        onReceiveMessage.Execute(receivedInfo);
        DebugLog("Received {" + receivedInfo + "}  from server, length " + reader.Length);
    }

#endif
}

public abstract class ClientManagerBase<TInst, TPacketEnum> : ClientManagerBase<TInst>
        where TInst : ClientManagerBase<TInst>
    where TPacketEnum : IConvertible
{

    public virtual void SendMessageToServer(TPacketEnum en, string content)
    {
#if USE_KingNetwork
        client.SendMessageEx(en, content);
#endif
    }

#if USE_KingNetwork
    protected override void OnMessageReceived(IKingBufferReader reader)
    {
        ///Warning:针对带有枚举的Packet信息，读取流程不能变，否则无法读取：
        ///1.读取Packet
        ///2.读取内容
        TPacketEnum en = reader.ReadMessagePacket<TPacketEnum>();
        OnMessageReceivedFunc(en, reader);
    }

    protected virtual void OnMessageReceivedFunc(TPacketEnum en, IKingBufferReader reader)
    {
        //调试用，可清除掉
        string receivedInfo = reader.ReadString();
        onReceiveMessage.Execute(receivedInfo);
        DebugLog("Received Packet [" + en + "] {" + receivedInfo + "}  from server");
    }
#endif
}
