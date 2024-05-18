#if UNITY_EDITOR
using System.IO;
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
        /// <summary>
        /// 获取父文件夹的相对路径
        /// </summary>
        /// <param name="unityRelatePath"></param>
        /// <returns></returns>
        public static string GetUnityRelateParentPath(string unityRelatePath)
        {
            string absPath = UnityRelateToAbsPath(unityRelatePath);
            string absParentPath = Directory.GetParent(absPath).FullName;//PS：Directory.GetParent会强制返回绝对路径，所以需要进行转换
            return AbsToUnityRelatePath(absParentPath);
        }
    }
}
#endif