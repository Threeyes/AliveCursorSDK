using System.Text;
using UnityEngine;
using Threeyes.Data;
using Threeyes.Core.Editor;
using Threeyes.Core;

namespace Threeyes.Persistent
{
    /// <summary>
    /// Read bytes file, mainly for decoding (eg: gif、video)
    /// (Basicly Support external file with any extension, but you can set the overrideFileFilterExtensions in Option to limit the valid file type) 
    /// 
    /// PS:
    /// 1.SOBytesInfo用于在Unity中提供可编辑的实体，以及链接Unity编辑器内部资源
    /// </summary>
    public class PersistentData_BytesFile : PersistentData_FileBase<SOBytesAsset, SOBytesAssetEvent, DataOption_BytesFile>
    {
        public override bool IsValid { get { return base.IsValid && DefaultAsset.textAsset; } }
        public override bool UseCache { get { return false; } }//PS:每次加载都需要从File中读取bytes，因此不能使用Cache
        [SerializeField] protected BytesEvent onBytesChanged;//Use this to Decode any file (eg: gif)

        protected override void NotifyAssetChanged(SOBytesAsset asset)
        {
            base.NotifyAssetChanged(asset);
            onBytesChanged.Invoke(asset.bytes);
        }
        protected override void SaveDefaultAssetFunc()
        {
            //PS:不需要存储DefaultAsset，因为SOBytesInfo是Unity内部类型
            //base.SaveDefaultAssetFunc();
        }

#if UNITY_EDITOR

        //——MenuItem——
        static string instName = "BytesPD ";
        [UnityEditor.MenuItem(strMenuItem_Root_File + "Bytes", false, intBasicMenuOrder + 2)]
        public static void CreateInst_Bytes()
        {
            EditorTool.CreateGameObjectAsChild<PersistentData_BytesFile>(instName);
        }

        //——Hierarchy GUI——
        public override string ShortTypeName { get { return "Bytes"; } }

        //——Inspector GUI——
        public override void SetInspectorGUIHelpBox_Error(StringBuilder sB)
        {
            base.SetInspectorGUIHelpBox_Error(sB);
            if (DefaultAsset && !DefaultAsset.textAsset)
            {
                sB.Append("textAsset in DefaultAsset can't null!");
                sB.Append("\r\n");
            }
        }
#endif
    }

}