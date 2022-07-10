using UnityEngine;
#if MIRROR
using Mirror;
#endif

public class NetworkPlayer :
#if MIRROR
    NetworkBehaviour
#else
    Component
#endif
{
}
