using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Threeyes.Persistent
{
    /// <summary>
    /// Save relate/abs filePath
    /// 
    /// PS:
    /// 1.Will expose the abs file path via event, you can use it to setup the following Component:
    ///     -Set VideoPlayer's file url via AC_VideoPlayerHelper.SetFileUrlAndPlay method
    /// </summary>
    public class PersistentData_FilePath : PersistentData_String
    {
        [SerializeField] protected StringEvent onAbsFilePathChanged;//对应加载的资源，便于外部调用其他组件        

        public override void OnValueChanged(string value, PersistentChangeState persistentChangeState)
        {
            base.OnValueChanged(value, persistentChangeState);
            onAbsFilePathChanged.Invoke(PathTool.GetAbsPath(PersistentDirPath, value));
        }

#if UNITY_EDITOR

        //——MenuItem——
        static string instName = "FilePathPD ";
        [UnityEditor.MenuItem(strMenuItem_Root_Basic_Ex + "PersistentData_FilePath", false, intBasicMenuOrder)]
        public static void CreateInst_FilePath()
        {
            Editor.EditorTool.CreateGameObjectAsChild<PersistentData_FilePath>(instName);
        }

        //——Hierarchy GUI——
        public override string ShortTypeName { get { return "FilePath"; } }

#endif

    }
}