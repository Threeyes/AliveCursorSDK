using System.Collections;
using Threeyes.Coroutine;
using UnityEngine;
/// <summary>
/// 音频渐入渐出
/// PS:Fade in out 可以通过物体移远，或者是DoTween的DoFade实现
/// </summary>
public class AudioFadeInOut : MonoBehaviour
{
    AudioSource audioSource;
    public float fadeTime = 1f;
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public void StartFadeInAndOut(float durationTime)
    {
        StartCoroutine(IEStartFadeInAndOut(durationTime));
    }

    IEnumerator IEStartFadeInAndOut(float durationTime)
    {
        yield return StartCoroutine(FadeIn(fadeTime));
        yield return new WaitForSeconds(durationTime);
        yield return StartCoroutine(FadeOut(fadeTime));
    }
    public void StartFadeIn(float fadeTime)
    {
        CoroutineManager.StartCoroutineEx(FadeIn(fadeTime));
    }
    public void StartFadeOut(float fadeTime)
    {
        CoroutineManager.StartCoroutineEx(FadeOut(fadeTime));
    }


    public IEnumerator FadeIn(float fadeTime)
    {
        float startVolume = 0.2f;

        audioSource.volume = 0;
        audioSource.Play();

        while (audioSource.volume < 1.0f)
        {
            audioSource.volume += startVolume * Time.deltaTime / fadeTime;

            yield return null;
        }

        audioSource.volume = 1f;
    }

    public IEnumerator FadeOut(float fadeTime)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeTime;

            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
    }
}
