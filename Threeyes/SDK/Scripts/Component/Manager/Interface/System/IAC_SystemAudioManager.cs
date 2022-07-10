using System;

public interface IAC_SystemAudioManager
{
    int RawSampleCount { get; }
    int FFTCount { get; }
    int SpectrumCount { get; }

    float CalculateLoudness(float[] rawSampleData);
}

public interface IAC_SystemAudio_RawSampleDataChangedHandler
{
    void OnRawSampleDataChanged(float[] rawSampleData);
}
public interface IAC_SystemAudio_FFTDataChangedHandler
{
    void OnFFTDataChanged(float[] fftData);
}
public interface IAC_SystemAudio_SpectrumDataChangedHandler
{
    void OnSpectrumDataChanged(float[] spectrumData);
}
