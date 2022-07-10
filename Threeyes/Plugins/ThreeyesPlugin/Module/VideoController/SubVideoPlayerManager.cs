using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
/// <summary>
/// 分步播放分段视频(Todo:用VideoEP+EPS代替）
/// </summary>
public class SubVideoPlayerManager : SequenceBase<VideoClipData>
{
    public static SubVideoPlayerManager Instance;
    public VideoController videoController;//VideoController组件引用


    public VideoClip videoClip;
    public SOSubVideoClipInfoGroup sOVideoStepClipInfo;
    public override List<VideoClipData> ListData
    {
        get
        {
            return sOVideoStepClipInfo.listData;
        }

        set
        {
            base.ListData = value;
        }
    }

    private void Awake()
    {
        Instance = this;
    }
    private void Update()
    {
        if (curIndex < 0)
            return;

        VideoClipData videoClipData = ListData[curIndex];

        if (videoClipData.IsNull())
            return;

        if (delayCheckTime > 0)
            delayCheckTime -= Time.deltaTime;
        else
        {
            if (videoController.CurTime > videoClipData.endTime.ToSeconds())//暂停播放
            {
                videoController.IsPlaying = false;
            }
        }


        if (Input.GetKeyDown(KeyCode.A))
        {
            SetPrevious();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            SetNext();
        }

    }
    float delayCheckTime = 0;

    protected override void SetDataFunc(VideoClipData data, int index)
    {
        base.SetDataFunc(data, index);
        if (sOVideoStepClipInfo.videoClip)
        {
            if (videoController.Comp.clip != sOVideoStepClipInfo.videoClip)
                videoController.Comp.clip = sOVideoStepClipInfo.videoClip;
        }

        videoController.CurTime = data.startTime.ToSeconds();
        videoController.IsPlaying = true;
        delayCheckTime = 0.5f;
    }
}