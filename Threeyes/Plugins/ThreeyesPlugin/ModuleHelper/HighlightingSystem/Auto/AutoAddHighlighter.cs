#if USE_HighlightingSystem
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HighlightingSystem;
/// <summary>
/// 自动为所有带MeshRenderer组件的子物体增加HL组件
/// </summary>
public class AutoAddHighlighter : MonoBehaviour
{

    [ContextMenu("AutoAddHL")]
    public void AutoAddHLFunc()
    {
        transform.ForEachChildComponent<MeshRenderer>(
            (mR) =>
            {
                Highlighter highlighter = mR.AddComponentOnce<Highlighter>();
                highlighter.enabled = false;//默认不显示
                highlighter.constant = true;//设置为持续高亮

            }, true, true, false);
    }
}
#endif