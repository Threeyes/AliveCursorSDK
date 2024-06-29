using System.Collections.Generic;
using UnityEngine;
using Threeyes.Core;
using System.Linq;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Threeyes.RuntimeSerialization
{
#if UNITY_EDITOR
    /// <summary>
    /// 用于编辑器更新特定组件的属性，如
    /// -Prefab Asset的RSGO信息，这类资源无法通过OnValidate更新
    /// </summary>
    public class RuntimeSerializationUpdater
    {
        static List<RuntimeSerializable_GameObject> listRSGO_ClearInstanceID = new List<RuntimeSerializable_GameObject>();//标记为需要清空InstanceID

        static List<RuntimeSerializable_GameObject> listRSGO_SetPersistentPrefabID = new List<RuntimeSerializable_GameObject>();//标记为需要初始化PersistentPrefabID

        public static void Register_ClearInstanceID(RuntimeSerializable_GameObject runtimeSerializable_GameObject)
        {
            listRSGO_ClearInstanceID.AddOnce(runtimeSerializable_GameObject);
        }

        public static void Register_UpdatePersistentPrefabID(RuntimeSerializable_GameObject runtimeSerializable_GameObject)
        {
            listRSGO_SetPersistentPrefabID.AddOnce(runtimeSerializable_GameObject);
        }

        static RuntimeSerializationUpdater()
        {
            //PrefabUtility.prefabInstanceUnpacked += OnPrefabInstanceUnpacked;//【V2】Todo:针对unpack的RSGO，需要清空其PersistentPrefabID
            EditorApplication.update += EditorUpdate;
        }

        private static void OnPrefabInstanceUnpacked(GameObject arg1, PrefabUnpackMode arg2)
        {
            throw new NotImplementedException();
        }

        static void EditorUpdate()
        {
            //# 清除有残留的InstanceID（Prefab不应该有实例ID）
            ForEachList(ref listRSGO_ClearInstanceID,
            (prefabPath, prefabGuid, prefabRoot, rSGO) =>
            {
                if (rSGO.TryClearInstanceID())
                {
                    Debug.LogWarning($"Test Clear {prefabRoot}'s PersistentPrefabID!");
                }
            });

            //# 更新RSGO的PersistentPrefabID
            ForEachList(ref listRSGO_SetPersistentPrefabID,
                (prefabPath, prefabGuid, prefabRoot, rSGO) =>
                {
                    if (rSGO.TryUpdatePersistentPrefabID(prefabGuid))
                    {
                        Debug.LogWarning($"Test Set {prefabRoot}'s PersistentPrefabID to {prefabGuid}!");
                    }
                });
        }

        /// <summary>
        /// 对列表的每一项都进行设置后移除
        /// </summary>
        /// <param name="listRSGO"></param>
        /// <param name="action">param:prefabPath, prefabGuid</param>
        static void ForEachList(ref List<RuntimeSerializable_GameObject> listRSGO, Action<string, string, GameObject, RuntimeSerializable_GameObject> action)
        {
            if (listRSGO.Count == 0) return;
            while (listRSGO.Count > 0)
            {
                RuntimeSerializable_GameObject rSGO = listRSGO.FirstOrDefault();
                listRSGO.Remove(rSGO);
                try
                {
                    if (!rSGO) continue;
                    string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(rSGO);
                    if (prefabPath.IsNullOrEmpty()) continue;
                    string prefabGuid = AssetDatabase.AssetPathToGUID(prefabPath);

                    //更新RSGO的PersistentPrefabID
                    using (var editingScope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
                    {
                        GameObject prefabRoot = editingScope.prefabContentsRoot;
                        RuntimeSerializable_GameObject runtimeSerializable_GameObject = prefabRoot.GetComponent<RuntimeSerializable_GameObject>();

                        if (runtimeSerializable_GameObject)
                            action.Execute(prefabPath, prefabGuid, prefabRoot, runtimeSerializable_GameObject);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }
    }
#endif
}