using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorHelper : ComponentHelperBase<Animator>
{
    public float normalizedTransitionDuration = 0.1f;

    public Vector2 speedRange = new Vector2(0, 1);//动画速度

    public void RandomSpeed()
    {
        Comp.speed = Random.Range(speedRange.x, speedRange.y);
    }

    public string curStateName;
    /// <summary>
    /// 切换到指定State
    /// </summary>
    /// <param name="stateName"></param>
    public void CrossFade(string stateName)
    {
        Comp.CrossFade(stateName, normalizedTransitionDuration);
    }

    #region Param

    /// <summary>
    /// 设置为动画的首帧并暂停，常用于开始展示
    /// 注意：要正常播放动画，必须先调用StopPlayback
    /// </summary>
    /// <param name="stateName"></param>
    public void SetFirstFrame(string stateName)
    {
        //Comp.CrossFade(stateName, 0);//切换到指定动画
        Comp.Play(stateName, -1, 0);//设置normalizedTime为初始值
        Comp.StartPlayback();//代码控制动画，同时禁止动画自动播放（https://docs.unity3d.com/ScriptReference/Animator.StartPlayback.html）
    }

    /// <summary>
    /// 取消动画控制
    /// </summary>
    public void StopPlayback()
    {
        Comp.StopPlayback();//代码控制动画
    }

    public void Play(string stateName)
    {
        Comp.Play(stateName);
    }

    public void SetTrigger(string name)
    {
        Comp.SetTrigger(name);
    }

    public string boolName = "";

    public void SetBool(bool isOn)
    {
        if (boolName != "")
        {
            if (isOn)
                SetBoolOn(boolName);
            else
                SetBoolOff(boolName);
        }
    }
    public void SetBoolOn(string name)
    {
        Comp.SetBool(name, true);
    }
    public void SetBoolOff(string name)
    {
        Comp.SetBool(name, false);
    }
    public void EnableAnimator(bool isEnable)
    {
        Comp.enabled = isEnable;
    }

    public string floatName;
    public void SetFloat(float value)
    {
        if (floatName.NotNullOrEmpty())
            Comp.SetFloat(floatName, value);
    }


    #endregion

}

