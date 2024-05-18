namespace Threeyes.Network
{
    using UnityEngine;
    using Threeyes.EventPlayer;
#if MIRROR//自动检测宏定义并激活，不需要再手动激活
    using Mirror;
#endif

    /// <summary>
    /// Todo:弄成通用的Network，放在EventPlayer中(后期通过Netcode宏定义来实现通用支持)
    /// 
    /// </summary>
#if MIRROR
    [RequireComponent(typeof(NetworkIdentity))]
#endif
    [DisallowMultipleComponent]
    public class NetworkEventPlayer : NetworkComponentBase
    {
        public EventPlayer Comp { get { if (!comp) comp = GetComponent<EventPlayer>(); return comp; } }
        [SerializeField] protected EventPlayer comp;

#if MIRROR

        /// Server端控制 ，类似于 clientAuthority=false

        [SyncVar]
        public bool IsActive;

        protected override void ServerUpdate()
        {
            SyncValue(Comp.IsActive, ref IsActive);
        }

        public override void OnStartClient()
        {
            Comp.IsCommandMode = true;//默认为Command模式
            Comp.actionCommandSetIsActive = CMDSetIsActive;
            Comp.actionCommandPlay = CMDPlay;

            //ToAdd：针对带参调用，通过反射绑定(或直接继承该方法）（或者在Ep子类中增加将object转为对应实例的方法，然后通过object传送）

            //以下主要用于让新登录的Client同步字段值
            //PS:Host(Server+Client)不需要初始化，因为是Server负责管理值
            //ToUpdate:通过Hook更新
            if (!isServer)
            {
                Comp.IsActive = IsActive;
            }
        }
        public override void OnStopClient()
        {
            Comp.actionCommandSetIsActive = null;
            Comp.actionCommandPlay = null;
        }

        [Command(requiresAuthority = false)]
        void CMDSetIsActive(bool isActive)//（Client端发起调用）在Server端运行
        {
            RPCSetIsActive(isActive);
        }
        [Command(requiresAuthority = false)]
        void CMDPlay(bool isPlay)
        {
            RPCPlay(isPlay);
        }


        [ClientRpc]
        void RPCSetIsActive(bool isActive)//（Server端发起调用）在Client端运行
        {
            //实际执行时：调用实际方法
            Comp.IsCommandMode = false;
            Comp.IsActive = isActive;
            Comp.IsCommandMode = true;
        }
        [ClientRpc]
        void RPCPlay(bool isPlay)
        {
            Comp.IsCommandMode = false;
            Comp.Play(isPlay);
            Comp.IsCommandMode = true;
        }

#endif
    }
}