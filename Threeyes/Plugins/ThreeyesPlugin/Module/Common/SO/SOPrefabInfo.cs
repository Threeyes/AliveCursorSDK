using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using Threeyes.Editor;
#endif
namespace Threeyes.Common
{

    //非必须，可以仅提供AD_SerializableItemPrefabInfo对应的So
    /// <summary>
    /// 提供预制物信息
    /// 
    /// ToAdd:
    /// -调用AssetMenuEditor_CreateAssetPrevew，增加一键创建Preview的通用方法，参考SZY
    /// </summary>
    [CreateAssetMenu(menuName = EditorDefinition_Common.AssetMenuPrefix_Common + "PrefabInfo", fileName = "PrefabInfo")]
    public class SOPrefabInfo : ScriptableObject
    {
        public string title;
        public GameObject prefab;
        public Texture preview;
        public string description;//[Optional]
        public string tooltip;//[Optional]

#if UNITY_EDITOR
        [ContextMenu("CreatePreview")]
        public void CreatePreview()
        {
            if (!prefab)
            {
                Debug.LogError("The prefab is null!");
                return;
            }
            preview = EditorTool.CreateAndSaveAssetPreview(prefab);
            EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// 与Prefab同名
        /// </summary>
        [ContextMenu("RenameLikePrefab")]
        public void RenameLikePrefab()
        {
            if (!prefab)
                return;
            EditorTool.Rename(this, prefab.name);
        }
#endif
    }
}