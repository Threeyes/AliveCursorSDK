using System.Collections;
using System.Collections.Generic;
using Threeyes.Coroutine;
using UnityEngine;
/// <summary>
/// 管理全局音频
/// </summary>
public class AudioManager : InstanceBase<AudioManager>
{

    public bool IsUsePreviousMutePrefs = false;

    public bool IsPreviousMutePrefs
    {
        get
        {
            return PlayerPrefs.GetInt("IsPreviousMute") != 0;
        }
        set { PlayerPrefs.SetInt("IsPreviousMute", value == true ? 1 : 0); }
    }

    public BoolEvent onInit;//初始化设置（需要激活Prefs）
    public BoolEvent onMuteUnMute = new BoolEvent();

    private void Awake()
    {
        if (IsUsePreviousMutePrefs)
            CoroutineManager.StartCoroutineEx(IEMute(IsPreviousMutePrefs));
    }

    IEnumerator IEMute(bool isMute)
    {
        yield return new WaitForEndOfFrame();
        Mute(IsPreviousMutePrefs);
        onInit.Invoke(IsPreviousMutePrefs);
    }

    [ContextMenu("ToggleMute")]
    public void ToggleMute()
    {
        Mute(!IsPreviousMutePrefs);
    }

    public void SetAudioListenerVolume(float percent)
    {
        AudioListener.volume = percent;
    }


    public void Mute(bool isMute)
    {
        AudioListener.volume = isMute ? 0 : 1;
        if (IsUsePreviousMutePrefs)
            IsPreviousMutePrefs = isMute;

        onMuteUnMute.Invoke(isMute);

    }
}
