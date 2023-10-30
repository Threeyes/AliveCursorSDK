using UnityEngine;
using UnityEngine.SceneManagement;

namespace Threeyes.Steamworks
{
    #region 以Hub前缀，定义通用的接口及Handler，包含各Manager的核心字段/方法/回调

    public interface IHubManagerModPreInitHandler
    {
        /// <summary>
        /// Get call before PersistentData is Loaded
        /// </summary>
        void OnModPreInit(Scene scene, ModEntry modEntry);
    }

    /// <summary>
    /// Manager Init Mod
    /// </summary>
    public interface IHubManagerModInitHandler
    {
        /// <summary>
        /// Get call right after PersistentData is Loaded
        /// </summary>
        void OnModInit(Scene scene, ModEntry modEntry);
        /// <summary>
        /// Get call right before PersistentData is Saved
        /// </summary>
        void OnModDeinit(Scene scene, ModEntry modEntry);
    }

    public interface IHubManagerWithController<TControllerInterface>
    {
        TControllerInterface ActiveController { get; }
    }

    //——SettingManager——
    public interface IHubSettingManager
    {
        bool HasInit { get; }
    }

    //——SystemManager——
    public interface IHubProgramActiveHandler
    {
        /// <summary>
        /// The activation status of the program. Should reduce performance consumption on deactive state
        /// </summary>
        /// <param name="isActive"></param>
        void OnProgramActiveChanged(bool isActive);
    }
    public interface IHubSystemWindow_ChangedHandler
    {
        /// <summary>
        /// Called before and after screen switching/resolution change
        /// 
        /// PS:Convert to real type if you need extra info
        /// </summary>
        /// <param name="e"></param>
        public void OnWindowChanged(WindowEventExtArgs e);
    }
    public interface IHubSystemWindow_ChangeCompletedHandler
    {
        /// <summary>
        /// Called after screen switching/resolution change
        /// </summary>
        public void OnWindowChangeCompleted();
    }

    public interface IHubSystemAudioManager
    {
        int RawSampleCount { get; }
        int FFTCount { get; }
        int SpectrumCount { get; }
    }
    public interface IHubSystemAudio_RawSampleDataChangedHandler
    {
        void OnRawSampleDataChanged(float[] rawSampleData);
    }
    public interface IHubSystemAudio_FFTDataChangedHandler
    {
        void OnFFTDataChanged(float[] fftData);
    }
    public interface IHubSystemAudio_SpectrumDataChangedHandler
    {
        void OnSpectrumDataChanged(float[] spectrumData);
    }

    //——ModManager——
    public interface IHubEnvironmentManager
    {
        /// <summary>
        /// The main camera
        /// </summary>
        Camera MainCamera { get; }
    }
    public interface IHubSceneManager
    {
        Scene HubScene { get; }
        public Scene CurModScene { get; }
    }
    #endregion

    #region 以下接口继承了必要的父接口，方便具体Manager直接继承

    //——System——
    public interface ISystemAudioManager : IHubSystemAudioManager, IHubManagerModInitHandler
    {
    }

    //——Mod——
    public interface IEnvironmentManager<TControllerInterface> :
    IHubManagerWithController<TControllerInterface>
    , IHubEnvironmentManager
    , IHubManagerModInitHandler
        where TControllerInterface : IEnvironmentController
    {
    }
    public interface IPostProcessingManager<TControllerInterface> : IHubManagerModInitHandler, IHubManagerWithController<TControllerInterface>
        where TControllerInterface : IPostProcessingController
    {
    }
    #endregion
}