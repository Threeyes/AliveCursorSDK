using UnityEngine;
using System;
using UnityEngine.Events;

#if USE_KingNetwork
using KingNetwork.Server.Interfaces;
using KingNetwork.Shared.Interfaces;
using KingNetwork.Server;
#endif
/// <summary>
/// 服务端
/// </summary>
/// <typeparam name="TInst"></typeparam>
public abstract class ServerManagerBase<TInst> : ServerClientBase<TInst>
    where TInst : ServerManagerBase<TInst>
{
    public BoolEvent onServerStarted;//Server启动，用于更新状态

    [Header("Runtime")]
    public bool isStarted = false;

    void Start()
    {
        if (isAutoConnect)
        {
            Connect(true);
        }
    }

    protected virtual void OnApplicationQuit()
    {
        Connect(false);
    }

    public override void Connect(bool isConnect)
    {
#if USE_KingNetwork
        if (isConnect)
        {
            //PS:Server 默认需要运行在新线程（因为作者是在VS上运行的）
            var SocketThread = new System.Threading.Thread(RunServer);
            SocketThread.IsBackground = true;
            SocketThread.Start();
        }
        else
        {
            if (server != null)
            {
                // stop the server when you don't need it anymore
                server.Stop();
                isStarted = false;
                DebugLog("OnServerStoped");
                onServerStarted.Invoke(false);
            }
        }
#endif
    }

#if USE_KingNetwork
    public UnityAction<IClient> onClientConnected;
    protected KingServer server;
#endif

#if USE_KingNetwork
    #region Inner Method
    void RunServer()
    {
        // create and start the server
        server = new KingServer(port: (ushort)port);
        server.OnServerStartedHandler += OnServerStartedRaw;
        server.OnClientConnectedHandler += OnClientConnectedRaw;
        server.OnClientDisconnectedHandler += OnClientDisconnectedRaw;
        server.OnMessageReceivedHandler += OnMessageReceivedRaw;
        server.Start(networkListenerType);
    }

    //——Callback——

    void OnServerStartedRaw()
    {
        Action action = () => OnServerStarted();
        action.ExecuteInMainThread();
    }
    void OnClientConnectedRaw(IClient client)
    {
        Action action = () => OnClientConnected(client);
        action.ExecuteInMainThread();
    }
    void OnClientDisconnectedRaw(IClient client)
    {
        Action action = () => OnClientDisconnected(client);
        action.ExecuteInMainThread();
    }
    void OnMessageReceivedRaw(IClient client, IKingBufferReader reader)
    {
        Action action = () => OnMessageReceived(client, reader);
        action.ExecuteInMainThread();
    }

    protected virtual void OnServerStarted()
    {
        onServerStarted.Invoke(true);
        isStarted = true;
        DebugLog("OnServerStarted");
    }
    protected virtual void OnClientConnected(IClient client)
    {
        //PS:每个应用对应的ID唯一，可以将其缓存并作为当前运行应用的标识
        DebugLog("OnClientConnected " + client.IpAddress + " " + client.Id);
    }
    protected virtual void OnClientDisconnected(IClient client)
    {
        DebugLog("OnClientDisconnected " + client.IpAddress + " " + client.Id);
    }

    // implements the callback for MessageReceivedHandler
    protected virtual void OnMessageReceived(IClient client, IKingBufferReader reader)
    {
        //调试用，可清除掉
        string receivedInfo = reader.ReadString();
        onReceiveMessage.Execute(receivedInfo);
        DebugLog("Received {" + receivedInfo + "} from client " + client.Id + " length " + reader.Length);
    }

    #endregion
#endif
}

public class ServerManagerBase<TInst, TPacketEnum> : ServerManagerBase<TInst>
    where TInst : ServerManagerBase<TInst>
    where TPacketEnum : IConvertible
{
#if USE_KingNetwork

    public virtual void SendMessageToAll(TPacketEnum en, string content)
    {
        //需要自行实现
        server.SendMessageToAllEx(en, content);
    }

    protected override void OnMessageReceived(IClient client, IKingBufferReader reader)
    {
        TPacketEnum en = reader.ReadMessagePacket<TPacketEnum>();
        OnMessageReceivedFunc(client, en, reader);
    }

    protected virtual void OnMessageReceivedFunc(IClient client, TPacketEnum en, IKingBufferReader reader)
    {
        //调试用，可清除掉
        string receivedInfo = reader.ReadString();
        onReceiveMessage.Execute(receivedInfo);
        DebugLog("Received Packet [" + en + "] {" + receivedInfo + "}  from client {" + client.Id + "}");

    }
#endif
}
