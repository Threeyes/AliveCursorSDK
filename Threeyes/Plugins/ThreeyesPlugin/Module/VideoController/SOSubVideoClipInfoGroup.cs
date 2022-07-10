using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(menuName = "SO/SOSubVideoClipInfoGroup")]
public class SOSubVideoClipInfoGroup : ScriptableObject
{
    public VideoClip videoClip;
    public List<VideoClipData> listData = new List<VideoClipData>();
}
/// <summary>
/// 视频段落
/// </summary>
[Serializable]
public class VideoClipData
{
    public string tips;
    public Vector3 startTime = new Vector3();
    
    public Vector3 endTime = new Vector3();
    public VideoClip videoClip;//如果非空，则覆盖原有的视频源
}

public static class VideoExtensions
{
    public static double ToSeconds(this Vector3 videoTime)
    {
        return videoTime.x * 3600 + videoTime.y * 60 + videoTime.z;
    }

}
