using UnityEngine;
using UnityEngine.Events;
#if MIRROR
using Mirror;
#endif
namespace Threeyes.Network
{
    public class NetworkBehaviourEventListener :
#if MIRROR
    NetworkBehaviour
#else
    Component
#endif
    {
        public UnityEvent onStartServer;
#if MIRROR
        public override void OnStartServer()
        {
            base.OnStartServer();
            onStartServer.Invoke();
        }
#endif
    }
}