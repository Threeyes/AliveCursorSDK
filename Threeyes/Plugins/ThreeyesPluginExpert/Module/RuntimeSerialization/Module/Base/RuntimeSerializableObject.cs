using UnityEngine;
using Threeyes.Data;

namespace Threeyes.RuntimeSerialization
{
    /// <summary>
    /// 可序列化的UnityObject类
    /// </summary>
    public interface IRuntimeSerializableObject : IRuntimeSerializable, IIdentityHolder { }

    /// <summary>
    /// 辅助指定Object（GameObject或组件）进行 序列化/反序列化
    /// 
    /// 原理：
    /// -参考 Mirror.NetworkBehaviour，只有挂载该组件的物体才会进行序列化
    /// Ref:GILES.pb_Scene...
    /// </summary>
    public abstract partial class RuntimeSerializableObject : MonoBehaviour, IRuntimeSerializableObject
    {
        public abstract Identity ID { get; set; }//UID，用于反序列化时匹配。参考RTSGO，避免同级有多个相同组件，导致序列化失败。
        public abstract System.Type ContainerType { get; }
        //public abstract Object BaseContainer { get; }
        //public abstract PropertyBag BasePropertyBag { get; }
    }
}