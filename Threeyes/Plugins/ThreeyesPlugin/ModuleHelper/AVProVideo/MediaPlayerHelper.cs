#if USE_AVProVideo

using RenderHeads.Media.AVProVideo;
using System;
using System.Collections;
using System.Collections.Generic;
using Threeyes.EventPlayer;
using UnityEngine;
namespace Threeyes.EventPlayer
{

    /// <summary>
    /// 功能：
    /// 1.将该组件挂在MediaPlayer物体中，可以监听其更新事件
    /// 2.将该组件挂在对应的按键或EventPlayer，可以自定义其路径及控制视频
    /// Todo:将MediaPlayerHelper继承于该脚本
    /// </summary>
    public class MediaPlayerHelper : EventPlayerForCompWithParamBase<MediaPlayer, MediaPlayerHelper, BoolEvent, bool>
    {
        public bool IsPlaying
        {
            get
            {
                if (Control != null)
                {
                    return Control.IsPlaying() && !Control.IsFinished();
                }
                return false;
            }
        }
        IMediaControl Control { get { return Comp.Control; } }

        //视频路径
        public string strVideoPath;

        public BoolEvent onPlayPause;//播放、暂停
        public FloatEvent onVideoProgress;//视频进度更新（使用方式：调用SliderHelper.SetValueWithoutNotify）
        public StringEvent onVideoTime;//视频时间进度更新（使用方式：设置对应Text）
        public BoolEvent onVideoPlayingState;//视频播放状态更新（使用方式：调用ToggleHelper.ToggleOnWithoutNotify）
        [Header("Runtime Info")]
        [SerializeField]
        protected MediaPlayerEvent.EventType curEventType = MediaPlayerEvent.EventType.PlaylistFinished;

        public bool isCloseOnStop = true;

        #region Public Method

        /// <summary>
        /// 设置路径，然后播放
        /// </summary>
        /// <param name="isPlay"></param>
        public void SetPathThenLoadAndPlay(bool isPlay)
        {
            Comp.m_VideoPath = strVideoPath;
            Play();
        }

        /// <summary>
        /// 加载并播放
        /// </summary>
        /// <param name="isPlay">true：加载并播放；false：停止并关掉视频</param>
        [System.Obsolete("使用Play(ture) instead")]
        public void LoadAndPlay(bool isPlay)
        {
            Play(isPlay);
        }

        public void PlayPause(bool isPlay)
        {
            if (Control != null)
            {
                if (isPlay)
                    Play();
                else
                    PauseFunc();//ToUpdate
            }
        }



        /// <summary>
        /// 通过Slider调用
        /// </summary>
        /// <param name="percent"></param>
        public void SetVideoProgress(float percent)
        {
            if (Control == null)
                return;
            if (Comp.Info == null || Comp.Info.GetDurationMs() == 0)
                return;

            int newTime = (int)(Comp.Info.GetDurationMs() * percent);
            int currentTime = (int)Control.GetCurrentTimeMs();
            if (newTime != currentTime)
                Control.Seek(newTime);
        }



        protected override void PlayFunc()
        {
            if (!Control.HasMetaData())
                Comp.OpenVideoFromFile(Comp.m_VideoLocation, Comp.m_VideoPath, Comp.m_AutoStart);//首次加载
            else
                Comp.Play();

            onPlayPause.Invoke(true);

            base.PlayFunc();
        }
        protected virtual void PauseFunc()
        {
            Control.Pause();
            onPlayPause.Invoke(false);
        }
        protected override void StopFunc()
        {
            if (isCloseOnStop)
                Comp.CloseVideo();
            else
                Comp.Stop();

            base.StopFunc();
        }

        #endregion


        void Start()
        {
            Comp.Events.AddListener(OnMediaPlayerEvent);
        }

        void OnDestroy()
        {
            Comp.Events.RemoveListener(OnMediaPlayerEvent);
        }

        void OnMediaPlayerEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
        {
            curEventType = et;

            switch (et)
            {
                case MediaPlayerEvent.EventType.ReadyToPlay:
                    break;
                case MediaPlayerEvent.EventType.Started:
                    break;
                case MediaPlayerEvent.EventType.FirstFrameReady:
                    break;
                case MediaPlayerEvent.EventType.MetaDataReady:
                    break;
                case MediaPlayerEvent.EventType.FinishedPlaying:
                    break;
                case MediaPlayerEvent.EventType.Closing:
                    break;
            }
        }

        void Update()
        {
            bool isErrorOccur = false;
            if (Control == null)
            {
                //Reset
                isErrorOccur = true;
            }

            if (Comp.Info == null)
            {
                isErrorOccur = true;
            }

            switch (curEventType)
            {
                case MediaPlayerEvent.EventType.StartedSeeking://正在跳转
                    return;
            }

            if (Comp.Info == null || Comp.Info.GetDurationMs() == 0)
            {
                isErrorOccur = true;
            }
            if (!isErrorOccur)
            {
                float curTime = Control.GetCurrentTimeMs();
                float sumTime = Comp.Info.GetDurationMs();
                UpdateVideoProgress(curTime / sumTime);
                UpdateVideoTime(curTime, sumTime);
            }
            else
            {
                UpdateVideoProgress(0);
                UpdateVideoTime(0, 0);
            }
            UpdatePlayingState();
        }

        void UpdatePlayingState()
        {
            onVideoPlayingState.Invoke(IsPlaying);
        }

        void UpdateVideoProgress(float percent)
        {
            onVideoProgress.Invoke(percent);
        }

        void UpdateVideoTime(float curTime, float totalTime)
        {
            string format = @"mm:ss";//定义时间的显示格式，如00:00
            DateTime dateTimeCur = new DateTime().AddMilliseconds(curTime);//当前时间
            DateTime dateTimeTotal = new DateTime().AddMilliseconds(totalTime);//总时间
            onVideoTime.Invoke(dateTimeCur.ToString(format) + " / " + dateTimeTotal.ToString(format));//整体时间显示格式为（当前时长/总时长）
        }

        #region Editor Method
#if UNITY_EDITOR

        public override bool IsCustomInspector
        {
            get
            {
                return false;
            }
        }

#endif
        #endregion
    }
}
#endif