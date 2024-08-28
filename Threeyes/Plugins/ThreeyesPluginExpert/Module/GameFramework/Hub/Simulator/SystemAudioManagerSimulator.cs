using UnityEngine;
using NaughtyAttributes;

namespace Threeyes.GameFramework
{
    public class SystemAudioManagerSimulator : SystemAudioManagerBase<SystemAudioManagerSimulator>
    {
        [Space]
        [InfoBox(
            "-Before game start, set AudioSource's [clip], then set AudioSource's [playOnAwake] to true if you prefer auto play (You can also Play/Pause the clip anytime during game start via EventPlayer)")]
        public AudioSource m_AudioSource;
        private void Update()
        {
            if (m_AudioSource.isPlaying)
            {
                m_AudioSource.GetOutputData(rawSampleData, 0);// get raw data for waveform			
                m_AudioSource.GetSpectrumData(fftData, 0, FFTWindow.BlackmanHarris);// fetch the fft	
                m_AudioSource.GetSpectrumData(spectrumData, 0, FFTWindow.Rectangular);// fetch the spectrum
                OnDataChanged();
            }
            else
            {
                OnDataUnChanged();
            }
        }
    }
}