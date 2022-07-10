#if UNITY_2020_1_OR_NEWER
#define NewVersion
#else//旧版本直接激活
#define Active
#endif
#if NewVersion && USE_Timeline
#define Active
#endif
#if Active
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0f, 1f, 0f)]
[TrackClipType(typeof(UITipsClip))]
[TrackBindingType(typeof(UITips))]
public class UITipsTrack : TrackBase<UITipsTrack, UITipsBehaviour, UITipsMixerBehaviour, UITipsClip, UITips>
{
    protected override void InitClip(TimelineClip timelineClip, UITipsClip clip, PlayableGraph graph, GameObject go, int inputCount)
    {
        UITipsClip uiTipsClip = timelineClip.asset as UITipsClip;
        SOTipsInfo soTipsInfo = uiTipsClip.soTipsInfo.Resolve(graph.GetResolver());
        if (soTipsInfo)
        {
            timelineClip.displayName = soTipsInfo.tips;

            //使Clip的长度与音频一致
            if (soTipsInfo && soTipsInfo.AudioClipLength > 0 && uiTipsClip.isLengthBaseOnAudio)
            {
                timelineClip.duration = soTipsInfo.AudioClipLength + 0.1f;
            }
        }
    }

    [ContextMenu("Refresh")]
    public void Refresh()
    {
        ////Todo:更新tips的时长
        ///
        //foreach (var timelineClip in GetClips())
        //{
        //    if (soTipsInfo && soTipsInfo.AudioClipLength > 0 && uiTipsClip.isLengthBaseOnAudio)
        //    {
        //        timelineClip.duration = soTipsInfo.AudioClipLength + 0.1f;
        //    }
        //}
    }
    void RefreshClip(TimelineClip timelineClip, PlayableGraph graph)
    {
        UITipsClip uiTipsClip = timelineClip.asset as UITipsClip;
        SOTipsInfo soTipsInfo = uiTipsClip.soTipsInfo.Resolve(graph.GetResolver());
        if (soTipsInfo)
        {
            timelineClip.displayName = soTipsInfo.tips;

            //使Clip的长度与音频一致
            if (soTipsInfo && soTipsInfo.AudioClipLength > 0 && uiTipsClip.isLengthBaseOnAudio)
            {
                timelineClip.duration = soTipsInfo.AudioClipLength + 0.1f;
            }
        }
    }
}
#endif