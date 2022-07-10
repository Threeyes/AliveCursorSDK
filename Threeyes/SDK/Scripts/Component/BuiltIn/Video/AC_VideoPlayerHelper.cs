using UnityEngine.Video;

public class AC_VideoPlayerHelper : ComponentHelperBase<VideoPlayer>
{
    /// <summary>
    /// Will auto select the correct method
    /// </summary>
    /// <param name="urlOrFilePath"></param>
    public void SetUrlAndPlay(string urlOrFilePath)
    {
        Comp.Stop();

        if (urlOrFilePath.StartsWith("http"))
            SetRemoteUrl(urlOrFilePath);
        else
            SetFileUrl(urlOrFilePath);

        Comp.Play();
    }

    /// <summary>
    /// Set remote url and play
    /// </summary>
    /// <param name="urlPath"></param>
    public void SetRemoteUrl(string urlPath)
    {
        if (urlPath.IsNullOrEmpty())
            return;
        Comp.source = VideoSource.Url;
        Comp.url = PathTool.ConvertToUnityFormat(urlPath);
    }

    /// <summary>
    /// Set local file path and play
    /// </summary>
    /// <param name="filePath"></param>
    public void SetFileUrl(string filePath)
    {
        if (filePath.IsNullOrEmpty())
            return;
        Comp.source = VideoSource.Url;
        Comp.url = PathTool.ConvertToUnityFormat("file://" + filePath);
    }
}