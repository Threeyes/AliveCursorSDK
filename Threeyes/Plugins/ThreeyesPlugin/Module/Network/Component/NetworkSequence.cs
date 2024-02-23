using System.Collections;
using System.Collections.Generic;
using Threeyes.Core;
using UnityEngine;
#if MIRROR
using Mirror;
#endif

namespace Threeyes.Network
{
#if MIRROR
    [RequireComponent(typeof(NetworkIdentity))]
#endif
    [DisallowMultipleComponent]
    public class NetworkSequence : NetworkComponentBase
    {
        public SequenceAbstract Comp
        {
            get
            {
                if (!comp)
                    comp = GetComponent<SequenceAbstract>();
                return comp;
            }
        }
        [SerializeField] protected SequenceAbstract comp;

#if MIRROR

        //——同步属性。针对可能在其他同步方法中调用到的字段（如CurIndex)（非必须）——
        //（注意不能用方法的形式，因为调用方法时会临时把IsCommandMode设置为false导致对应回调无法被正确调用）
        [SyncVar]
        public int CurIndex;

        protected override void ServerUpdate()
        {
            //同步属性/字段
            SyncValue(Comp.CurIndex, ref CurIndex);
        }


        //——同步方法——

        public override void OnStartClient()
        {
            Comp.IsCommandMode = true;
            Comp.actCommandSetDelta = CMDSetDelta;
            Comp.actCommandSet = CMDSet;
            Comp.actCommandReset = CMDReset;

            //在用户登录时同步属性/字段
            if (!isServer)
            {
                Comp.CurIndex = CurIndex;
            }
        }

        public override void OnStopClient()
        {
            Comp.actCommandSetDelta = null;
            Comp.actCommandSet = null;
            Comp.actCommandReset = null;
        }

        [Command(requiresAuthority = false)]
        void CMDSetDelta(int delta)
        {
            RPCSetDelta(delta);
        }
        [Command(requiresAuthority = false)]
        void CMDSet(int index)
        {
            RPCSet(index);
        }
        [Command(requiresAuthority = false)]
        void CMDReset(int index)
        {
            RPCReset(index);
        }


        [ClientRpc]
        void RPCSetDelta(int delta)
        {
            Comp.IsCommandMode = false;
            Comp.SetDelta(delta);
            Comp.IsCommandMode = true;
        }
        [ClientRpc]
        void RPCSet(int index)
        {
            Comp.IsCommandMode = false;
            Comp.Set(index);
            Comp.IsCommandMode = true;
        }
        [ClientRpc]
        void RPCReset(int index)
        {
            Comp.IsCommandMode = false;
            Comp.Reset(index);
            Comp.IsCommandMode = true;
        }
#endif
    }
}