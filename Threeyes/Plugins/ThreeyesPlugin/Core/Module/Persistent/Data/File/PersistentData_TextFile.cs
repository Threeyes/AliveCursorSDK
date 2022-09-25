using UnityEngine;
using Threeyes.Data;

namespace Threeyes.Persistent
{
	/// <summary>
	/// (Basicly Support external file with any extension)
	/// 
	/// Warning:
	/// 1.Ref: Please notice that files with the .txt and .bytes extension will be treated as text and binary files, respectively. Do not attempt to store a binary file using the .txt extension, as this will create unexpected behaviour when attempting to read data from it.(只有bytes后缀才能原样存读，通过其他后缀初始化时可能会导致数据丢失的问题，这时应该使用)(https://docs.unity3d.com/Manual/class-TextAsset.html)（https://issuetracker.unity3d.com/issues/textasset-dot-bytes-returns-different-bytes-with-different-file-extensions）
	/// </summary>
	public class PersistentData_TextFile : PersistentData_FileBase<TextAsset, TextAssetEvent, DataOption_TextFile>
    {
        [SerializeField] protected StringEvent onStringChanged;

        protected override void NotifyAssetChanged(TextAsset asset)
        {
            base.NotifyAssetChanged(asset);
            if (asset)
                onStringChanged.Invoke(asset.text);
        }

#if UNITY_EDITOR

        //——MenuItem——
        static string instName = "TextPD ";
        [UnityEditor.MenuItem(strMenuItem_Root_File + "Text", false, intBasicMenuOrder + 0)]
        public static void CreateInst()
        {
            Editor.EditorTool.CreateGameObjectAsChild<PersistentData_TextFile>(instName);
        }

        //——Hierarchy GUI——
        public override string ShortTypeName { get { return "Text"; } }

#endif
    }

}