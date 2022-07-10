using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Sequence_SODialogTipsInfo : SequenceBase<SODialogTipsInfo>
{
    public DialogTipsInfoEvent onSetTipsInfo;
    protected override void SetDataFunc(SODialogTipsInfo data, int index)
    {
        base.SetDataFunc(data, index);
        onSetTipsInfo.Invoke(data);
    }
}
