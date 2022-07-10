using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemShakeExecuteListener : ShakeExecuteListener
{
    ParticleSystem pS;
    public AudioSource audioSource;

    public bool isAutoPlay = true;
    public bool isAutoStop = true;
    private void Awake()
    {
        pS = GetComponent<ParticleSystem>();
        if (!audioSource)
            audioSource = GetComponent<AudioSource>();
    }

    protected override void OnShakeIncrease()
    {
        base.OnShakeIncrease();
        if (isAutoPlay)
            Play();
    }
    protected override void OnShakeDecrease()
    {
        base.OnShakeDecrease();
        if (isAutoStop)
            Stop();
    }

    void Play()
    {
        if (!pS.isPlaying)
        {
            pS.Play();
            PlayAudio(true);
        }
    }

    void Stop()
    {
        if (!pS.isStopped)
        {
            pS.Stop();
            PlayAudio(false);
        }
    }

    void PlayAudio(bool isPlay)
    {
        if (audioSource)
        {
            if (isPlay)
                audioSource.Play();
            else
                audioSource.Stop();
        }
    }

    [ContextMenu("Copy and Delete")]
    void Do()
    {
        ShakeExecuteListener shakeExecuteListener = GetComponent<ShakeExecuteListener>();
        if (shakeExecuteListener)
        {
            this._executePercent = shakeExecuteListener._executePercent;
            this.randomExecutePercent = shakeExecuteListener.randomExecutePercent;
            DestroyImmediate(shakeExecuteListener);
        }
    }
}
