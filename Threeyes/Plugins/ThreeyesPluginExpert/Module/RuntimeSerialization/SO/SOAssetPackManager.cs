using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.RuntimeSerialization
{
    /// <summary>
    /// 管理当前激活的SOAsset
    /// </summary>
    public static class SOAssetPackManager
    {
        public static List<SOAssetPack> listActiveAssetPack = new List<SOAssetPack>();//用于序列化/反序列化时临时设置的数据（ToUpdate：在加载Mod后就开始初始化）


        #region Deserialize
        /// <summary>
        /// 查找首个匹配GUID的Prefab
        /// </summary>
        /// <param name="prefabGuid"></param>
        /// <returns></returns>
        public static GameObject TryGetPrefab(string prefabGuid)
        {
            foreach (var soAP in listActiveAssetPack)
            {
                if (!soAP)
                    continue;
                GameObject prefab = soAP.TryGetPrefab(prefabGuid);
                if (prefab)
                    return prefab;
            }
            return null;
        }
        public static GameObject TryInstantiatePrefab(string prefabGuid, Transform parent)
        {
            foreach (var soAP in listActiveAssetPack)
            {
                if (!soAP)
                    continue;
                GameObject inst = soAP.TryInstantiatePrefab(prefabGuid, parent);
                if (inst)
                    return inst;
            }
            return null;
        }
        #endregion

        #region Serialize
        public static string GetPrefabMetadata(GameObject prefab)
        {
            foreach (var soAP in listActiveAssetPack)
            {
                if (!soAP)
                    continue;

                string guid = soAP.GetPrefabMetadata_Runtime(prefab);
                if (guid.NotNullOrEmpty())
                    return guid;
            }
            return "";
        }
        #endregion
    }
}