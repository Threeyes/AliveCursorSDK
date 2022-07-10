using UnityEngine;
using Threeyes.Network;
#if MIRROR
using Mirror;
#endif

#if MIRROR
[RequireComponent(typeof(RemoteDestinationPointManager))]
#endif
[DisallowMultipleComponent]
public class NetworkRemoteDestinationPointManager : NetworkComponentHelperBase
{
    public RemoteDestinationPointManager Comp
    {
        get
        {
            if (!comp)
                comp = GetComponent<RemoteDestinationPointManager>();
            return comp;
        }
    }
    [SerializeField] protected RemoteDestinationPointManager comp;


#if MIRROR

    public override void OnStartClient()
    {
        Comp.IsCommandMode = true;
        Comp.actCommandSetPos = CMDSetPos;
    }

    public override void OnStopClient()
    {
        Comp.actCommandSetPos = null;
    }

    [Command(requiresAuthority = false)]
    void CMDSetPos(RemoteDestinationPoint rdp, Vector3? positionOverride, bool isInvokeEvent)
    {
        RPCSetPos(rdp, positionOverride, isInvokeEvent);
    }
    [ClientRpc]
    void RPCSetPos(RemoteDestinationPoint rdp, Vector3? positionOverride, bool isInvokeEvent)
    {
        if (rdp)
        {
            Comp.IsCommandMode = false;
            rdp.SetPos(positionOverride);
            Comp.IsCommandMode = true;
        }
    }

#endif
}
#if MIRROR
//需要通过扩展方法来序列化/反序列化Mirror不支持的类实例（https://mirror-networking.gitbook.io/docs/guides/data-types#custom-data-types）
public static class NetworkRemoteDestinationPointSerializer
{
    //通过序列化唯一的InstanceID来找到对应物体(bug:每个打包程序的instanceID都不一样）
    //Update:通过物体在层级的位置
    public static void Write(this NetworkWriter writer, RemoteDestinationPoint inst)
    {
        // no need to serialize the data, just the name of the armor
        string str = SceneUniqueTool.GetSceneID(inst);
        writer.WriteString(str);
    }

    public static RemoteDestinationPoint Read(this NetworkReader reader)
    {
        string str = reader.ReadString();
        // load the same armor by name. 
        return SceneUniqueTool.FindBySceneID<RemoteDestinationPoint>(str, RemoteDestinationPointManager.Instance.tfRoot);
    }
}
#endif