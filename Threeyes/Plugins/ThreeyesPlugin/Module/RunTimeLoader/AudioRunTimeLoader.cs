using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AudioRunTimeLoader : RunTimeLoaderBase<AudioClip>
{
    public AudioSource audioSource;

    protected override void Awake()
    {
        if (!audioSource)
            audioSource = GetComponent<AudioSource>();

        base.Awake();
    }


    protected override AudioClip GetAssetFunc(WWW www)
    {
        return www.GetAudioClip();
    }

    protected override void SetAssetFunc(AudioClip asset, UnityAction<AudioClip> actOnLoadSucExter = null)
    {
        base.SetAssetFunc(asset, actOnLoadSucExter);
        if (asset)
        {
            audioSource.clip = asset;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("无法加载音频！");
        }
    }
}
