#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Threeyes.Core.Editor
{
    public static class AssetDatabaseTool
    {
        /// <summary>
        /// Find Assets inside Assets
        /// 
        /// </summary>
        /// <param name="filter">筛选器（如果只传入字符串，那就匹配名字含有该字符串的文件；可用通过l:（Label）  t:（Type）等进行进一步筛选）（https://docs.unity3d.com/ScriptReference/AssetDatabase.FindAssets.html）（示例：xFile l:architecture t:texture2D）</param>
        /// <returns></returns>
        public static List<T> LoadAssets<T>(string filter, string[] searchInFolders = null) where T : Object
        {
            //Ref: https://forum.unity.com/threads/trouble-with-assetdatabase-findassets.312240/
            string[] assetsGUIDs = AssetDatabase.FindAssets(filter, searchInFolders);
            List<T> listResult = LoadAssetsByGUIDs<T>(assetsGUIDs);
            return listResult;
        }

        public static List<T> LoadAssetsByGUIDs<T>(string[] assetsGUIDs) where T : Object
        {
            List<T> listResult = new List<T>();
            foreach (string guid in assetsGUIDs)
            {
                T inst = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid)) as T;
                if (inst)
                    listResult.Add(inst);
            }
            return listResult;
        }
    }
}
#endif