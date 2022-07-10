#if UNITY_EDITOR
#if USE_RTVoice
using Crosstales.RTVoice.Tool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System;
using System.Linq;
/// <summary>
/// PS：调用GenerateAll，生成所有音频文件，并且自动链接到SOTipsInfo上
/// 可能会报Null错误，因为一次性生成太多音频，多运行几次就行
/// 
/// Todo:针对某个音频单独生成，可以是叠加打开该场景，生成后，关闭
/// </summary>
public class GenerateAudio : SingletonBase<GenerateAudio>
{
    public SpeechText speechText;
    public string inputPath = @"Resources\SO Assets\Tips\";//  Resources\SO Assets\Tips\
    public string outputPath = @"_Audios\TTS Group\";//  _Audios\TTS Group\  或  Resources\Audios\TTS Group\

    public static string sceneName = @"Assets\_ThirdPlugins\Colyu Plugin Group\ModuleHelper\RTVoice\Scene\GenerateAudio.unity";


    public static List<SOTipsInfo> listToGenereate = new List<SOTipsInfo>();//指定需要生成的数据，非空表示需要生成
    public static void AutoGenerateSpec()
    {
        foreach (UnityEngine.Object obj in Selection.objects)
        {
            if (obj is SOTipsInfo)
            {
                listToGenereate.Add(obj as SOTipsInfo);
            }
        }
        AutoGenerate();
    }

    [MenuItem(EditorDefinition.TopMenuItemPrefix + "生成提示语音")]
    public static void AutoGenerate()
    {
        EditorSceneManager.sceneOpened += OnSceneOpened;
        Scene scene = EditorSceneManager.OpenScene(sceneName, OpenSceneMode.Additive);
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode openSceneMode)
    {
        Debug.Log("Auto Generate AudioClip");
        try
        {
            GenerateAudio generateAudioInst = null;
        //ps:不能直接调用Instance，因为他会生成一个GenerateAudio物体。
        foreach (var go in scene.GetRootGameObjects())
        {
            Transform tfRoot = go.transform;
            generateAudioInst = tfRoot.FindFirstComponentInChild<GenerateAudio>(true, true);
            if (generateAudioInst)
            {
                generateAudioInst.GenerateAll();//自动生成选中的语音
                break;
            }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("生成语音失败！\r\n" + e);
        }

        EditorSceneManager.CloseScene(scene, true);
        EditorSceneManager.sceneOpened -= OnSceneOpened;
        listToGenereate.Clear();
    }

    public List<SOTipsInfo> listSOTipsInfo = new List<SOTipsInfo>();
    [ContextMenu("GenerateAll")]
    void GenerateAll()
    {
        StartGenerateAll();
        StartGenerateAll();
    }

    void StartGenerateAll()
    {
        if (!speechText)
            speechText = GetComponent<SpeechText>();

        //Init
        listSOTipsInfo = new List<SOTipsInfo>();
        int createdAudio = 0;


        //Todo：改为相对于Tips的目录自动生成音频文件并存放，避免有些tips放在自定义位置

        //参考 http://blog.csdn.net/lfxq0168/article/details/40076269
        //List<Object> listObject = AssetDatabase.LoadAllAssetsAtPath(inputPath).ToList();
        //创建音频文件
        List<UnityFileInfo> listFileInfo = GetAllObject(inputPath);//未筛选的文件
        List<UnityFileInfo> listTargetFileInfo = new List<UnityFileInfo>();//需要生成音频的文件
        foreach (UnityFileInfo fileInfo in listFileInfo)
        {
            SOTipsInfo tips = AssetDatabase.LoadAssetAtPath<SOTipsInfo>(fileInfo.relateUnityPath);
            if (!tips)
                continue;
            if (tips.speechTips.IsNullOrEmpty())//排除没有音频文本的
                continue;

            if (!tips.isAutoGenerateAudio)
                continue;

            if (listToGenereate.Count > 0)//如果指定生成列表不为空，那就只生成这些tips语音
            {
                if (!listToGenereate.Contains(tips))//排除不需要生成的
                    continue;
            }

            //创建音频
            string outputSubFolderPath = fileInfo.relateSonPath.Substring(0, fileInfo.relateSonPath.Length - fileInfo.file.Name.Length);//  ClassRoom Group/BuildingIn Group/ClassRoom/
            string speakerOutFolderPath = outputPath + outputSubFolderPath;//  _Audios\TTS Group\ClassRoom Group/BuildingIn Group/ClassRoom/
            speechText.FilePath = speakerOutFolderPath;
            speechText.Text = tips.speechTips;
            speechText.FileName = fileInfo.fileName;
            speechText.Speak();

            string audioPath = "Assets/" + speakerOutFolderPath + fileInfo.fileName + ".wav";
            fileInfo.audioPath = audioPath;
            fileInfo.sOTipsInfo = tips;
            listTargetFileInfo.Add(fileInfo);
        }
        //PS:生成后需要刷新AssetDatabase，否则第一次链接不上
        //Bug:需要等上一个生成后，再生成下一个，否则会报错
        AssetDatabase.Refresh();

        //等待生成完毕后，再逐个赋值
        foreach (UnityFileInfo fileInfo in listTargetFileInfo)
        {
            SOTipsInfo tips = fileInfo.sOTipsInfo;
            AudioClip audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(fileInfo.audioPath);

            tips.audioClip = audioClip;
            // 通知编辑器有资源被修改了
            EditorUtility.SetDirty(tips);
            // 保存所有修改
            AssetDatabase.SaveAssets();

            listSOTipsInfo.Add(tips);
        }
        AssetDatabase.Refresh();


        //检查有多少音频已经赋值
        foreach (UnityFileInfo fileInfo in listTargetFileInfo)
        {
            if (fileInfo.sOTipsInfo.audioClip)
                createdAudio++;
        }
        Debug.Log("Cretat Sound: " + createdAudio + "/" + listTargetFileInfo.Count);
    }

    /// <summary>
    /// 获取指定目录下的所有文件
    /// </summary>
    /// <param name="relatePath"></param>
    /// <returns></returns>
    List<UnityFileInfo> GetAllObject(string relatePath)
    {
        List<UnityFileInfo> listObj = new List<UnityFileInfo>();
        string mainFolderpath = Application.dataPath;
        mainFolderpath = mainFolderpath + "/" + relatePath;
        //mainFolderpath.Replace(@"\", "/");
        string[] arrStrFolderPath = Directory.GetFiles(mainFolderpath, "*", SearchOption.AllDirectories);

        //循环遍历每一个路径，单独加载(C# 使用/ )
        foreach (string strFirePath in arrStrFolderPath)
        {
            if (strFirePath.EndsWith(".meta"))
                continue;
            UnityFileInfo fileInfo = new UnityFileInfo();
            fileInfo.file = new FileInfo(strFirePath);
            fileInfo.fileName = fileInfo.file.Name.GetFileNameWithoutExtension();
            string strTempPath = strFirePath;
            strTempPath = strTempPath.Replace(@"\", "/");//替换路径中的反斜杠为正斜杠
            relatePath = relatePath.Replace(@"\", "/");
            fileInfo.relateParentPath = relatePath;

            strTempPath = strTempPath.Substring(strTempPath.IndexOf("Assets"));//截取我们需要的路径  Assets/.....
            fileInfo.relateUnityPath = strTempPath;
            string strToCut = "Assets/" + relatePath;
            //print("strToCut: " + strToCut);
            strTempPath = strTempPath.Substring(strToCut.Length);//relatePath内的路径，ClassRoom Group/......
            fileInfo.relateSonPath = strTempPath;
            //print(strTempPath);
            listObj.Add(fileInfo);
        }
        return listObj;
    }

    [System.Serializable]
    public class UnityFileInfo
    {
        public SOTipsInfo sOTipsInfo;
        public FileInfo file;
        public string fileName;
        /// <summary>
        /// 相对于项目文件夹的路径，用于LoadAssetAtPath。 
        /// </summary>
        public string relateUnityPath;//  Assets/Resources/SO Assets/ClassRoom Group/BuildingIn Group/ClassRoom/Find Bag.asset

        /// <summary>
        /// 父文件夹的路径
        /// </summary>
        public string relateParentPath;//  Resources/SO Assets/

        /// <summary>
        /// 相对于父文件夹的路径
        /// </summary>
        public string relateSonPath;//  ClassRoom Group/BuildingIn Group/ClassRoom/Find Bag.asset


        public string audioPath;
    }
}
#endif
#endif