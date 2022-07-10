using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
/// <summary>
/// 
/// Bug：
/// Playable Dierctor：
///     Wrap Mode 为Hold时，stopped事件不会被调用，需要手动调用Stop函数，否则还会影响后续的TL
/// </summary>
public class PlayableDirectorHelper : ComponentHelperBase<PlayableDirector>
{

    public float PlaybackSpeed//控制TL播放速度
    {
        get
        {
            return playbackSpeed;
        }

        set
        {
            playbackSpeed = value;
            if (Comp && Comp.playableGraph.IsValid())
                Comp.playableGraph.GetRootPlayable(0).SetSpeed(value);
        }
    }
    private float playbackSpeed = 1;

    public UnityEvent onPlayed;

    public UnityEvent onPaused;
    public UnityEvent onResume;
    public UnityEvent onStopped;
    public BoolEvent onPlayStop;

    /// <summary>
    /// 更新当前帧，适用于在游戏开始时，TL未开始但卡在固定帧，调用该方法可更新其状态
    /// </summary>
    public void Evaluate()
    {
        Comp.Evaluate();
    }

    [ContextMenu("Play")]
    public void Play()
    {
        Play(true);
    }

    public void Play(PlayableAsset playableAsset)
    {
        Play(false);
        Comp.playableAsset = playableAsset;
        Play(true);
    }

    public void Play(bool isPlay)
    {
        InitData();
        if (isPlay)
            Comp.Play();
        else
            Comp.Stop();
    }

    public void RePlay()
    {
        RePlay(true);
    }

    /// <summary>
    /// 播放指定时间
    /// </summary>
    /// <param name="time"></param>
    public void Play(float time)
    {

    }
    /// <summary>
    /// 适用于重置播放状态（可以防止声音提前播放的问题）
    /// </summary>
    /// <param name="isPlay"></param>
    public void RePlay(bool isPlay)
    {
        Play(false);
        Comp.time = 0;
        if (isPlay)
            Play(true);
    }



    public void Pause()
    {
        Pause(true);
    }

    public void Resume()
    {
        Pause(false);
        onResume.Invoke();
    }

    public void Pause(bool isPause)
    {
        //相比调用Comp.Pause，该方法能够避免触发OnBehaviourPause导致EP的Stop被调用，原因见下面：
        //Timeline有个官方bug，调用Pause不可用（https://forum.unity.com/threads/pausing-timeline-playback.473652/）#14
        if (Comp && Comp.playableGraph.IsValid())
            Comp.playableGraph.GetRootPlayable(0).SetSpeed(isPause ? 0 : 1);
    }

    bool isInited = false;

    void InitData()
    {
        if (!Comp)
            return;
        if (isInited)
            return;

#if UNITY_2018_1_OR_NEWER
        Comp.played += OnPlayed;
        Comp.paused += OnPaused;
        Comp.stopped += OnStopped;
#endif

    }

    public void SetInitialTime(float time)
    {
#if UNITY_EDITOR
        Debug.Log("Set TL" + Comp.name + " initialTime to: " + time);
#endif

        Comp.initialTime = time;
    }


    private void OnPlayed(PlayableDirector obj)
    {
        onPlayed.Invoke();
        onPlayStop.Invoke(true);
    }

    private void OnPaused(PlayableDirector obj)
    {
        onPaused.Invoke();
    }

    void OnStopped(PlayableDirector obj)
    {
        onStopped.Invoke();
        onPlayStop.Invoke(false);
    }

}
