//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using System;
//#if MIRROR
//using Mirror;
//#endif
///// <summary>
///// Bug:因为该组件的调用方法有些参数无法通过网络传递，可能会导致丢失参数，而且NetworkIdentify会导致物体隐藏，所以暂不使用
///// </summary>
//#if MIRROR
//[RequireComponent(typeof(RemoteDestinationPoint))]
//#endif
//[DisallowMultipleComponent]
//public class NetworkRemoteDestinationPoint : NetworkComponentHelperBase
//{
//    public RemoteDestinationPoint Comp
//    {
//        get
//        {
//            if (!comp)
//                comp = GetComponent<RemoteDestinationPoint>();
//            return comp;
//        }
//    }
//    [SerializeField] protected RemoteDestinationPoint comp;


//#if MIRROR

//    public override void OnStartClient()
//    {
//        Comp.IsCommandMode = true;
//        Comp.actCommandSetPos += CMDSetPos;
//    }

//    public override void OnStopClient()
//    {
//        Comp.actCommandSetPos = null;
//    }

//    [Command(requiresAuthority = false)]
//    void CMDSetPos()
//    {
//        RPCSetPos();
//    }

//    [ClientRpc]
//    void RPCSetPos()
//    {
//        Comp.IsCommandMode = false;
//        Comp.SetPos();
//        Comp.IsCommandMode = true;

//    }
//#endif
//}
