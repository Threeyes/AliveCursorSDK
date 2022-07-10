using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemHelperGroup : ComponentGroupBase<ParticleSystemHelper>
{
    public void Play(bool isPlay)
    {
        ForEachChildComponent((c) => c.Play(isPlay));
    }

    public void SetStartSizeMultiplier(float percent)
    {
        ForEachChildComponent((c) => c.SetStartSizeMultiplier(percent));
    }

    public void SetStartLifeTimeMultiplier(float percent)
    {
        ForEachChildComponent((c) => c.SetStartLifeTimeMultiplier(percent));
    }

    #region Editor Method

#if UNITY_EDITOR

    [ContextMenu("SetLoopOff")]
    public void SetLoopOff()
    {
        ForEachChildComponent((c) => c.SetLoop(false));
    }

#endif

    #endregion

}
