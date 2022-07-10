using UnityEngine;
using UnityEngine.Events;

public class ParticleSystemHelper : ComponentHelperBase<ParticleSystem>
{
    public BoolEvent onPlayStop;
    public UnityEvent onPlay;
    public UnityEvent onStop;

    public void Play(bool isPlay)
    {
        onPlayStop.Invoke(isPlay);
        if (isPlay)
        {
            Comp.Play();
            onPlay.Invoke();
        }
        else
        {
            Comp.Stop();
            onStop.Invoke();
        }
    }

    public void SetStartSizeMultiplier(float percent)
    {
        var module = GetMainModule();
        module.startSizeMultiplier = percent;
    }

    public void SetStartLifeTimeMultiplier(float percent)
    {
        var module = GetMainModule();
        module.startLifetimeMultiplier = percent;
    }

    public void SetStartColor(Color color)
    {
        var module = GetMainModule();
        module.startColor = color;
    }
    ParticleSystem.MainModule GetMainModule()
    {
        return Comp.main;
    }

    #region Editor Method

#if UNITY_EDITOR

    public void SetLoop(bool isOn)
    {
        var module = GetMainModule();
        module.loop = isOn;
    }

#endif

    #endregion

}
