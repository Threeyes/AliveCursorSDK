#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine.Events;
using UnityEditor.Callbacks;
using System;
using System.Linq;
using UnityEditor.SceneManagement;

#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;
#endif

namespace Threeyes.Editor
{
    /// <summary>
    /// 项目自动打包
    /// 参考：http://www.manew.com/blog-96480-3391.html
    /// </summary>
    public static partial class AssetMenuEditor_BuildPlayer
    {
        public static SOBuildInfo curBuildInfo;

        [MenuItem(EditorDefinition.TopMenuItemPrefix + "打包上次选中场景 &%b")]
        public static void BuildCur()
        {
            Debug.Log("Inter");
            if (SOBuildInfoManager.Instance.CurBuildInfo)
            {
                BulidTarget(SOBuildInfoManager.Instance.CurBuildInfo);
            }
        }
        /// <summary>
        /// 打包所有工程
        /// </summary>
        [MenuItem("Assets/Build Select")]
        public static void BuildSelect()
        {
            List<SOBuildInfo> listBuildInfo = new List<SOBuildInfo>();

            foreach (UnityEngine.Object obj in Selection.objects)
            {
                if (obj is SOBuildInfo)
                {
                    listBuildInfo.Add(obj as SOBuildInfo);
                }
            }

            Debug.Log("Build Info Count: " + listBuildInfo.Count);
            foreach (SOBuildInfo buildInfo in listBuildInfo)
            {
                BulidTarget(buildInfo, false);
            }
            Debug.Log("Build All Finish");
        }

        /// <summary>
        /// 打包所有工程
        /// </summary>
        [MenuItem("Assets/Build Select and Run")]
        public static void BuildSelectAndRun()
        {
            List<SOBuildInfo> listBuildInfo = new List<SOBuildInfo>();

            foreach (UnityEngine.Object obj in Selection.objects)
            {
                if (obj is SOBuildInfo)
                {
                    listBuildInfo.Add(obj as SOBuildInfo);
                }
            }
            //Resources.FindObjectsOfTypeAll<SOBuildInfo>().ToList();

            Debug.Log("Build Info Count: " + listBuildInfo.Count);
            foreach (SOBuildInfo buildInfo in listBuildInfo)
            {
                BulidTarget(buildInfo, true);
            }
            Debug.Log("Build All Finish");
        }

        /// <summary>
        /// 增加该Attribute，打包完成后被Unity调用
        /// https://docs.unity3d.com/ScriptReference/Callbacks.PostProcessBuildAttribute.html
        /// </summary>
        /// <param name="target"></param>
        /// <param name="pathToBuiltProject"></param>
        [PostProcessBuild(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
        }

        public static void CopyExtraFiles(SOBuildInfo buildInfo)
        {
            //生成Log文件(Todo:不要用json，直接写入
            string buildLogFileName = "BuildLog.txt";
            string buildLogFilePath = GetBuildFolderPath(buildInfo) + "/" + buildLogFileName;
            string content = "";
            content += "BuildTime：\t\t" + DateTime.Now.Serialize() + "\r\n";
            content += "UserName：\t\t" + Environment.UserName + "\r\n";
            content += "UserDomainName:\t" + Environment.UserDomainName + "\r\n";
            content += "\r\n";
            content += "更新描述:\r\n";
            var listUpdateDescriptionInfo = buildInfo.listUpdateDescriptionInfo.SimpleClone();
            listUpdateDescriptionInfo.Reverse();//反向列出，便于观看
            content += listUpdateDescriptionInfo.ConnectToString("\r\n\r\n");
            File.WriteAllText(buildLogFilePath, content);

            //复制Icon
            if (buildInfo.icon)
            {
                string absIconPath = PathTool.ProjectDirPath + "/" + AssetDatabase.GetAssetPath(buildInfo.icon);

                //复制并重命名为Icon
                string absBuildIconPath = GetBuildFolderPath(buildInfo) + "/" + PathDefinition.relateIconFolderPath;
                if (!Directory.Exists(absBuildIconPath))
                {
                    Directory.CreateDirectory(absBuildIconPath);
                }
                File.Copy(absIconPath, absBuildIconPath + PathDefinition.iconPicName, true);
            }
        }

        /// <summary>
        /// 返回中间的路径(OEM)
        /// </summary>
        /// <param name="buildInfo"></param>
        /// <returns></returns>
        public static string GetMiddleOEMFolder(SOBuildInfo buildInfo)
        {
            string middlePath = "";

            if (buildInfo.companyName.NotNullOrEmpty())
            {
                middlePath += buildInfo.companyName.ToString() + "/";
            }
            if (buildInfo.oemName.NotNullOrEmpty())
            {
                middlePath += buildInfo.oemName + "/";
            }
            return middlePath;
        }

        /// <summary>
        /// 获取存放在外部的StreamingAssets路径
        /// </summary>
        /// <param name="buildInfo"></param>
        /// <returns></returns>
        public static string GetOEMStreamingAssetsDataPath(SOBuildInfo buildInfo = null)
        {
            if (!buildInfo)
                buildInfo = SOBuildInfoManager.Instance.curBuildInfo;
            return PathDefinition.dataFolderPath + "/StreamingAssets/" + GetMiddleOEMFolder(buildInfo) + buildInfo.streamingAssetsFolderName;//在 Assets同级目录/Data/StreamingAssets/ 下，按公司名存放
        }

        /// <summary>
        /// 备份StreamAssets到外部目录
        /// </summary>
        /// <param name="buildInfo"></param>
        public static void BackUpStreamAssets(SOBuildInfo buildInfo)
        {
            CopyStreamingAssetsFilesFunc(Application.dataPath + "/StreamingAssets/", GetOEMStreamingAssetsDataPath(buildInfo));

        }

        /// <summary>
        /// 从外部复制StreamingAssets资源到Unity中，为随后的打包做准备
        /// </summary>
        /// <param name="buildInfo"></param>
        public static void CopyStreamingAssets(SOBuildInfo buildInfo)
        {
            if (buildInfo.isReplaceStreamingAsset)//删除已有的文件
            {
                PathTool.DirectoryDelete(Application.dataPath + "/StreamingAssets");
            }

            CopyStreamingAssetsFilesFunc(GetOEMStreamingAssetsDataPath(buildInfo), Application.dataPath + "/StreamingAssets");
        }

        /// <summary>
        /// 注意！只拷贝文件夹，这是为了避免删除其他文件夹
        /// </summary>
        /// <param name="sourceFolderPath"></param>
        /// <param name="targetFolderPath"></param>
        private static void CopyStreamingAssetsFilesFunc(string sourceFolderPath, string targetFolderPath)
        {
            try
            {
                //Todo：也要拷贝文件
                if (Directory.Exists(sourceFolderPath))
                {
                    foreach (string folderPath in Directory.GetDirectories(sourceFolderPath))
                    {
                        string folderName = new DirectoryInfo(folderPath).Name;
                        string taregetFolderPath = targetFolderPath + "/" + folderName;
                        PathTool.DirectoryCopy(folderPath, taregetFolderPath);
                    }
                }
                else
                {
                    Debug.LogError("找不到指定目录!\r\n" + sourceFolderPath);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("拷贝StreamingAssets数据失败：\r\n" + e);
            }

            AssetDatabase.Refresh();

            Debug.Log("Copy StreamAssets Finish");
        }

        /// <summary>
        /// BuildTargetGroup.Standalone;
        /// </summary>
        static BuildTargetGroup targetGroup { get { return EditorUserBuildSettings.selectedBuildTargetGroup; } }

        /// <summary>
        /// BuildTarget.StandaloneWindows64;//SteamVR插件 推荐64位
        /// </summary>
        static BuildTarget buildTarget { get { return EditorUserBuildSettings.activeBuildTarget; } }

        /// <summary>
        /// 设置Building Settings 中的场景
        /// </summary>
        /// <param name="buildInfo"></param>
        /// <returns></returns>
        public static List<string> SetBuildingSettingScene(SOBuildInfo buildInfo)
        {
            List<string> listBuildScene = new List<string>();

            List<EditorBuildSettingsScene> listEditorScene = EditorBuildSettings.scenes.ToList();
            listEditorScene.ForEach((ebss => ebss.enabled = false));
            foreach (SOSceneInfo sceneInfo in buildInfo.AllSceneInfo)
            {
                string sceneName = sceneInfo.buildName;
                EditorBuildSettingsScene targetScene = listEditorScene.Find(
                    (s) =>
                    {
                        string tempSceneName = s.path.GetFileNameWithoutExtension();
                        return tempSceneName == sceneName;
                    });
                if (targetScene != null)
                {
                    Debug.Log("Select Scene: " + sceneName + "\r\n" + targetScene.path);
                    listBuildScene.Add(targetScene.path);
                    targetScene.enabled = true;//勾选指定场景
                }
            }
            if (listBuildScene.Count > 0)
                EditorBuildSettings.scenes = listEditorScene.ToArray();
            else
            {
                Debug.LogError("No Scene Select!");
            }
            return listBuildScene;
        }

        /// <summary>
        /// 设置控制台场景信息
        /// </summary>
        /// <param name="sOConsoleSceneConfig"></param>
        public static void SetConsoleSceneConfig(SOConsoleSceneConfig sOConsoleSceneConfig)
        {
            if (SOConsoleSceneManager.Instance)
            {
                SOConsoleSceneManager.Instance.CurConsoleSceneConfig = sOConsoleSceneConfig;//设置当前的ConsoleSceneConfig
                Debug.Log("设置当前的ConsoleSceneConfig");
            }
        }

        /// <summary>
        /// 设置PlayerSettings 的相关信息
        /// </summary>
        /// <param name="buildInfo"></param>
        public static void SetAsCur(SOBuildInfo buildInfo)
        {
            if (SOBuildInfoManager.Instance)
            {
                SOBuildInfoManager.Instance.CurBuildInfo = buildInfo;//设置当前的SOBuildInfo
                EditorUtility.SetDirty(SOBuildInfoManager.Instance);//！需要调用该方法保存更改
            }

            if (buildInfo.companyName.IsNullOrEmpty())
            {
                Debug.LogError("BuildInfo中公司名为空");
                return;
            }

            //设置Icon (Ps:会覆盖 PlayerSettings里的Default Icon，需要在Icon栏目下勾选（override for PC，Mac……）)
            Texture2D[] sourceIcon = PlayerSettings.GetIconsForTargetGroup(targetGroup, IconKind.Any);
            List<Texture2D> listIcon = new List<Texture2D>();
            for (int i = 0; i != sourceIcon.Length; i++)
                listIcon.Add(buildInfo.logo);
            PlayerSettings.SetIconsForTargetGroup(targetGroup, listIcon.ToArray(), IconKind.Any);
            PlayerSettings.SetApplicationIdentifier(targetGroup, buildInfo.applicationIdentifier);
            //PlayerSettings.applicationIdentifier = buildInfo.applicationIdentifier;
            PlayerSettings.companyName = buildInfo.companyName.ToString();
            PlayerSettings.productName = buildInfo.appName;//标题名
        }

        public static void BulidTarget(SOBuildInfo buildInfo, bool isAutoRunPlayer = true)
        {
            Debug.Log("准备打包: " + buildInfo.name);
            curBuildInfo = buildInfo;

            //查找并设置场景
            List<string> listBuildScene = SetBuildingSettingScene(buildInfo);

            if (listBuildScene.Count == 0)
            {
                Debug.LogError("空场景");
                return;
            }
            if (listBuildScene.Count != buildInfo.AllSceneInfo.Count)
            {
                Debug.LogError("请把缺少的场景添加到Building Setting界面中");
                return;
            }

            //创建相关打包文件夹
            string folderName = GetBuildFolderPath(buildInfo);//程序文件夹 -- (Build Group/Colyu/Classroom_Earthquake）
            if (Directory.Exists(folderName))
            {
                //如果名称不一样，每次build删除之前的残留
                //if (File.Exists(target_name))
                //{
                //    File.Delete(target_name);
                //}
            }
            else
            {
                Directory.CreateDirectory(folderName);//创建所需的文件夹
            }

            if (buildInfo.sOConsoleSceneConfig)
                SetConsoleSceneConfig(buildInfo.sOConsoleSceneConfig);//设置ConsoleSceneConfig信息

            SetAsCur(buildInfo);//设置ProjectingSetting中的信息

            ModifyScene(buildInfo);//更改场景的引用（需要先调用SetPlayerSettingInfo）

            PlatfromSetting();

            if (buildInfo.isCopyStreamingAsset)
                CopyStreamingAssets(buildInfo);

            //Todo:从当前设置中获取
            BuildOptions buildOptions = BuildOptions.None;
            if (isAutoRunPlayer)
                buildOptions = BuildOptions.AutoRunPlayer;

            string buildPath = folderName + "/" + GetBuildFileNameWithExtension(buildInfo);
            //开始Build场景
            GenericBuild(listBuildScene.ToArray(), buildPath, targetGroup, buildTarget, buildOptions);

            CopyExtraFiles(curBuildInfo);

            //只选择Build后，打开文件夹
            if (buildInfo.isOpenFolderAfterBuild && !isAutoRunPlayer)
            {
                PathTool.OpenFolder(buildPath);
            }

            PlatfromResetAfterBuild();

            OnAfterBuild(buildInfo);
        }

        static bool cacheActivateWaveVRModuleState;
        /// <summary>
        /// 临时设置打包设置
        /// </summary>
        private static void PlatfromSetting()
        {
            //#if USE_VIU
            //        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)//自动设置
            //        {
            //            cacheActivateWaveVRModuleState = HTC.UnityPlugin.Vive.VIUSettings.activateWaveVRModule;
            //            if (HTC.UnityPlugin.Vive.VIUSettings.activateWaveVRModule == false)
            //            {
            //                HTC.UnityPlugin.Vive.VIUSettings.activateWaveVRModule = true;
            //                Debug.Log("临时激活VIUSettings中的WaveVRModule");
            //            }
            //        }
            //#endif
        }
        /// <summary>
        /// 还原打包设置
        /// </summary>
        private static void PlatfromResetAfterBuild()
        {
            //Bug：不会复原
            //#if USE_VIU
            //        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)//自动设置
            //        {
            //            HTC.UnityPlugin.Vive.VIUSettings.activateWaveVRModule = cacheActivateWaveVRModuleState;
            //        }
            //#endif
        }


        /// <summary>
        /// 设置场景中的引用
        /// </summary>
        /// <param name="buildInfo"></param>
        public static void ModifyScene(SOBuildInfo buildInfo)
        {
            //修改场景
            foreach (SOSceneModifierBase sOSceneModifier in buildInfo.listSceneModifier)
            {
                sOSceneModifier.OnBeforeBuild();
            }
        }

        private static void OnAfterBuild(SOBuildInfo buildInfo)
        {
            //修改场景
            foreach (SOSceneModifierBase sOSceneModifier in buildInfo.listSceneModifier)
            {
                sOSceneModifier.OnBeforeBuild();
            }
        }


        public static string GetBuildFilePath(SOBuildInfo buildInfo = null)
        {
            if (!buildInfo)
                buildInfo = SOBuildInfoManager.Instance.CurBuildInfo;
            return GetBuildFolderPath(buildInfo) + "/" + GetBuildFileNameWithExtension(buildInfo);
        }

        public static string GetBuildFileNameWithExtension(SOBuildInfo buildInfo = null)
        {
            if (!buildInfo)
                buildInfo = SOBuildInfoManager.Instance.CurBuildInfo;
            string target_name = buildInfo.appName;
            switch (targetGroup)
            {
                case BuildTargetGroup.Standalone:
                    target_name += ".exe";//程序文件名
                    break;
                case BuildTargetGroup.Android:
                    target_name += ".apk";//程序文件名
                    break;
                default:
                    Debug.LogError("还未处理！");
                    break;
            }
            return target_name;
        }
        public static string GetBuildFolderPath(SOBuildInfo buildInfo = null)
        {
            if (!buildInfo)
                buildInfo = SOBuildInfoManager.Instance.CurBuildInfo;
            string groupFolderName = Application.dataPath.Replace("/Assets", "") + "/Build Group";//程序的父文件夹
            return groupFolderName + "/" + GetMiddleOEMFolder(buildInfo) + buildInfo.folderName; ;//程序文件夹 -- (Build Group/Colyu/Classroom_Earthquake）
        }

        /// <summary>
        /// 找到激活的场景
        /// </summary>
        /// <returns></returns>
        private static List<EditorBuildSettingsScene> FindEditorScenes()
        {
            List<EditorBuildSettingsScene> EditorScenes = new List<EditorBuildSettingsScene>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                EditorScenes.Add(scene);
            }
            return EditorScenes;
        }
        /// <summary>
        /// 找到所有场景
        /// </summary>
        /// <returns></returns>
        private static string[] FindEnabledEditorScenes()
        {
            List<string> EditorScenes = new List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (!scene.enabled) continue;
                EditorScenes.Add(scene.path);
            }
            return EditorScenes.ToArray();
        }



        static bool GenericBuild(string[] scenes, string target_dir, BuildTargetGroup targetGroup, BuildTarget build_target, BuildOptions build_options)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(targetGroup, build_target);
#if UNITY_2018_1_OR_NEWER
            BuildReport buildReport = BuildPipeline.BuildPlayer(scenes, target_dir, build_target, build_options);
            string res = "";
            if (buildReport.summary.result == BuildResult.Failed)
                res = "Failed";
#else
        string res = BuildPipeline.BuildPlayer(scenes, target_dir, build_target, build_options);
#endif

            if (res.Length > 0)
            {
                string strScenes = "";
                foreach (var scene in scenes)
                {
                    strScenes += scene + " ";
                }
                Debug.LogError("BuildPlayer failure: " + res + "\r\n" + strScenes);
                return false;
            }
            Debug.Log("Build Finish:\r\n" + target_dir);
            return true;
        }

        [MenuItem(EditorDefinition.TopMenuItemPrefix + "安装apk")]
        public static void InstallApk()
        {
            try
            {
                System.Diagnostics.Process myProcess = new System.Diagnostics.Process();
                myProcess.StartInfo.FileName = "C:\\Windows\\system32\\cmd.exe";
                string apkFullPathName = GetBuildFilePath();
                //System.IO.File.Copy()
                myProcess.StartInfo.Arguments = "/c adb install -r -d " + apkFullPathName;
                myProcess.Start();
                myProcess.WaitForExit();
                int ExitCode = myProcess.ExitCode;
                UnityEngine.Debug.Log("Install Simulator apk is done : " + ExitCode);
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.Log("安装失败: " + e);
            }
        }
    }
}
#endif

