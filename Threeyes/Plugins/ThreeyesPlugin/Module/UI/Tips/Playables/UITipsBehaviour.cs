#if UNITY_2020_1_OR_NEWER
#define NewVersion
#else//旧版本直接激活
#define Active
#endif
#if NewVersion && USE_Timeline
#define Active
#endif
#if Active
using System;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class UITipsBehaviour : BehaviourBase<UITips>
{
    public SOTipsInfo soTipsInfo;
    public bool isLengthBaseOnAudio = true;//基于音频设置总长度
    public bool isHideOnPause = true;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (!trackBinding)
            return;

        if (!soTipsInfo)
            return;

        trackBinding.SetTipsAndShow(soTipsInfo);
    }


    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (!trackBinding)
            return;

        //Prevent get call on Tineline Start https://forum.unity.com/threads/release-event-player-visual-play-and-organize-unityevent.536984/#post-3605916
        double time = playable.GetGraph().GetRootPlayable(0).GetTime();
        if (time > 0)
        {
            if (Application.isPlaying)//避免在编辑器时提前隐藏导致出错
            {
                //Todo：当Clip重叠时，不调用Hide
                //注意：info.weight一直为0
                if (isHideOnPause)
                    trackBinding.Hide();
            }
        }
    }
}
#endif