using System.Collections;
using System.Collections.Generic;
using Threeyes.Core;
using UnityEngine;
using UnityEngine.Video;

namespace Threeyes.ModuleHelper
{
    /// <summary>
    /// 
    /// 
    /// Warning:
    /// 以下字段只有在VideoClip设置了【视频文件】后才会有效或出现（可能是为了检测视频是否为双声道）
    /// -VideoPlayer的AudioSource字段。解决办法：此时可以进Debug模式，直接设置TargetAudioSources字段
    /// -audioTrackCount不设置时为0。解决办法：统一使用AudioSource，并尝试获取该物体身上的AudioSource
    /// </summary>
    public class VideoPlayerHelper : ComponentHelperBase<VideoPlayer>
    {
        #region Init
        /// <summary>
        /// Will auto select the correct method
        /// </summary>
        /// <param name="urlOrFilePath"></param>
        public void SetUrlAndPlay(string urlOrFilePath)
        {
            Comp.Stop();

            if (urlOrFilePath.StartsWith("http"))
                SetRemoteUrl(urlOrFilePath);
            else
                SetFileUrl(urlOrFilePath);

            Comp.Play();
        }

        /// <summary>
        /// Set remote url and play
        /// </summary>
        /// <param name="urlPath"></param>
        public void SetRemoteUrl(string urlPath)
        {
            if (urlPath.IsNullOrEmpty())
                return;
            Comp.source = VideoSource.Url;
            Comp.url = PathTool.ConvertToUnityFormat(urlPath);
        }

        /// <summary>
        /// Set local file path and play
        /// </summary>
        /// <param name="filePath"></param>
        public void SetFileUrl(string filePath)
        {
            if (filePath.IsNullOrEmpty())
                return;
            Comp.source = VideoSource.Url;//不管是否为本地文件，只要是外部数据都需要设置为Url
            Comp.url = PathTool.ConvertToUnityFormat("file://" + filePath);
        }


        bool hasVideoPlayerInit = false;//Has the VideoPlayer Event init?
        void TryInitData()
        {
            if (!Application.isPlaying)
                return;

            if (hasVideoPlayerInit)
                return;

            if (Comp)
            {
                Comp.loopPointReached += LoopPointReached;//Invoked when the VideoPlayer reaches the end of the content to play.(Bug: loopPointReached delegate for non-looping video wasn't triggered)
                Comp.prepareCompleted += PrepareCompleted;//Invoked when the VideoPlayer preparation is complete.

                Comp.seekCompleted += SeekCompleted;//Invoke after a seek operation completes.

                Comp.frameReady += FrameReady;//Invoked when a new frame is ready.
            }
            hasVideoPlayerInit = true;
        }
        protected virtual void LoopPointReached(VideoPlayer source)
        {
        }
        protected virtual void PrepareCompleted(VideoPlayer source)
        {
            //加载完成后，检查是否有需要初始化的配置
            if (cacheVideoInitSetting != null)
            {
                SetMuteFunc(cacheVideoInitSetting.isMute);
                cacheVideoInitSetting = null;//重置
            }
        }

        protected virtual void SeekCompleted(VideoPlayer source)
        {
        }
        protected virtual void FrameReady(VideoPlayer source, long frameIdx)
        {
        }
        #endregion

        #region Control
        public void TogglePlayPause()
        {
            if (Comp.isPlaying)
                Pause();
            else
                Play();
        }

        public void Play()
        {
            if (!Application.isPlaying)
                return;

            TryInitData();

            if (Comp)
                Comp.Play();
        }
        public void Stop()
        {
            if (!Application.isPlaying)
                return;
            if (Comp)
                Comp.Stop();
        }
        public void Pause()
        {
            if (!Application.isPlaying)
                return;
            if (Comp)
                Comp.Pause();
        }


        public bool IsMute
        {
            get
            {
                switch (Comp.audioOutputMode)
                {
                    case VideoAudioOutputMode.None:
                        return true;
                    case VideoAudioOutputMode.AudioSource:
                        for (ushort i = 0; i != Comp.audioTrackCount; i++)
                        {
                            AudioSource audioSource = Comp.GetTargetAudioSource(i);
                            if (audioSource)
                                return audioSource.mute;
                        }
                        return true;//如果没有设置AudioSource，当作Mute
                    case VideoAudioOutputMode.Direct:
                        bool isMute = true;
                        for (ushort i = 0; i != Comp.audioTrackCount; i++)
                        {
                            //只要任意Track的音频不是Mute，都算作非Mute
                            if (!Comp.GetDirectAudioMute(i))
                                isMute = false;
                        }
                        return isMute;
                    case VideoAudioOutputMode.APIOnly:
                        //ToAdd：该选项比较罕见，又底层管理
                        return false;
                    default:
                        return true;
                }
            }
        }

        public void ToggleMute()
        {
            SetMute(!IsMute);
        }
        public void Mute()
        {
            SetMute(true);
        }
        public void Unmute()
        {
            SetMute(false);
        }

        VideoSetting cacheVideoInitSetting;//缓存的Video设置，延迟到视频加载后使用，使用完成后要置空
        public void SetMute(bool isMute)
        {
            ///Warning:
            ///-如果当前没有加载视频源，那么Comp.audioTrackCount就会返回0。解决办法是将配置缓存下来，等视频加载后调用PrepareCompleted事件，再基于cacheVideoInitSetting进行初始化
            bool isPrepared = Comp.isPrepared;
            if (!isPrepared)
            {
                //检查是否为空，避免其他方法也使用该实例来存储设置
                if (cacheVideoInitSetting == null)
                {
                    cacheVideoInitSetting = new VideoSetting();
                }
                cacheVideoInitSetting.isMute = isMute;
                return;
            }
            SetMuteFunc(isMute);
        }

        private void SetMuteFunc(bool isMute)
        {
            switch (Comp.audioOutputMode)
            {
                case VideoAudioOutputMode.None:
                    return;
                case VideoAudioOutputMode.AudioSource:
                    for (ushort i = 0; i != Comp.audioTrackCount; i++)
                    {
                        AudioSource audioSource = Comp.GetTargetAudioSource(i);
                        if (audioSource)
                            audioSource.mute = isMute;
                    }
                    break;
                case VideoAudioOutputMode.Direct:
                    for (ushort i = 0; i != Comp.audioTrackCount; i++)
                    {
                        Comp.SetDirectAudioMute(i, isMute);
                    }
                    break;
                case VideoAudioOutputMode.APIOnly:
                    //暂不处理
                    return;
            }
        }

        public class VideoSetting
        {
            public bool isMute;
        }
        #endregion
    }
}