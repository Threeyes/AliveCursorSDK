using UnityEngine;
using UnityEngine.SceneManagement;

namespace Threeyes.Steamworks
{
    //——Mod Common——

    //——Manager Common——

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

    public interface IHubSystemWindow_ChangeCompletedHandler
    {
        /// <summary>
        /// Called after screen switching/resolution change
        /// </summary>
        public void OnWindowChangeCompleted();
    }

    #region SystemAudioManager
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
    #endregion

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
}