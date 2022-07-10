using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 设置一系列相同的SOOptionTipsInfo
/// 常用于选择题
/// </summary>
public class Sequence_SOOptionTipsInfo : SequenceBase<SOOptionTipsInfo>
{
    public OptionTipsInfoEvent onSetTipsInfo;
    protected override void SetDataFunc(SOOptionTipsInfo data, int index)
    {
        base.SetDataFunc(data, index);
        onSetTipsInfo.Invoke(data);
    }

    [System.Serializable]
    public class OptionTipsInfoEvent : UnityEvent<SOOptionTipsInfo>
    {

    }
}
