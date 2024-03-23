using System.Collections;
using System.Collections.Generic;
using Threeyes.Core;
using UnityEngine;
using System.Linq;
namespace Threeyes.RuntimeSerialization
{
    /// <summary>
    /// 管理当前激活的SOAsset
    /// </summary>
    public static class SOAssetPackManager
    {
        static List<SOAssetPackInfo> listActiveSOAssetPackInfo = new List<SOAssetPackInfo>();

        public static void Add(SOAssetPackInfo sOAssetPackInfo)
        {
            listActiveSOAssetPackInfo.AddOnce(sOAssetPackInfo);
            listActiveSOAssetPackInfo.RemoveAll(data => !data.IsValid);//清空无效数据
        }
        public static void Remove(SOAssetPackInfo sOAssetPackInfo)
        {
            listActiveSOAssetPackInfo.Remove(sOAssetPackInfo);
            listActiveSOAssetPackInfo.RemoveAll(data => !data.IsValid);//清空无效数据
        }

        #region Serialize
        /// <summary>
        /// 根据传入的预制物，检查匹配的Metadata
        /// 
        /// ToUpdate：
        /// -【非必须】可选传入scope，如果有多个匹配项则优先匹配相同scope的（如Default场景Mod中，则优先查找该Mod中的prefab）
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="guid"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        public static bool GetPrefabMetadata(GameObject prefab, out string guid, out string scope)
        {
            guid = scope = "";
            foreach (var aPInfo in listActiveSOAssetPackInfo)
            {
                if (!aPInfo.IsValid)
                    continue;

                string tempGuid = aPInfo.sOAssetPack.GetPrefabMetadata_Runtime(prefab);
                if (tempGuid.NotNullOrEmpty())
                {
                    guid = tempGuid;
                    scope = aPInfo.scope;
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Deserialize
        public static GameObject TryGetPrefab(RuntimeSerializable_GameObject runtimeSerializable_GameObject)
        {
            string latestScope;
            return TryGetPrefab(runtimeSerializable_GameObject.CachePrefabID.Guid, runtimeSerializable_GameObject.CacheScope, out latestScope);
        }

        /// <summary>
        /// 查找首个匹配的Prefab
        /// </summary>
        /// <param name="prefabGuid"></param>
        /// <param name="scope">之前保存的scope（可能会过期，所以不是决定因素）</param>
        /// <param name="latestScope">最新的Scope信息</param>
        /// <returns></returns>
        public static GameObject TryGetPrefab(string prefabGuid, string scope, out string latestScope)
        {
            latestScope = "";
            List<MatchedPrefabInfo> listMatchedPrefabInfo = new List<MatchedPrefabInfo>();
            foreach (var aPInfo in listActiveSOAssetPackInfo)
            {
                if (!aPInfo.IsValid)
                    continue;
                GameObject prefab = aPInfo.sOAssetPack.TryGetPrefab(prefabGuid);
                if (prefab)
                    listMatchedPrefabInfo.Add(new MatchedPrefabInfo(aPInfo, prefab));
            }
            MatchedPrefabInfo targetMPI = null;
            if (listMatchedPrefabInfo.Count == 0)
            {
            }
            else if (listMatchedPrefabInfo.Count == 1)//仅有1个匹配项：返回该元素
            {
                targetMPI = listMatchedPrefabInfo[0];
            }
            else  //多于一个匹配项：因为不同项目Prefab的ID重复，需要通过scope进行二次筛选
            {
                targetMPI = listMatchedPrefabInfo.FirstOrDefault(mPI => mPI.sOAssetPackInfo.scope == scope);
                if (targetMPI == null)//没有匹配项：可能是旧场景保存的序列化内容没有初始化该字段，不作为报错，而是先默认返回第一个元素
                {
                    targetMPI = listMatchedPrefabInfo[0];
                    Debug.LogWarning($"找不到匹配 scope:{scope} 的prefab (guid:{prefabGuid})! 使用首个相同guid的预制物代替！");//不算错误，但有可能会出现引用错误
                }
            }

            if (targetMPI != null)
            {
                latestScope = targetMPI.sOAssetPackInfo.scope;//返回最新的Scope，方便更新旧的序列化文件，或者更改了域的文件
                return targetMPI.prefab;
            }
            return null;
        }
        #endregion

        #region Define
        class MatchedPrefabInfo
        {
            public SOAssetPackInfo sOAssetPackInfo;
            public GameObject prefab;

            public MatchedPrefabInfo() { }
            public MatchedPrefabInfo(SOAssetPackInfo sOAssetPackInfo, GameObject prefab)
            {
                this.sOAssetPackInfo = sOAssetPackInfo;
                this.prefab = prefab;
            }
        }
        #endregion
    }

    #region Define
    ///ToUpdate:
    ///-TryGetPrefab：参数增加可空Scope，或者可以直接
    ///-RS_GO.InitPrefabMetadata：可以直接传入SOAssetPackInfo
    ///等方法， 需要返回数据类
    public class SOAssetPackInfo
    {
        public bool IsValid { get { return sOAssetPack != null; } }

        public string scope = "";//【可选】其所在范围的唯一标识，可避免不同项目Prefab的ID重复时进行二次筛选（如Mod文件的唯一名称、SDK、Builtin等特殊且不变的字段）（保存到根物体的GameObjectPropertyBag中，与ID同级）
        public SOAssetPack sOAssetPack;

        public SOAssetPackInfo()
        {
        }

        public SOAssetPackInfo(string scope, SOAssetPack sOAssetPack)
        {
            this.scope = scope;
            this.sOAssetPack = sOAssetPack;
        }
    }
    #endregion
}