using UnityEngine;
/// <summary>
/// 管理头部UI
/// </summary>
public class VRHeadCanvasHelper : MonoBehaviour
{

    public SOTipsInfo tips;

    public FloatEvent onPlayAudioFinish;

    [ContextMenu("SetTips")]
    public void SetTips()
    {
        UIVRHeadCanvas.Instance.SetTips(tips, (f) => onPlayAudioFinish.Invoke(f));
    }

    public void ResetTips()
    {
        UIVRHeadCanvas.Instance.ResetCanvasGroup();
        UIVRHeadCanvas.Instance.StopAudio(tips ? tips.audioClip : null);
    }

    /// <summary>
    /// 停止所有音频
    /// </summary>
    public void StopAudio()
    {
        UIVRHeadCanvas.Instance.StopAudio();
    }

    public void FadeInCanvasGroup()
    {
        UIVRHeadCanvas.Instance.FadeInCanvasGroup();
    }

    public void FadeOutCanvasGroup()
    {
        UIVRHeadCanvas.Instance.FadeOutCanvasGroup();
    }
}