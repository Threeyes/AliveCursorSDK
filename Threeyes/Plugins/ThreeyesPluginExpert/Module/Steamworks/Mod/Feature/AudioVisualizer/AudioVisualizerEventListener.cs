using System.Collections;
using System.Collections.Generic;
using Threeyes.Core;
using UnityEngine;

namespace Threeyes.Steamworks
{
    /// <summary>
    /// Receive Common Audio Event
    /// 
    /// ToAdd:
    /// -改为可持久化
    /// </summary>
    public class AudioVisualizerEventListener : MonoBehaviour
        , IHubSystemAudio_RawSampleDataChangedHandler
    {
        public FloatEvent onRawLoudnessChanged;

        #region Init
        protected virtual void OnEnable()
        {
            if (ManagerHolder.SystemAudioManager != null)//避免在建模场景测试时报错
                ManagerHolder.SystemAudioManager.Register(this);
        }
        protected virtual void OnDisable()
        {
            if (ManagerHolder.SystemAudioManager != null)//避免在建模场景测试时报错
                ManagerHolder.SystemAudioManager.UnRegister(this);
        }
        #endregion

        #region Callback
        [Tooltip("Smoothing loundness")] [Range(0.1f, 1)] public float lerpLoudness = 0.5f;//平滑音量（设置为1可立即到达目标音量）
        float lastLoudness;
        public void OnRawSampleDataChanged(float[] rawSampleData)
        {
            float curLoudness = Mathf.Lerp(lastLoudness, AudioVisualizerTool.CalculateLoudness(rawSampleData), lerpLoudness);
            onRawLoudnessChanged.Invoke(curLoudness);
            lastLoudness = curLoudness;
        }
        #endregion
    }
}