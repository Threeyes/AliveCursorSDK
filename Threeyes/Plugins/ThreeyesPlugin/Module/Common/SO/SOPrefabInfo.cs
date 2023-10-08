using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using Threeyes.Editor;
#endif
#if USE_NaughtyAttributes
using NaughtyAttributes;
#endif

namespace Threeyes.Common
{
    /// <summary>
    /// 提供预制物信息
    /// 
    /// ToAdd:
    /// +调用AssetMenuEditor_CreateAssetPrevew，增加一键创建Preview的通用方法
    /// </summary>
    [CreateAssetMenu(menuName = EditorDefinition_Common.AssetMenuPrefix_Common + "PrefabInfo", fileName = "PrefabInfo")]
    public class SOPrefabInfo : ScriptableObject
    {
#if USE_NaughtyAttributes
        [ResizableTextArea]
#endif
        public string remark;//开发者内部注释
        [Space]

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
        /// 将文件名改为Prefab的同名
        /// </summary>
        [ContextMenu("RenameAfterPrefab")]
        public void RenameAfterPrefab()
        {
            if (!prefab)
                return;
            EditorTool.Rename(this, prefab.name);
        }
#endif
    }
}