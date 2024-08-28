using System.Collections.Generic;
using Threeyes.Persistent;
using Threeyes.Data;
using UnityEngine;
using System.Text;
using Threeyes.Core.Editor;

namespace Threeyes.GameFramework
{
    /// <summary>
    /// According to PD_ Option value, switching corresponding skin (same as PersistentData_option+GameObject Helper function)
    /// </summary>
    [AddComponentMenu(GameFramework_EditorDefinition.ComponentMenuPrefix_Persistent + "PersistentData_OptionSkin", 0)]
    public class PersistentData_OptionSkin : PersistentData_Option
    {
        public override bool IsValid { get { return tfSkinGroup != null && base.IsValid && dataOption.listOptionData.Count > 0; } }

        protected Transform TfSkinGroup { get { return tfSkinGroup; } set { tfSkinGroup = value; } }
        [SerializeField] private Transform tfSkinGroup;

        void Reset()
        {
            key = "Skin";
        }

        //ToUpdate:Auto Setup
        [ContextMenu("Setup")]
        void Setup()
        {
            var listOptionData = new List<DataOption_OptionInfo.OptionData>();
            foreach (Transform tfSkin in tfSkinGroup)
            {
                listOptionData.Add(new DataOption_OptionInfo.OptionData(tfSkin.name));
            }
            dataOption.listOptionData = listOptionData;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        public override void OnValueChanged(int value, PersistentChangeState persistentChangeState)
        {
            base.OnValueChanged(value, persistentChangeState);
            SetChildActiveSolo(value);
        }
        public void SetChildActiveSolo(int index)
        {
            if (!tfSkinGroup)
                return;

            for (int i = 0; i != tfSkinGroup.childCount; i++)
            {
                Transform tfChild = tfSkinGroup.GetChild(i);
                tfChild.gameObject.SetActive(i == index);
            }
            if (index >= tfSkinGroup.childCount)
            {
                Debug.LogError($"{index}/{tfSkinGroup.childCount} Out of bounds!");
            }
        }


        #region Editor Method
#if UNITY_EDITOR

        //——MenuItem——
        static string instName_Skin = "OptionPD_Skin";
        [UnityEditor.MenuItem(GameFramework_EditorDefinition.HierarchyMenuPrefix_Persistent + "OptionPD_Skin", false)]
        public static void CreateInst_Skin()
        {
            EditorTool.CreateGameObjectAsChild<PersistentData_OptionSkin>(instName_Skin);
        }

        //——Hierarchy GUI——
        public override string ShortTypeName { get { return "OptionPD_Skin"; } }
        //——Inspector GUI——
        public override void SetInspectorGUIHelpBox_Error(StringBuilder sB)
        {
            base.SetInspectorGUIHelpBox_Error(sB);
            if (TfSkinGroup == null)
            {
                sB.Append("TfSkinGroup can't be null!");
                sB.Append("\r\n");
            }
            else if (dataOption.listOptionData.Count == 0)
            {
                sB.Append("Please invoke SetUp method in ContextMenu!");
                sB.Append("\r\n");
            }
        }
#endif
        #endregion
    }
}