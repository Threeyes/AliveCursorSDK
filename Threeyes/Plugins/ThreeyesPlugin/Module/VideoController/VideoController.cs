using UnityEngine;
using UnityEngine.Video;
/// <summary>
/// 控制视频播放，封装常用属性
/// </summary>
[RequireComponent(typeof(VideoPlayer))]
public class VideoController : ComponentHelperBase<VideoPlayer>
{
    /// <summary>
    /// 公开Comp组件
    /// </summary>

    /// <summary>
    /// 视频的播放状态
    /// </summary>
    public bool IsPlaying
    {
        get { return Comp.isPlaying; }
        set
        {
            if (value)
                Comp.Play();
            else
                Comp.Pause();
        }
    }

    /// <summary>
    /// 视频播放进度
    /// </summary>
    public float VideoPercent
    {
        get
        {
            return Comp.frameCount > 0 ? (float)Comp.frame / Comp.frameCount : 0;//计算当前的播放进度
        }
        set
        {
            Comp.frame = (long)(Comp.frameCount * value);//设置当前播放的帧序号
        }
    }

    /// <summary>
    /// 视频音量
    /// </summary>
    public float VolumePercent
    {
        get { return Comp.GetDirectAudioVolume(0); }
        set { Comp.SetDirectAudioVolume(0, value); }
    }

    /// <summary>
    /// 已播放时长
    /// </summary>
    public double CurTime
    {
        get { return (double)Comp.time; }
        set { Comp.time = value; }
    }

    /// <summary>
    /// 视频总时长
    /// </summary>
    /// <summary>
    /// 视频总时长
    /// </summary>
    public double TotalTime
    {
        get
        {
            switch (Comp.source)
            {
                case VideoSource.VideoClip:
                    return Comp.clip ? Comp.clip.length : 0;
                case VideoSource.Url:
                    return Comp.frameRate > 0 ? Comp.frameCount / Comp.frameRate : 0;//Url不能获取总时间，如果小于0的话，DateTime报错
            }
            return 0;
        }
    }
}
