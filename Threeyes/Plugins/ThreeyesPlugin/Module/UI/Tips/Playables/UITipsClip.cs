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
public class UITipsClip : ClipBase<UITipsTrack, UITipsBehaviour, UITips>
{
    public ExposedReference<SOTipsInfo> soTipsInfo;
    public bool isLengthBaseOnAudio = true;//基于音频设置总长度
    public bool isHideOnPause = true;

    public override void InitClone(UITipsBehaviour clone, PlayableGraph graph, GameObject owner)
    {
        clone.soTipsInfo = soTipsInfo.Resolve(graph.GetResolver());
        clone.isLengthBaseOnAudio = isLengthBaseOnAudio;
        clone.isHideOnPause = isHideOnPause;
    }
}
#endif