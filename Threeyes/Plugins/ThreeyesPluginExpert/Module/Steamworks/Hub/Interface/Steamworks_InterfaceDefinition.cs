using UnityEngine;
using UnityEngine.SceneManagement;

namespace Threeyes.Steamworks
{
    #region 用于ManagerHolder，包含各Manager的核心字段/方法

    /// <summary>
    /// Manager Init Mod
    /// </summary>
    public interface IHubManagerModInitHandler
    {
        void OnModInit(Scene scene, ModEntry modEntry);
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
        float CalculateLoudness(float[] rawSampleData);
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

    #region 供各Manager继承

    //——SystemManager——
    public interface ISystemAudioManager : IHubSystemAudioManager, IHubManagerModInitHandler
    {
    }

    //——ModManager——
    public interface IEnvironmentManager :
    IHubManagerWithController<IEnvironmentController>
    , IHubEnvironmentManager
    , IHubManagerModInitHandler
    {
    }
    public interface IPostProcessingManager : IHubManagerModInitHandler, IHubManagerWithController<IPostProcessingController>
    {
    }


    #endregion
}