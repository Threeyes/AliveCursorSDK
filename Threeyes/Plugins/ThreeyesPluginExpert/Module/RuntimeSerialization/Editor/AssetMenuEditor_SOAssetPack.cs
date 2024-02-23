#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Threeyes.Core.Editor;

namespace Threeyes.RuntimeSerialization
{
    /// <summary>
    /// 
    /// </summary>
    public static class AssetMenuEditor_SOAssetPack
    {
        private const string MenuAssetPath = "Assets/";
        private const string MenuAssetPath_RuntimeSerialization = MenuAssetPath + "RuntimeSerialization/";
        private const int Priority = 2000;

        //已完成
        [MenuItem(MenuAssetPath_RuntimeSerialization + "Create SOAssetPack from this folder", false, Priority)]
        private static void CreateFromThisFolder()
        {
            //——以下参考Unity.RuntimeSceneSerialization.EditorInternal.SaveJsonScene——

            var guids = Selection.assetGUIDs;
            if (guids.Length != 1)
                return;

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            //Debug.LogError(path);
      
            string sourceAbsDirPath = EditorPathTool.UnityRelateToAbsPath(path);
            string destAbsDirPath = Directory.GetParent(sourceAbsDirPath).FullName;
            SOAssetPack.CreateFromFolder(sourceAbsDirPath, destAbsDirPath);
        } 
    }
}
#endif