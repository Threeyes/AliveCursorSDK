using System;
using System.Collections.Generic;
using UnityEngine;
using UnityObject = UnityEngine.Object;
using Threeyes.Core;
using Threeyes.Core.Editor;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Threeyes.RuntimeSerialization
{
    /// <summary>
    /// 存储Asset的meta信息，方便运行时重新连接Asset及Prefab。类似于运行时获取Resource资源的功能
    /// 
    /// Ref:Unity.RuntimeSceneSerialization.AssetPack
    /// 
    /// Stores asset metadata (guid and fileId) for assets associated with a JSON-serialized scene
    /// This type is used as a look up table for asset objects, and can be used to build an AssetBundle for loading
    /// scene assets in player builds
    /// 
    /// ToUpdate:
    /// -添加所有Assets的meta信息，方便运行时替换资源
    /// </summary>
    [CreateAssetMenu(menuName = "SO/RuntimeSerialization/" + "AssetPack", fileName = "AssetPack")]
    public class SOAssetPack : ScriptableObject, ISerializationCallbackReceiver
    {
        const int k_InvalidId = -1;

        [HideInInspector]
        [SerializeField]
        string[] m_Guids;

        [HideInInspector]
        [SerializeField]
        Asset[] m_Assets;

        [HideInInspector]
        [SerializeField]
        List<string> m_PrefabGuids = new();

        [HideInInspector]
        [SerializeField]
        List<GameObject> m_Prefabs = new();

        readonly Dictionary<string, Asset> m_AssetDictionary = new();
        readonly Dictionary<UnityObject, KeyValuePair<string, long>> m_AssetLookupMap = new();
        readonly Dictionary<string, GameObject> m_PrefabDictionary = new();

        // Local method use only -- created here to reduce garbage collection. Collections must be cleared before use
        readonly List<string> k_Guids = new();
        readonly List<Asset> k_Assets = new();

        public Dictionary<string, Asset> Assets => m_AssetDictionary;
        public Dictionary<string, GameObject> Prefabs => m_PrefabDictionary;

        /// <summary>
        /// The number of assets and prefabs in in this SOAssetPack
        /// </summary>
        public int AssetCount => Assets.Count + Prefabs.Count;

        /// <summary>
        /// 外部注册并生成Prefab的方法
        /// </summary>
        readonly HashSet<IPrefabFactory> m_PrefabFactories = new();

        /// <summary>
        /// Register an IPrefabFactory to instantiate prefabs which were not saved along with the scene
        /// </summary>
        /// <param name="factory">An IPrefabFactory which can instantiate prefabs by guid</param>
        public void RegisterPrefabFactory(IPrefabFactory factory) { m_PrefabFactories.Add(factory); }

        /// <summary>
        /// Unregister an IPrefabFactory
        /// </summary>
        /// <param name="factory">The IPrefabFactory to be unregistered</param>
        public void UnregisterPrefabFactory(IPrefabFactory factory) { m_PrefabFactories.Remove(factory); }

        /// <summary>
        /// Clear all asset references in this SOAssetPack
        /// </summary>
        public void Clear()
        {
            m_AssetDictionary.Clear();
            m_AssetLookupMap.Clear();
            m_PrefabDictionary.Clear();
        }

        /// <summary>
        /// Get the guid and sub-asset index for a given asset
        /// Also adds the asset to the asset pack in the editor
        /// </summary>
        /// <param name="obj">The asset object</param>
        /// <param name="guid">The guid for this asset in the AssetDatabase</param>
        /// <param name="fileId">The fileId within the asset for the object</param>
        /// <param name="warnIfMissing">Whether to print warnings if the object could not be found (suppress if this
        /// might be a scene object and metadata doesn't exist)</param>
        public void GetAssetMetadata(UnityObject obj, out string guid, out long fileId, bool warnIfMissing)
        {
            if (m_AssetLookupMap.TryGetValue(obj, out var assetData))
            {
                guid = assetData.Key;
                fileId = assetData.Value;
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                GetOrAddAssetMetadata(obj, out guid, out fileId, warnIfMissing);
                return;
            }
#endif

            guid = string.Empty;
            fileId = k_InvalidId;

            // ReSharper disable once CommentTypo
            // Suppress warning for DontSave objects
            if ((obj.hideFlags & HideFlags.DontSave) == HideFlags.None && warnIfMissing)
                Debug.LogWarning($"Could not find asset metadata for {obj}");
        }

        /// <summary>
        /// Get an asset based on its guid and fileId
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public UnityObject GetAsset(string guid, long fileId)
        {
            if (!m_AssetDictionary.TryGetValue(guid, out var asset))
            {
#if UNITY_EDITOR
                Debug.LogWarningFormat("Asset pack {0} does not contain an asset for guid {1}. Falling back to AssetDatabase.", name, guid);
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path))
                {
                    Debug.LogWarningFormat("Could not find asset path for {0}", guid);
                    return null;
                }

                var assets = AssetDatabase.LoadAllAssetsAtPath(path);
                if (assets == null)
                {
                    Debug.LogWarningFormat("Could not load asset with guid {0}", guid);
                    return null;
                }

                foreach (var subAsset in assets)
                {
                    if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(subAsset, out _, out long assetFileId))
                    {
                        if (assetFileId == fileId)
                            return subAsset;
                    }
                }

                Debug.LogWarningFormat("Invalid index (too large): {0} for asset at path {1}", fileId, path);
                return null;
#else
                Debug.LogWarningFormat("Could not load asset with guid {0}", guid);
                return null;
#endif
            }

            return asset.GetAsset(fileId);
        }

        /// <summary>
        /// Called before serialization to set up lists from dictionaries
        /// </summary>
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            k_Guids.Clear();
            k_Assets.Clear();
            foreach (var kvp in m_AssetDictionary)
            {
                var guid = kvp.Key;
                if (string.IsNullOrEmpty(guid))
                    continue;

                k_Guids.Add(guid);
                k_Assets.Add(kvp.Value);
            }

            m_Guids = k_Guids.ToArray();
            m_Assets = k_Assets.ToArray();

            m_PrefabGuids.Clear();
            m_Prefabs.Clear();
            foreach (var kvp in m_PrefabDictionary)
            {
                var guid = kvp.Key;
                if (string.IsNullOrEmpty(guid))
                    continue;

                m_PrefabGuids.Add(guid);
                m_Prefabs.Add(kvp.Value);
            }
        }

        /// <summary>
        /// Called after serialization to set up dictionaries from lists
        /// </summary>
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            m_AssetDictionary.Clear();
            var count = m_Guids.Length;
            for (var i = 0; i < count; i++)
            {
                var asset = m_Assets[i];
                var guid = m_Guids[i];

                if (string.IsNullOrEmpty(guid))
                    continue;

                m_AssetDictionary[guid] = asset;

                foreach (var kvp in asset.Assets)
                {
                    var assetRef = kvp.Value;
                    if (assetRef)
                        m_AssetLookupMap[assetRef] = new KeyValuePair<string, long>(guid, kvp.Key);
                }
            }

            count = m_PrefabGuids.Count;
            for (var i = 0; i < count; i++)
            {
                var prefab = m_Prefabs[i];
                var guid = m_PrefabGuids[i];
                if (string.IsNullOrEmpty(guid))
                    continue;

                m_PrefabDictionary[guid] = prefab;
            }
        }

        /// <summary>
        /// 用于重新生成Prefab
        /// Instantiate the prefab with the given guid, if it is in the asset pack or can be created by a registered factory
        /// </summary>
        /// <param name="prefabGuid">The guid of the prefab to be instantiated</param>
        /// <param name="parent">The parent object to be used when calling Instantiate</param>
        /// <returns>The instantiated prefab, or null if one was not instantiated</returns>
        public GameObject TryInstantiatePrefab(string prefabGuid, Transform parent)
        {
            if (m_PrefabDictionary.TryGetValue(prefabGuid, out var prefab))
            {
                if (prefab != null)
                    return Instantiate(prefab, parent);
                else
                {
                    Debug.LogError($"Prefab with guid [{prefabGuid}] is null!");
                    return null;
                }
            }

            foreach (var factory in m_PrefabFactories)//第三方注册的创建Prefab的工厂（暂未用上）
            {
                try
                {
                    prefab = factory.TryInstantiatePrefab(prefabGuid, parent);
                    if (prefab != null)
                        return prefab;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            return null;
        }
        public GameObject TryGetPrefab(string prefabGuid)
        {
            if (m_PrefabDictionary.TryGetValue(prefabGuid, out var prefab))
            {
                if (prefab != null)
                    return prefab;
                else
                {
                    Debug.LogError($"Prefab with guid [{prefabGuid}] is null!");
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// 【序列化时】获取Prefab的信息
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="guid"></param>
        public string GetPrefabMetadata_Runtime(GameObject prefab)
        {
            string guid = "";
            foreach (var pair in m_PrefabDictionary)
            {
                if (pair.Value == prefab)
                {
                    guid = pair.Key;
                    break;
                }
            }
            return guid;
        }

        #region ——Editor——
#if UNITY_EDITOR

        [ContextMenu("EditorScanData")]
        void EditorScanData()
        {
            //基于当前文件夹进行更新（常用于测试）
            string relatedPath = AssetDatabase.GetAssetPath(this);
            string sourceAbsDirPath = EditorPathTool.UnityRelateToAbsPath(relatedPath);
            string destAbsDirPath = System.IO.Directory.GetParent(sourceAbsDirPath).FullName;
            CreateFromFolder(destAbsDirPath, destAbsDirPath, relatedPath.GetFileNameWithoutExtension());
        }

        readonly Dictionary<UnityObject, string> m_GuidMap = new();

        /// <summary>
        /// 针对特定文件夹创建或更新SOAssetPack
        /// 
        /// Ref：Unity.RuntimeSceneSerialization.EditorInternal.MenuItems.SaveJsonScene
        /// </summary>
        /// <param name="sourceAbsDirPath">目标文件夹路径</param>
        /// <param name="destAbsDirPath">存储SOAssetPack的文件夹路径</param>
        public static void CreateFromFolder(string sourceAbsDirPath, string destAbsDirPath, string fileNameWithoutExtension = "AssetPack")
        {
            //#1 获取文件夹位置
            string relateDirPath = EditorPathTool.AbsToUnityRelatePath(sourceAbsDirPath);

            //#2 创建或清空已有SOAssetPack（位置为选中文件夹里）
            PathTool.GetOrCreateDir(destAbsDirPath);
            string assetPackPath = EditorPathTool.AbsToUnityRelatePath(destAbsDirPath + "/" + fileNameWithoutExtension + ".asset");
            SOAssetPack assetPack = AssetDatabase.LoadAssetAtPath<SOAssetPack>(assetPackPath);
            bool created = false;
            if (assetPack == null)
            {
                created = true;
                assetPack = ScriptableObject.CreateInstance<SOAssetPack>();
            }
            else
            {
                assetPack.Clear();//清空
            }

            //#3 扫描文件夹下的所有预制物
            var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new string[] { relateDirPath });
            foreach (var prefabGuid in prefabGuids)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
                assetPack.AddPrefabMetadata(prefabGuid, prefabPath);
            }

            //#3 保存SOAssetPack并刷新
            if (created)
            {
                //if (assetPack.AssetCount > 0)//不管有无资源都创建
                AssetDatabase.CreateAsset(assetPack, assetPackPath);
            }
            else
            {
                //if (assetPack.AssetCount > 0)
                EditorUtility.SetDirty(assetPack);
                //else if (AssetDatabase.LoadAssetAtPath<SOAssetPack>(assetPackPath) != null)
                //    AssetDatabase.DeleteAsset(assetPackPath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"{(created ? "Create" : "Update")} assetPack with [{assetPack.Prefabs.Count}] Prefabs at: " + assetPackPath);
        }

        /// <summary>
        /// Get the guid of a given prefab, storing the result in the given SOAssetPack, if provided
        /// </summary>
        /// <param name="prefabInstance">The prefab instance whose guid to find</param>
        /// <param name="guid">The guid, if one is found</param>
        /// <param name="assetPack">The SOAssetPack to store the prefab and guid</param>
        public static void GetPrefabMetadata(GameObject prefabInstance, out string guid, SOAssetPack assetPack = null)
        {
            var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefabInstance);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning($"Could not find prefab path for {prefabInstance.name}");
                guid = null;
                return;
            }

            guid = AssetDatabase.AssetPathToGUID(path);
            if (string.IsNullOrEmpty(guid))
            {
                Debug.LogWarning($"Could not find guid for {path}");
                return;
            }

            if (assetPack != null)
                assetPack.m_PrefabDictionary[guid] = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        /// <summary>
        /// 利用传入的Prefab资源信息初始化
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="path"></param>
        public void AddPrefabMetadata(string guid, string path)
        {
            if (m_PrefabDictionary.ContainsKey(guid))
                return;
            m_PrefabDictionary[guid] = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        void GetOrAddAssetMetadata(UnityObject obj, out string guid, out long fileId, bool warnIfMissing)
        {
            Asset asset;
            if (!m_GuidMap.TryGetValue(obj, out guid))
            {
                if (TryGetGUIDAndLocalFileIdentifier(obj, out guid, out fileId, warnIfMissing))
                {
                    m_GuidMap[obj] = guid;
                    if (!m_AssetDictionary.TryGetValue(guid, out asset))
                    {
                        asset = new Asset();
                        m_AssetDictionary[guid] = asset;
                    }

                    asset.AddAssetMetadata(obj, fileId);
                    return;
                }

                m_GuidMap[obj] = null;
                return;
            }

            if (string.IsNullOrEmpty(guid))
            {
                fileId = k_InvalidId;
                return;
            }

            if (!m_AssetDictionary.TryGetValue(guid, out asset))
            {
                asset = new Asset();
                m_AssetDictionary[guid] = asset;
            }

            asset.GetOrAddAssetMetadata(obj, out fileId, warnIfMissing);
        }

        static bool TryGetGUIDAndLocalFileIdentifier(UnityObject obj, out string guid, out long fileId, bool warnIfMissing)
        {
            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out guid, out fileId))
            {
                // ReSharper disable once CommentTypo
                // Check if target object is marked as "DontSave"--that means it is a scene object but won't be found in metadata
                // Otherwise, this is an error, and we cannot find a valid asset path
                // Suppress warning in certain edge cases (i.e. during deserialization before scene object metadata is set up)
                if ((obj.hideFlags & HideFlags.DontSave) == HideFlags.None && warnIfMissing)
                    Debug.LogWarningFormat("Could not find asset path for {0}", obj);

                guid = string.Empty;
                fileId = k_InvalidId;
                return false;
            }
            return true;
        }

        #region Scene Related （PS：新实现与Scene无关）
        //const string k_AssetPackFilter = "t:" + nameof(SOAssetPack);
        //static readonly Dictionary<SceneAsset, SOAssetPack> k_SceneToAssetPackCache = new();

        ///// <summary>
        ///// The associated SceneAsset for this SOAssetPack
        ///// </summary>
        //public UnityObject SceneAsset { set => m_SceneAsset = value; get => m_SceneAsset; }
        //[SerializeField]
        //UnityObject m_SceneAsset;//ToDelete

        ///// <summary>
        ///// Get the SOAssetPack associated with the given scene(找到Scene路径下匹配的SOAssetPack)
        ///// </summary>
        ///// <param name="sceneAsset">The SceneAsset which will be used to find the SOAssetPack</param>
        ///// <returns>The associated SOAssetPack, if one exists</returns>
        //public static SOAssetPack GetAssetPackForScene(SceneAsset sceneAsset)
        //{
        //    if (k_SceneToAssetPackCache.TryGetValue(sceneAsset, out var assetPack))
        //        return assetPack;

        //    var allAssetPacks = AssetDatabase.FindAssets(k_AssetPackFilter);
        //    foreach (var guid in allAssetPacks)
        //    {
        //        var path = AssetDatabase.GUIDToAssetPath(guid);
        //        if (string.IsNullOrEmpty(path))
        //            continue;

        //        var loadedAssetPack = AssetDatabase.LoadAssetAtPath<SOAssetPack>(path);
        //        if (loadedAssetPack == null)
        //            continue;

        //        var loadedSceneAsset = loadedAssetPack.SceneAsset as SceneAsset;
        //        if (loadedSceneAsset == null)
        //            continue;

        //        k_SceneToAssetPackCache[loadedSceneAsset] = loadedAssetPack;

        //        if (loadedSceneAsset == sceneAsset)
        //            assetPack = loadedAssetPack;
        //    }
        //    return assetPack;
        //}
        ///// <summary>
        ///// Remove a cached asset pack mapping
        ///// </summary>
        ///// <param name="sceneAsset">The SceneAsset associated to the SOAssetPack to remove</param>
        //public static void RemoveCachedAssetPack(SceneAsset sceneAsset)
        //{
        //    if (sceneAsset != null)
        //        k_SceneToAssetPackCache.Remove(sceneAsset);
        //}

        ///// <summary>
        ///// Clear the cached map of scenes to asset packs
        ///// </summary>
        //public static void ClearSceneToAssetPackCache() { k_SceneToAssetPackCache.Clear(); }
        #endregion

#endif
        #endregion

        #region Define
        [Serializable]
        public class Asset : ISerializationCallbackReceiver
        {
            [SerializeField]
            long[] m_FileIds;

            [SerializeField]
            UnityObject[] m_Assets;

            readonly Dictionary<long, UnityObject> m_FileIdToAssetMap = new();
            readonly Dictionary<UnityObject, long> m_AssetToFileIdMap = new();

            // Local method use only -- created here to reduce garbage collection. Collections must be cleared before use
            readonly List<long> k_FileIds = new();
            readonly List<UnityObject> k_Assets = new();

            public Dictionary<long, UnityObject> Assets => m_FileIdToAssetMap;

#if UNITY_EDITOR
            internal long AddAssetAndGetFileId(UnityObject asset, bool warnIfMissing)
            {
                if (asset == null)
                    return k_InvalidId;

                if (m_AssetToFileIdMap.TryGetValue(asset, out var fileId))
                    return fileId;

                if (!TryGetGUIDAndLocalFileIdentifier(asset, out _, out fileId, warnIfMissing))
                    return k_InvalidId;

                m_AssetToFileIdMap[asset] = fileId;
                m_FileIdToAssetMap[fileId] = asset;
                return fileId;
            }

            public void GetOrAddAssetMetadata(UnityObject obj, out long fileId, bool warnIfMissing)
            {
                if (m_AssetToFileIdMap.TryGetValue(obj, out fileId))
                    return;

                if (!TryGetGUIDAndLocalFileIdentifier(obj, out _, out fileId, warnIfMissing))
                    fileId = k_InvalidId;

                AddAssetMetadata(obj, fileId);
            }
#endif

            internal long GetFileIdOfAsset(UnityObject asset)
            {
                if (asset == null)
                    return k_InvalidId;

                return m_AssetToFileIdMap.TryGetValue(asset, out var fileId) ? fileId : k_InvalidId;
            }

            public UnityObject GetAsset(long fileId)
            {
                return m_FileIdToAssetMap.TryGetValue(fileId, out var asset) ? asset : null;
            }

            public void OnBeforeSerialize()
            {
                k_FileIds.Clear();
                k_Assets.Clear();
                foreach (var kvp in m_FileIdToAssetMap)
                {
                    var fileId = kvp.Key;
                    var asset = kvp.Value;

                    k_FileIds.Add(fileId);
                    k_Assets.Add(asset);
                }

                m_FileIds = k_FileIds.ToArray();
                m_Assets = k_Assets.ToArray();
            }

            public void OnAfterDeserialize()
            {
                if (m_FileIds == null || m_Assets == null)
                    return;

                var length = m_FileIds.Length;
                var assetLength = m_Assets.Length;
                if (assetLength < length)
                    Debug.LogWarning("Problem in Asset Pack. Number of assets is less than number of fileIds");

                m_FileIdToAssetMap.Clear();
                m_AssetToFileIdMap.Clear();
                for (var i = 0; i < length; i++)
                {
                    var fileId = m_FileIds[i];
                    if (i < assetLength)
                    {
                        var asset = m_Assets[i];
                        if (asset != null)
                            m_AssetToFileIdMap[asset] = fileId;

                        m_FileIdToAssetMap[fileId] = asset;
                    }
                    else
                    {
                        m_FileIdToAssetMap[fileId] = null;
                    }
                }
            }

            public bool TryGetFileId(UnityObject obj, out long fileId) { return m_AssetToFileIdMap.TryGetValue(obj, out fileId); }

            public void AddAssetMetadata(UnityObject obj, long fileId)
            {
                m_AssetToFileIdMap[obj] = fileId;
                m_FileIdToAssetMap[fileId] = obj;
            }
        }
        #endregion
    }

    #region Define
    /// <summary>
    /// 
    /// Prefab factories can be registered to AssetPacks to supplement their ability to instantiate prefabs（用于扩充生成Prefab的功能）
    /// 
    /// As an extension for AssetPacks, the IPrefabFactory API can be used to write custom code that provides prefab references on deserialization. To use this feature, define a class which implements the IPrefabFactory interface, and use the RegisterPrefabFactory method on the AssetPack being used to deserialize the scene before calling ImportScene. When deserializing a prefab, first the AssetPack is checked for a prefab with the corresponding GUID, and if it is not found, the prefab factories are queried. Because a HashSet is used to contain the list of prefab factories, they are generally queried in the order they were added, but this is not guaranteed.（https://docs.unity3d.com/Packages/com.unity.runtime-scene-serialization@1.0/manual/index.html）
    /// </summary>
    public interface IPrefabFactory
    {
        /// <summary>
        /// Try to instantiate a prefab with the given guid
        /// </summary>
        /// <param name="guid">The guid of the prefab that should be instantiated</param>
        /// <param name="parent">The parent object to use when calling Instantiate</param>
        /// <returns>The prefab instance, if one was created</returns>
        GameObject TryInstantiatePrefab(string guid, Transform parent = null);
    }
    #endregion
}
