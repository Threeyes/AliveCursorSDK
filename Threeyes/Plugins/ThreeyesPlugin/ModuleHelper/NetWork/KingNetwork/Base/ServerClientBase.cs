using UnityEngine;
#if USE_KingNetwork
using KingNetwork.Shared;
#endif
/// <summary>
/// Server/Client 的通用基类
/// </summary>
/// <typeparam name="TInst"></typeparam>
public abstract class ServerClientBase<TInst> : InstanceBase<TInst>
        where TInst : ServerClientBase<TInst>
{
    public int port = 7172;
    public StringEvent onReceiveMessage;//ToUse
    public bool isAutoConnect = true;

#if USE_KingNetwork
    public NetworkListenerType networkListenerType = NetworkListenerType.TCP;
#endif

    [Header("Debug")]
    public bool isLog = true;
    public StringEvent onLog;

    #region Public Method
    public abstract void Connect(bool isConnect);
    #endregion


    #region Utility
    protected void DebugLog(string info)
    {
        if (!isLog)
            return;
        Debug.Log(info);
        onLog.Invoke(info);
    }
    #endregion
}
