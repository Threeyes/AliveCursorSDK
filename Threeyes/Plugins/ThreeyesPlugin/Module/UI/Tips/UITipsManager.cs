using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 管理场景中全局的Tips
/// </summary>
public static class UITipsManager
{
    public static IUITipsManager Instance;
}

public interface IUITipsManager
{
    void PlayAudio(AudioClip clip, UnityAction<float> actOnPlayFinish);
    void StopAudio(AudioClip clip);

}