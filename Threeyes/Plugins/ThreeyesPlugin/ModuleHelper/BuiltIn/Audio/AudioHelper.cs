using System.Collections;
using System.Collections.Generic;
using Threeyes.Coroutine;
using UnityEngine;
using UnityEngine.Events;
public class AudioHelper : ComponentHelperBase<AudioSource>
{
    public bool isRandomVolume;
    public Range_Float rangeVolume = new Range_Float(0.5f, 1);
    public bool isRandomPitch;
    public Range_Float rangePitch = new Range_Float(0.8f, 1.2f);

    public List<AudioClip> listClips = new List<AudioClip>();

    public FloatEvent onPlayFinish = new FloatEvent();//回调参数为音频的长度

    public FloatEvent onAllAudioClipPlayFinish = new FloatEvent(); //回调参数为音频的总长度
    #region Public Func

    public bool IsPlaying { get { return Comp.isPlaying; } set { Play(value); } }

    public void SetClip(AudioClip audioClip)
    {
        if (!Comp)
            return;

        Comp.clip = audioClip;
    }

    [ContextMenu("RandomPlay")]
    public void RandomPlay()
    {
        if (listClips.Count > 0)
        {
            PlayFunc(true, listClips.GetRandom());
        }
        else
        {
            Debug.LogError("Null Clips!");
        }
    }

    public void TogglePlay()
    {
        Play(!IsPlaying);
    }

    [ContextMenu("RandomPlay")]
    public void Play()
    {
        Play(true);
    }

    /// <summary>
    /// 暂停播放（注意！未兼容音频完成事件）
    /// </summary>
    /// <param name="isPause"></param>
    public void Pause(bool isPause)
    {
        if (isPause)
            Comp.Pause();
        else
            Comp.UnPause();
    }
    public void Mute(bool isMute)
    {
        Comp.mute = !isMute;
    }
    public void Play(bool isPlay)
    {
        PlayFunc(isPlay);
    }

    public void Play(float volume)
    {
        Comp.volume = volume;
        Play();
    }

    /// <summary>
    /// 播放一系列声音
    /// </summary>
    /// <param name="listClip"></param>
    /// <param name="delay"></param>
    public void Play(List<AudioClip> listClip, float delay = 0, UnityAction actAllAudioPlayFinish = null)
    {
        if (listClip.Count == 0)
            return;

        StartCoroutine(IEPlaySeq(listClip, delay, actAllAudioPlayFinish));
    }


    IEnumerator IEPlaySeq(List<AudioClip> listClip, float delay, UnityAction actAllAudioPlayFinish = null)
    {
        int i = 0;
        float totalLength = 0;
        while (i != listClip.Count)
        {
            float length = listClip[i].length;
            Play(listClip[i]);
            yield return new WaitForSeconds(length + delay);
            totalLength += length + delay;
            i++;
        }
        actAllAudioPlayFinish.Execute();
        onAllAudioClipPlayFinish.Invoke(totalLength);
    }

    public void Play(AudioClip clip)
    {
        Play(clip, null);
    }
    public void Play(AudioClip clip, UnityAction<float> actOnPlayFinish)
    {
        PlayFunc(true, clip, actOnPlayFinish);
    }
    public void Stop()
    {
        PlayFunc(false);
    }

    public void Stop(AudioClip clip)
    {
        PlayFunc(false, clip);
    }

    #endregion

    protected Coroutine cacheEnum;

    UnityAction<float> actionOnPlayAudioFinish;
    void PlayFunc(bool isPlay, AudioClip clip = null, UnityAction<float> tempActionOnPlayAudioFinish = null)
    {
        //1. Stop Cur Clip
        if (!isPlay)
        {
            bool canStop = false;
            if (clip)//停止匹配的声音
            {
                if (Comp.clip == clip)
                    canStop = true;
            }
            else
            {
                canStop = true;
            }
            if (canStop)
            {
                TryStopCoroutine();
                Comp.Stop();
            }
        }

        //2. Set Clip
        if (clip)
            Comp.clip = clip;

        //3.Play
        if (isPlay)
        {
            if (Comp.clip)
            {
                TryStopCoroutine();//会同时调用ResetData，清除监听事件

                actionOnPlayAudioFinish = tempActionOnPlayAudioFinish;
                //if (tempActionOnPlayAudioFinish.NotNull())
                //{
                //    this.onPlayFinish.AddListener(tempActionOnPlayAudioFinish);
                //}

                cacheEnum = CoroutineManager.StartCoroutineEx(IEDelayNotifyFinish(Comp.clip.length));//可能外部也有监听
                PlayAudio();
            }
        }
    }

    #region Inner Func

    void PlayAudio()
    {
        //Init Settting
        if (isRandomVolume)
            Comp.volume = rangeVolume.RandomValue;
        if (isRandomPitch)
            Comp.pitch = rangePitch.RandomValue;

        Comp.Play();
    }

    void StopAudio()
    {
        Comp.Stop();
    }

    #endregion

    protected void TryStopCoroutine()
    {
        if (cacheEnum != null)
            CoroutineManager.StopCoroutineEx(cacheEnum);
        ResetData();
    }

    void ResetData()
    {
        //onPlayFinish.RemoveAllListeners();//（清除所有非持久的监听者）Remove all non-persisent (ie created from script) listeners from the event.
    }

    /// <summary>
    /// 延迟通知音频已经播放完成
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    IEnumerator IEDelayNotifyFinish(float duration)
    {
        yield return new WaitForSeconds(duration);
        onPlayFinish.Invoke(duration);
        actionOnPlayAudioFinish.Execute(duration);
        ResetData();
    }
}
