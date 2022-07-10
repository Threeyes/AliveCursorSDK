#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
/// <summary>
/// 功能：复制项目文件的路径
/// 来源：https://forum.unity.com/threads/please-include-a-copy-path-when-right-clicking-a-game-object.429480/
/// </summary>
namespace Threeyes.Editor
{
    public static class AssetMenuEditor_AssetPath
    {
        private const string MenuAssetPath = "Assets/";
        private const int Priority = 2000;

        [MenuItem(MenuAssetPath + "Copy Full Path #%c", false, Priority + 2)]
        private static void CopyFullPath()
        {
            var guids = Selection.assetGUIDs;

            if (guids.Length > 0)
            {
                var assetPath = System.IO.Path.Combine(Application.dataPath, AssetDatabase.GUIDToAssetPath(guids[0]).Replace("Assets/", string.Empty));
                assetPath = assetPath.Replace('/', '\\');
                EditorGUIUtility.systemCopyBuffer = assetPath;
                Debug.Log("复制路径：\r\n" + assetPath);
            }
        }

        /// <summary>
        /// Find Assets inside Assets
        /// </summary>
        /// <param name="filter">筛选器（如果只传入字符串，那就匹配名字含有该字符串的文件；可用通过l:t:等进行进一步筛选）（https://docs.unity3d.com/ScriptReference/AssetDatabase.FindAssets.html）</param>
        /// <returns></returns>
        public static List<T> FindAsset<T>(string filter) where T : UnityEngine.Object
        {
            //Ref: https://forum.unity.com/threads/trouble-with-assetdatabase-findassets.312240/
            string[] assetsGUIDs = AssetDatabase.FindAssets(filter);
            List<T> listResult = new List<T>();
            foreach (string guid in assetsGUIDs)
            {
                T inst = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)) as T;
                if (inst)
                    listResult.Add(inst);
            }
            return listResult;
        }

        [MenuItem(MenuAssetPath + "Copy Full Path %#2", true, Priority + 2)]
        private static bool CopyFullPathValidation()
        {
            return Selection.assetGUIDs.Length == 1;//判断是否只选中一个文件夹
        }

        //[MenuItem(MenuAssetPath + "Copy Path In Assets %#1", false, Priority + 1)]
        private static void CopyPathInAssets()
        {
            var guids = Selection.assetGUIDs;

            var assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            EditorGUIUtility.systemCopyBuffer = assetPath;
        }

        //[MenuItem(MenuAssetPath + "Copy Path In Assets %#1", true, Priority + 1)]
        private static bool CopyPathInAssetsValidation()
        {
            return Selection.assetGUIDs.Length == 1;
        }

        //[MenuItem(MenuAssetPath + "Copy Path In Resources %#0", false, Priority)]
        //private static void CopyPathInResources()
        //{
        //    var guids = Selection.assetGUIDs;

        //    var assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        //    assetPath = assetPath.Substring(assetPath.IndexOf("Resources/", StringComparison.Ordinal) +
        //                                    "Resources/".Length);
        //    var extensionPos = assetPath.LastIndexOf(".", StringComparison.Ordinal);
        //    if (extensionPos >= 0)
        //        assetPath = assetPath.Substring(0, extensionPos);

        //    EditorGUIUtility.systemCopyBuffer = assetPath;
        //}

        //[MenuItem(MenuAssetPath + "Copy Path In Resources %#0", true, Priority)]
        //private static bool CopyPathInResourcesValidation()
        //{
        //    return Selection.assetGUIDs.Length == 1 &&
        //           AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]).Contains("Resources");
        //}

    }
}
#endif