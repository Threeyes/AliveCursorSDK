using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.GameFramework
{
    public class AudioVisualizerBase : MonoBehaviour
        , IHubSystemAudio_RawSampleDataChangedHandler
        , IHubSystemAudio_FFTDataChangedHandler
        , IHubSystemAudio_SpectrumDataChangedHandler
    {
        #region Init
        protected virtual void OnEnable()
        {
            ManagerHolder.SystemAudioManager.Register(this);
        }
        protected virtual void OnDisable()
        {
            ManagerHolder.SystemAudioManager.UnRegister(this);
        }
        #endregion

        #region Callback
        public virtual void OnRawSampleDataChanged(float[] rawSampleData)
        {
        }
        public virtual void OnFFTDataChanged(float[] fftData)
        {
        }
        public virtual void OnSpectrumDataChanged(float[] spectrumData)
        {
        }
        #endregion
    }
}