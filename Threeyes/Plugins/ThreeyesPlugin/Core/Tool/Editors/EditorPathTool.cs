#if UNITY_EDITOR
using Threeyes.Core;
using UnityEngine;

namespace Threeyes.Core.Editor
{
    public static class EditorPathTool
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objAsset"></param>
        /// <returns></returns>
        public static string GetAssetAbsPath(Object objAsset)
        {
            if (!objAsset)
                return "";
            return UnityRelateToAbsPath(UnityEditor.AssetDatabase.GetAssetPath(objAsset));
        }

        /// <summary>
        /// Unity local path to abs path
        /// </summary>
        /// <param name="unityRelatePath"></param>
        /// <returns></returns>
        public static string UnityRelateToAbsPath(string unityRelatePath)
        {
            int length = unityRelatePath.Length;
            if (length >= 6)
            {
                return Application.dataPath + unityRelatePath.Substring(6, length - 6);//裁剪掉unityRelatePath的"Assets/"
            }
            Debug.LogError($"Relate path length error: [{unityRelatePath}]");
            return unityRelatePath;
        }
        public static string AbsToUnityRelatePath(string absPath)
        {
            //转换成同一样式
            absPath = PathTool.ConvertToSystemFormat(absPath);
            string appDataPath = PathTool.ConvertToSystemFormat(Application.dataPath);
            string relativepath = absPath;
            if (absPath.StartsWith(appDataPath))
            {
                relativepath = "Assets" + absPath.Substring(appDataPath.Length);
            }
            else
            {
                Debug.LogError("The path is not inside this Unity Project:\r\n" + absPath);
            }
            return relativepath;
        }

    }
}
#endif