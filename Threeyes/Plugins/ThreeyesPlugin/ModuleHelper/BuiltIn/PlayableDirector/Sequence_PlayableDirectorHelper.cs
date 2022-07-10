using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 管理一组TL
/// 常用于：控制同一个物体（如车辆通过不同的TL，沿着指定的轨迹行走）
/// 注意：
/// 1.因为TL在游戏运行时都会重置，所以要不保持第一帧为空，要不默认隐藏其他TL
/// </summary>
[DefaultExecutionOrder(-400)]
public class Sequence_PlayableDirectorHelper : SequenceForCompBase<PlayableDirectorHelper>
{
    protected override void Awake()
    {
        base.Awake();
        //默认隐藏所有未调用的物体(Todo:增加Reset其他Data的方法，在里面写SetActive)
        foreach (var pdh in ListData)
        {
            pdh.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 激活指定物体
    /// </summary>
    /// <param name="data"></param>
    void ActiveData(PlayableDirectorHelper data)
    {
        //只显示当前data对应的物体
        foreach (var pdh in ListData)
        {
            pdh.gameObject.SetActive(data == pdh);
        }
    }

    public void ReplayCur(bool isPlay)
    {
        if (CurData)
            CurData.RePlay(isPlay);
    }

    public void PauseCur()
    {
        if (CurData)
            CurData.Pause(true);
    }

    public void EvaluateCur()
    {
        if (CurData)
            CurData.Evaluate();
    }

    public void ResumeCur()
    {
        if (CurData)
            CurData.Resume();
    }

    protected override void SetDataFunc(PlayableDirectorHelper data, int index)
    {
        ActiveData(data);
        data.RePlay(true);
    }
}
