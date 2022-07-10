using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 播放动态序列帧，针对Sprite
/// </summary>
[System.Obsolete("Use GifPlayer instead", false)]
public class UIGifPlayer : SequenceBase<Sprite>
{
    public int frameCount = 30;
    public Image image;

    protected override void SetDataFunc(Sprite data, int index)
    {
        base.SetDataFunc(data, index);
        image.sprite = data;
    }

    float startTime = 0;

    private void Awake()
    {
        IsLoop = true;
        startTime = Time.time;
        if (!image)
            image = GetComponent<Image>();
    }
    void Update()
    {
        float deltaTime = Time.time - startTime;
        if (deltaTime > 1f / frameCount)
        {
            SetNext();
            startTime = Time.time;
        }
    }
}
