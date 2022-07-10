using UnityEngine;
#if MIRROR
using Mirror;
#endif

namespace Threeyes.Network
{
    /// （Warning：继承NetworkBehaviour的不能是泛型，否则Mirror会报错。所以不能提炼出类似于NetworkComponentHelper的泛型基类）
    public abstract class NetworkComponentHelperBase :
#if MIRROR
        NetworkBehaviour
#else
        MonoBehaviour
#endif
    {

#if MIRROR

        //PS:Host兼做Server和Client

        protected virtual void Update()
        {
            if (isServer)
            {
                ServerUpdate();
            }
        }

        protected virtual void ServerUpdate() { }

        /// <summary>
        /// Sync any value
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static void SyncValue<TValue>(TValue source, ref TValue target)
        {
            if (!target.Equals(source))
                target = source;
        }

#endif
    }
}