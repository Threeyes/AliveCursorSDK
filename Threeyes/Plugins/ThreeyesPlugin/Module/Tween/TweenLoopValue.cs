#if USE_DOTween
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
/// <summary>
/// TweenLoop 特定数值，通过onChangeValue调用
/// </summary>
public class TweenLoopValue : TweenValue
{
    [Header("设置 Loop")]
    public bool isLoop = true;
    public int loops = -1;//循环次数
    public LoopType loopType = LoopType.Yoyo;


    protected override Tweener SetUpTween(Tweener sourceTweener)
    {
        if (isLoop)
            return base.SetUpTween(sourceTweener).SetLoops(loops, loopType);
        else
            return base.SetUpTween(sourceTweener);
    }

}
#endif