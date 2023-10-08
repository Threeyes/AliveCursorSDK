using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.Steamworks
{
    /// <summary>
    /// Receive Common Audio Event
    /// </summary>
    public class AudioVisualizerEventListener : MonoBehaviour
        , IHubSystemAudio_RawSampleDataChangedHandler
    {
        public FloatEvent onRawLoudnessChanged;

        #region Callback
        float cacheLoudness;
        public void OnRawSampleDataChanged(float[] rawSampleData)
        {
            cacheLoudness = AudioVisualizerTool.CalculateLoudness(rawSampleData);
            onRawLoudnessChanged.Invoke(cacheLoudness);
        }
        #endregion
    }
}