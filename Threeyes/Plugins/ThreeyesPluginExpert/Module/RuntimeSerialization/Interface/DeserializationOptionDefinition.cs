#if USE_JsonDotNet
using Newtonsoft.Json;
using System;
using UnityEngine;
#endif
namespace Threeyes.RuntimeSerialization
{
    public interface IDeserializationOption
    {
    }

#if USE_JsonDotNet
    [JsonObject(MemberSerialization.Fields)]//PS: Ignore Property
#endif
    public class DeserializationOption : IDeserializationOption
    {
        public DeserializationOption() { }
    }

    [Serializable]
    public class DeserializationOption_GameObject : DeserializationOption
    {
        /// <summary>
        /// 如果反序列化时找不到某个带RS_GO组件的子物体实例对应的PropertyBag信息，则删掉
        /// </summary>
        public bool DeleteNotExistInstance { get { return deleteNotExistInstance; } set { deleteNotExistInstance = value; } }

        /// <summary>
        /// 是否删除预制物中不存在的带RS_GO组件的物体实例
        /// </summary>
        public bool DeletePrefabNotExistInstance { get { return deletePrefabNotExistInstance; } set { deletePrefabNotExistInstance = value; } }


        [SerializeField] protected bool deleteNotExistInstance = true;//删除不存在的RS_GO实例
        [SerializeField] protected bool deletePrefabNotExistInstance = false;//（如果deleteNotExistInstance为true）删除反序列化时生成的预制物中不存在的RS_GO实例。建议为false，因为如果预制物修改了层级或RS_GO的ID，也不会导致意外删除其子物体


        //——Runtime（存放反序列化时需要传给子类的信息）——
        internal bool isParentRuntimeDeserializedPrefab;//该物体或父物体是否为运行时通过反序列化生成的预制物实例（主要用于deletePrefabNotExistInstance字段）
        public DeserializationOption_GameObject()
        {
            deleteNotExistInstance = true;
            deletePrefabNotExistInstance = false;
        }
    }
}