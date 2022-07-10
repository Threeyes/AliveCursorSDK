using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 播放动态序列帧，针对3D Texture
/// </summary>
public class UISeqTexturePlayer : SequenceBase<Texture>
{
    public int frameCount = 30;
    public float intervalTime = 0;//每轮循环的间隔时间
    public Renderer rendererObj;

    protected override void SetDataFunc(Texture data, int index)
    {
        if (!rendererObj)
            return;
        base.SetDataFunc(data, index);
        rendererObj.material.mainTexture = data;
    }

    float startTime = 0;

    private void Awake()
    {
        IsLoop = true;
        startTime = Time.time;
        if (!rendererObj)
            rendererObj = GetComponent<Renderer>();
    }

    bool isWaitForNextRound = false;
    void Update()
    {
        float deltaTime = Time.time - startTime;

        if (deltaTime > 1f / frameCount)//等待下一循环
        {
            if (isWaitForNextRound)
            {
                if (deltaTime < intervalTime)
                    return;
                else
                    isWaitForNextRound = false;
            }

            SetNext();
            startTime = Time.time;

            if (intervalTime > 0)
            {
                if (CurIndex == Count - 1)//Reach End
                {
                    isWaitForNextRound = true;
                }
            }
        }
    }
}
