#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace Threeyes.Editor
{
    public static class AssetMenuEditor_ScriptChangeEncoding
    {
        //PS:通过在根目录增加“.editorconfig”文件，从而使VS默认以UTF8格式保存代码(https://stackoverflow.com/questions/41335199/how-to-config-visual-studio-to-use-utf-8-as-the-default-encoding-for-all-project)
        [MenuItem(EditorDefinition.AssetMenuItemPrefix + "VS Save Script as UTF8")]
        private static void VSSaveScriptAsUTF8()
        {
            string projectDir = PathTool.ProjectDirPath;
            string editorconfigDir = Path.Combine(projectDir, ".editorconfig");
            if (!File.Exists(editorconfigDir))
            {
                string content = "root = true\r\n\r\n[*.cs]\r\ncharset = utf-8";
                File.WriteAllText(editorconfigDir, content);
            }
        }

        //[MenuItem(EditorDefinition.AssetMenuItemPrefix + "Change Script Encoding to UTF8")]
        //private static void ChangeEncodingToUTF8()
        //{
        ////ToUpdate:以下直接更改编码的方法无效会导致保存后的UTF8文件中文乱码，有可能时读取时使用的编码方式不一样，待更新
        //List<string> assetPaths = EditorTool.GetSelectionAssetPaths();
        //if (assetPaths.Count == 0)
        //    return;

        //foreach (var assetpath in assetPaths)
        //{
        //    string rootAbsPath = EditorPathTool.UnityRelateToAbsPath(assetpath);

        //    foreach (FileInfo fileInfo in new DirectoryInfo(rootAbsPath).GetFiles("*.cs", SearchOption.AllDirectories))
        //    {
        //        string s = File.ReadAllText(fileInfo.FullName);
        //        File.WriteAllText(fileInfo.FullName, s, Encoding.UTF8);
        //        //Debug.Log($"Change {fileInfo.Name} into UTF8");
        //    }
        //}
        //AssetDatabase.Refresh();
        //}
    }
}
#endif