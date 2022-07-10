#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Threeyes.Editor;
using System.IO;
using System.Linq;
namespace Threeyes.Persistent
{
    /// <summary>
    /// 为Assets文件夹中的SO生成对应的PD场景实例
    /// 
    /// 使用方法：
    /// 1.首次创建时：选择需要所有序列化SO文件的 单个 根目录，然后点击右键调用以下指定方法即可
    /// 2.后续更新时可以选中任意子文件夹并再次调用同样的方法，可更新已有的场景PD实例而不是以选中文件夹作为根目录（Todo：改为支持选中多个子文件夹）
    /// </summary>
    public static class AssetMenuEditor_PersistentFile
    {
        private const int Priority = 2999;

        ///ToAdd：增加分支选项：
        ///仅更新场景中已存在的ControllerManager（针对子场景项目中,只需要管理该场景资源的项目)（或者可以统一放在启动界面中进行管理，缺点就是加载很慢）


        /// <summary>
        /// 为选中文件夹中符合的素材（如SO），生成对应的场景PD
        /// 
        /// </summary>
        [MenuItem("Assets/Persistent/创建并存储在StreamingAsset文件夹", false, Priority)]
        static void CreatePersistentDataForSelectDir_StreamingAsset()
        {
            CreateFunc(ExternalFileLocation.StreamingAsset);
        }
        [MenuItem("Assets/Persistent/创建并存储在Data文件夹", false, Priority + 1)]
        static void CreatePersistentDataForSelectDir_Data()
        {
            CreateFunc(ExternalFileLocation.CustomData);
        }
        private static void CreateFunc(ExternalFileLocation externalFileLocation)
        {
            List<string> assetPaths = EditorTool.GetSelectionAssetPaths();
            if (assetPaths.Count == 0) { return; }

            //Set the first select dir as RootPath
            string rootAbsPath = EditorPathTool.UnityRelateToAbsPath(assetPaths[0]);
            DirectoryInfo rootDI = new DirectoryInfo(rootAbsPath);
            if (!rootDI.Exists)
                return;

            //找到所有继承ScriptableObject的资源
            List<AssetFileInfo> listAsset = new List<AssetFileInfo>();
            foreach (FileInfo fileInfo in PathTool.GetSubFiles(rootDI, searchPattern: "*.asset"))
            {
                string relateFilePath = EditorPathTool.AbsToUnityRelatePath(fileInfo.FullName);
                ScriptableObject sO = AssetDatabase.LoadAssetAtPath<ScriptableObject>(relateFilePath);
                if (sO)
                {
                    //Debug.Log(sO.name);
                    AssetFileInfo assetFileInfo = new AssetFileInfo()
                    {
                        fileInfo = fileInfo,
                        scriptableObject = sO
                    };
                    listAsset.Add(assetFileInfo);
                }
            }

            //根据<文件夹绝对路径,List<AssetFileInfo>>进行分组
            var dirGroups = from asset in listAsset
                            group asset by asset.fileInfo.Directory.FullName into newGroup
                            select newGroup;

            PersistentControllerManagerGroup controllerManagerGroup = PersistentControllerManagerGroup.Instance;//如果场景没有该物体，则会自动创建

            Undo.RegisterFullObjectHierarchyUndo(controllerManagerGroup, "PersistentControllerManagerGroup Update");

            foreach (var dirGroup in dirGroups)
            {
                ///查找或创建场景中对应的PersistentControllerManager_Json
                string subDirPath_startFromRoot = PathTool.ConvertToUnityFormat(dirGroup.Key.Substring(rootDI.Parent.FullName.Length + 1));//获取从Root开始的相对路径 (通过+1，将后面的\也裁掉)
                PersistentControllerManager_File relateControllerManager = controllerManagerGroup.ListComp.FirstOrDefault(
                    (cm) =>
                    {
                        PersistentControllerManager_File cm_Json = cm as PersistentControllerManager_File;
                        if (cm_Json)
                        {
                            //PS.有可能开发者选择了子目录，因此只需要确定后面的路径的完全一致即可
                            string absDirGroup = dirGroup.Key;
                            string cmSubDirPath = PathTool.ConvertToSystemFormat(cm_Json.subDirPath);
                            Debug.Log(absDirGroup + "  " + cmSubDirPath+" =>"+ absDirGroup.EndsWith(cmSubDirPath));
                            return absDirGroup.EndsWith(cmSubDirPath);
                        }
                        return false;
                    }) as PersistentControllerManager_File;

                if (relateControllerManager == null)
                {
                    relateControllerManager = new GameObject().AddComponent<PersistentControllerManager_File>();
                    relateControllerManager.subDirPath = subDirPath_startFromRoot;
                    relateControllerManager.transform.SetParent(controllerManagerGroup.transform);
                    relateControllerManager.name = "PersistentControllerManager (" + subDirPath_startFromRoot + ")";
                    Debug.Log("创建PCM: " + subDirPath_startFromRoot);
                }
                else
                {
                    Debug.Log("更新PCM: " + subDirPath_startFromRoot);
                }
                //更新配置
                relateControllerManager.transform.DestroyAllChild();
                relateControllerManager.externalFileLocation = externalFileLocation;


                foreach (AssetFileInfo asset in dirGroup)
                {
                    ScriptableObject scriptableObject = asset.scriptableObject;
                    string goName = nameof(PersistentData_SO) + " (" + scriptableObject.name + ")";
                    PersistentData_SO persistentData = new GameObject(goName).AddComponent<PersistentData_SO>();
                    persistentData.Key = scriptableObject.name;
                    persistentData.transform.SetParent(relateControllerManager.transform);
                    persistentData.TargetValue = scriptableObject;
                }
            }

            EditorUtility.SetDirty(controllerManagerGroup);
            ///ToDo:
            ///1.将文件夹名称（相对于选中文件夹的路径）作为Controller的相对路径
            ///2.如果文件是ScriptableObject，那就创建对应实例
            ///找到场景中的PersistentControllerManagerGroup单例并作为其子物体（如果同级有相同物体，那就直接替换）
        }

        //类似于ProjectManager
        class AssetFileInfo
        {
            public FileInfo fileInfo;
            public ScriptableObject scriptableObject;
        }
    }
}
#endif