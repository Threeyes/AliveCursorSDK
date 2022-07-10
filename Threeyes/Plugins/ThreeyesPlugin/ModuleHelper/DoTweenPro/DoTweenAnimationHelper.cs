#if USE_DOTweenPro

using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// PS:要把AutoKill关掉
/// 
/// 注意：DoTweenAnimation会自动调用子物体的DoTweenAnimation
/// </summary>
public class DoTweenAnimationHelper : ComponentHelperBase<DOTweenAnimation>
{
    Tween CurTween { get { return Comp ? Comp.tween : null; } }//返回默认的tween

    #region For All Tweens (凡是没有指定Tween的方法，都是针对所有Tween)

    public void DoRestartAll(bool isPlay)
    {
        if (isPlay)
        {
            foreach (var comp in Comps)
            {
                if (comp.tween.NotNull())
                    comp.tween.Restart();
            }
            //Comp.DORestart();//Bug:这个全局调用无效
        }
        else
        {
            Comp.DORewind();
            Comp.DOKill();
        }
    }

    #endregion


    public void DoGotoPercent(float Percent)
    {
        if (CurTween.IsNull())
            return;
        DOGoto(CurTween.Duration() * Percent);
    }

    public void DOGoto(float newTime)
    {
        if (CurTween.IsNull())
            return;
        Comp.tween.Goto(newTime);
    }

    public bool isRestartFromHere = false;
    public void DORestart(bool isPlay)
    {
        if (CurTween.IsNull())
        {
            if (isPlay)
            {
                Comp.DORestart(isRestartFromHere);
            }
            else
            {
                Comp.DOKill();
                Comp.DORewind();
            }
            return;
        }
        else//已有Tweener
        {

            if (isPlay)
            {
                CurTween.Restart();
            }
            else
            {
                CurTween.Kill();
                CurTween.Rewind();
            }
        }
    }

    public void DoPlayPause(bool isPlay)
    {
        if (CurTween.IsNull())
            return;

        if (isPlay)
            CurTween.Play();
        else
            CurTween.Pause();
    }

    public void DoPlayForwards(bool isForward)
    {
        if (CurTween.IsNull())
            return;

        if (isForward)
            CurTween.PlayForward();
        else
            CurTween.PlayBackwards();
    }

    [ContextMenu("CommitChangesAndRestart")]
    public void CommitChangesAndRestart()
    {
        if (CurTween.IsNull())
            return;

        CurTween.Rewind();
        CurTween.Kill();
        if (Comp.isValid)
        {
            Comp.CreateTween();// 编辑器下调用
            Comp.tween.Play();
        }
    }

}

#endif