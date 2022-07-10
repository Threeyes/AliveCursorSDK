using System;
using UnityEngine;

[System.Serializable]
public class GifFrameData : IDisposable
{
    public Texture2D texture;
    public float frameDelaysSeconds;//DelayTime

    public void Dispose()
    {
        if (texture)
            UnityEngine.Object.Destroy(texture);
    }
}