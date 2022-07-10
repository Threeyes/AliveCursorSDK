using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
/// <summary>
/// 设置VideoPlayer的属性，可以配合VideoPlayerHelper-onInit使用（为了避免初始化的问题，建议在Inspector里引用）
/// </summary>
public class VideoPlayerInfoSetter : ComponentHelperBase<VideoPlayer>
{

    public ExternalFileLocation externalFileLocation = ExternalFileLocation.StreamingAsset;
    public string filePath = "Video/a.mp4";//在指定目录下的位置

    public void SetInfo()
    {
        Comp.url = PathTool.GetPath(externalFileLocation, filePath);

        ////(ToDelete)对应路径：https://www.cnblogs.com/vsirWaiter/p/5340284.html
        //switch (externalFileLocation)
        //{
        //    case ExternalFileLocation.CustomPath://完全自定义的路径
        //        if (Application.platform == RuntimePlatform.Android)
        //            Comp.url = "file://";
        //        Comp.url += filePath;
        //        break;
        //    case ExternalFileLocation.StreamingAsset:
        //        //PC/Android 通用路径
        //        //Android= jar:file:///data/app/xxx.xxx.xxx.apk/!/assets
        //        Comp.url = Application.streamingAssetsPath + "/" + filePath;
        //        break;
        //    case ExternalFileLocation.AndroidRoot:
        //        Comp.url = "file://" + PathTool.GetAndroidRootPath() + filePath;
        //        break;
        //    case ExternalFileLocation.PersistentDataPath:
        //        //Android注意：
        //        //-本地持久化的目录，不是安卓apk包里面的东西，所以不需要加jar:
        //        //-Play on awake defaults to true. Set it to false to avoid the url set below to auto-start playback since we're in Start().
        //        if (Application.platform == RuntimePlatform.Android)
        //            Comp.url = "file://";
        //        Comp.url += Application.persistentDataPath + "/" + filePath;
        //        break;

        //    case ExternalFileLocation.CustomData:
        //        if (Application.platform == RuntimePlatform.Android)
        //            Comp.url = "file://";
        //        Comp.url += PathDefinition.dataFolderPath + "/" + filePath;
        //        break;
        //}
        Debug.Log("Video FilePath: " + Comp.url);
    }

    /// <summary>
    /// 清除RenderTexture残余内容
    /// </summary>
    public void Clear()
    {
        //Todo
    }
}
