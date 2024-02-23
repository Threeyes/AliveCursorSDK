using System;
using System.Collections;
using System.Collections.Generic;
using Threeyes.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Threeyes.Steamworks
{
    /// <summary>
    /// ToUpdate:
    /// -提供一个方法，传入枚举（flag，可以组合，默认为人耳可识别区间），可以返回频率区间（bass、lowMid、mid、highMid、treble）
    ///         -参考：https://audiomotion.dev/#/ 的getEnergy方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SystemAudioManagerBase<T> : HubManagerBase<T>
        , ISystemAudioManager
    where T : SystemAudioManagerBase<T>
    {
        #region Interface
        public int RawSampleCount { get { return rawSampleCount; } }
        public int FFTCount { get { return fftSize; } }
        public int SpectrumCount { get { return spectrumCount; } }

        //——以下字段可改为枚举，放到Config中——
        protected virtual int rawSampleCount { get { return 256; } }//源数据(256足够呈现波动，太多会有割裂感）
        protected virtual int fftSize { get { return 256; } }// 【受音量影响】Defines FFT data size constants that can be used for FFT calculations. (Note that only the half of the specified size can be used for visualizations.)   
        protected virtual int spectrumCount { get { return 128; } }//柱状图（针对AC：64已经足够细致，因为光标显示区域不大，不应该过于细分）（可以直接用该数据作为可视化数据）
        #endregion

        #region Property & Field
        ///PS：
        ///1.存储的Data是左/右声的平均值
        protected float[] rawSampleData, identityRawSampleData;//(Data range: [-1, 1])（值为类似Sin函数的Y值。原理：https://answers.unity.com/questions/472188/what-does-getoutputdata-sample-float-represent.html）[PS:不应该暴露rawData，否则容易获取信息]
        protected float[] fftData, identityFftData;//(Data range: [0.0, 1.0])（傅里叶变换）
        protected float[] spectrumData, identitySpectrumData;//(Data range: [0.0, 1.0])（音谱）
        #endregion

        #region Unity Method
        protected virtual void Awake()
        {
            rawSampleData = new float[RawSampleCount];
            identityRawSampleData = new float[RawSampleCount];
            fftData = new float[FFTCount];
            identityFftData = new float[FFTCount];
            spectrumData = new float[SpectrumCount];
            identitySpectrumData = new float[SpectrumCount];
        }
        #endregion

        #region Callback
        public virtual void OnModInit(Scene scene, ModEntry modEntry)
        {
            RemoveMissingHandlers();//清空无效注册实例（如忘了取消注册）
        }
        public virtual void OnModDeinit(Scene scene, ModEntry modEntry)
        {
        }
        #endregion

        #region Register
        static List<IHubSystemAudio_DataChangedHandler> listDataChangedHandler = new List<IHubSystemAudio_DataChangedHandler>();//Cache registed instances
        public void Register(IHubSystemAudio_DataChangedHandler handler)
        {
            listDataChangedHandler.AddOnce(handler);
        }
        public void UnRegister(IHubSystemAudio_DataChangedHandler handler)
        {
            listDataChangedHandler.Remove(handler);
        }

        #endregion

        #region Inner Method
        protected bool isDataLocked = false;

        /// <summary>
        /// Invoke when data changed
        /// </summary>
        protected virtual void OnDataChanged()
        {
            ///PS:
            ///-改为由物体主动监听/取消监听（参考XR）（可以是添加到List<接口>而不是直接监听事件回调），避免物体因为实时创建、RS_GO反序列化导致延迟出现而无法被以下方法获取到

            // Since this is being changed on a seperate thread we do this to be safe
            lock (spectrumData)
            {
                isDataLocked = true;
                FireEvent<IHubSystemAudio_RawSampleDataChangedHandler>((inst) => inst.OnRawSampleDataChanged(rawSampleData));
                FireEvent<IHubSystemAudio_FFTDataChangedHandler>((inst) => inst.OnFFTDataChanged(fftData));
                FireEvent<IHubSystemAudio_SpectrumDataChangedHandler>((inst) => inst.OnSpectrumDataChanged(spectrumData));
                isDataLocked = false;
            }
        }

        /// <summary>
        /// Invoke when data unchanged (eg: no audio input)
        /// 
        /// PS：
        /// -如果没输入要及时通知，否则会卡在最后一次的数据项中
        /// </summary>
        protected virtual void OnDataUnChanged()
        {
            FireEvent<IHubSystemAudio_RawSampleDataChangedHandler>((inst) => inst.OnRawSampleDataChanged(identityRawSampleData));
            FireEvent<IHubSystemAudio_FFTDataChangedHandler>((inst) => inst.OnFFTDataChanged(identityFftData));
            FireEvent<IHubSystemAudio_SpectrumDataChangedHandler>((inst) => inst.OnSpectrumDataChanged(identitySpectrumData));
        }
        #endregion

        #region Utility
        /// <summary>
        /// 移除空引用
        /// 
        /// PS:可以每次加载Mod前调用，或者每隔一段时间调用一次，避免频繁调用
        /// </summary>
        protected static void RemoveMissingHandlers()
        {
            listDataChangedHandler.RemoveAll((d) => d == null);
        }
        protected static void FireEvent<THandler>(Action<THandler> act)
        {
            foreach (IHubSystemAudio_DataChangedHandler handler in listDataChangedHandler)
            {
                if (handler is THandler handlerInstance)
                    act.Invoke(handlerInstance);
            }
        }
        #endregion
    }

    #region Define
    /// <summary>
    /// （人耳可识别）音频频率区间子集
    /// 
    /// PS：
    /// -计算区间方法：
    ///         -计算每个data的大概对应范围=(20000-20)/RawSampleCount。如采样数为256，则每个Data
    ///         对应的值为78hz
    /// 
    /// ToUse:
    /// -限制采样区间，以及返回对应的子集区间内的RawData、FFTData、SpectrumData等
    /// 
    /// Ref：https://www.cuidevices.com/blog/understanding-audio-frequency-range-in-audio-design
    /// </summary>
    [System.Flags]
    public enum AudioFrequencySubset
    {
        None = 0,

        SubBass,//16 to 60 Hz. This is the low musical range - an upright bass, tuba, bass guitar, at the lower end, will fall into this category （PS：下限可以裁剪为20HZ，大约为首个data）
        Bass,//60 to 250 Hz	. In the lower midrange are typical brass instruments, and mid woodwinds, like alto saxophone and the middle range of a clarinet
        LowerMidrange,//250 to 500 Hz	. In the lower midrange are typical brass instruments, and mid woodwinds, like alto saxophone and the middle range of a clarinet
        Midrange,//500 Hz to 2 kHz. The name may be midrange, but it is on the higher end of the fundamental frequencies created by most musical instruments. Here, one can find instruments like the violin and piccolo
        HigherMidrange,//2 to 4 kHz. As mentioned, harmonics are at multiples of the fundamental frequency, so if expecting the fundamentals for a trumpet to be in the lower midrange, one can expect the harmonic to be at 2 times, 3 times, and 4 times that fundamental, which would put them in this range
        Presence,//4 to 6 kHz. Harmonics for the violin and piccolo are found here
        Brilliance,//6 to 20 kHz	. Above 6 kHz is where sounds become more like whines and whistles because they are so high pitched. In this range, sibilant sounds (the unwanted whistle when sometimes pronouncing an ‘s’) and harmonics for certain percussive sounds like cymbals are found

        All = ~0
    }
    #endregion
}
//测试