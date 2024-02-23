using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using Threeyes.Core.Editor;
using Threeyes.Core;
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
    [CreateAssetMenu(menuName = EditorDefinition_Common.AssetMenuPrefix_SO_Common + "PrefabInfo", fileName = "PrefabInfo")]
    public class SOPrefabInfo : ScriptableObject
    {
        public virtual string Tooltip { get { return tooltip; } }
        public virtual string Title { get { return title; } set { title = value; } }
        public virtual GameObject Prefab { get { return prefab; } set { prefab = value; } }
        public virtual Texture Preview { get { return preview; } set { preview = value; } }
        public virtual string Description { get { return description; } set { description = value; } }

#if USE_NaughtyAttributes
        [ResizableTextArea]
#endif
        [SerializeField] protected string remark;//开发者内部注释
        [Space]
        [SerializeField] protected string title;
        [SerializeField] protected GameObject prefab;
        [SerializeField] protected Texture preview;
        [SerializeField] protected string description;//[Optional]
        [SerializeField] protected string tooltip;//[Optional]

#if UNITY_EDITOR
        [ContextMenu("ClearData")]
        public void ClearData()
        {
            ClearDataFunc();
        }

        private void ClearDataFunc(bool isIncludePrefab = true)
        {
            remark = "";
            title = "";
            if (isIncludePrefab)
                prefab = null;
            preview = null;
            description = "";
            tooltip = "";
        }

        [ContextMenu("InitAfterPrefab")]
        public void InitAfterPrefab()
        {
            if (!Prefab)
            {
                Debug.LogError("Prefab is null!");
                return;
            }

            ClearDataFunc(false);
            RenameAfterPrefab();
            CreatePreview();
        }

        /// <summary>
        /// 将文件名改为Prefab的同名
        /// </summary>
        [ContextMenu("RenameAfterPrefab")]
        public void RenameAfterPrefab()
        {
            if (!Prefab)
                return;
            EditorTool.Rename(this, Prefab.name);

            if (title.IsNullOrEmpty())
                title = Prefab.name;
        }
  
        [ContextMenu("CreatePreview")]
        public void CreatePreview()
        {
            if (!Prefab)
            {
                Debug.LogError("The prefab is null!");
                return;
            }
            Preview = EditorTool.CreateAndSaveAssetPreview(Prefab);
            EditorUtility.SetDirty(this);
        }
#endif
    }
}