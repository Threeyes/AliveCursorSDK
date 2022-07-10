using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
#if UNITY_EDITOR
#endif
/// <summary>
/// 常见的UI
/// </summary>
[DefaultExecutionOrder(200)]//迟于InterfaceAnimManager执行
public class UITips : UITipsBase<SOTipsInfo>
{
    public static string defaultName = " Tips";

#if UNITY_EDITOR

    [ContextMenu("UpdateTips")]
    public override void UpdateTips()
    {
        base.UpdateTips();
    }

#endif

}
