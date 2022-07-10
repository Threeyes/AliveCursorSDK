using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ShakeExecuteListener : ShakeListenerBase
{
    public float _executePercent = -1;//执行onShaking的阈值。 如果_executePercent为负数，则使用RandomValue
    public Random_Float randomExecutePercent;

    public bool isExecuteOnce = false;//true：只执行一次，适用于不可逆的效果（玻璃破碎）； false：重复执行，适合落石等粒子效果

    public UnityEvent onShakeIncrease;//震动值升到某个百分比后执行的方法
    public UnityEvent onShakeDecrease;//震动值降低

    protected override void OnShaking(float percent)
    {
        float targetPercent = _executePercent;
        if (targetPercent < 0)
            targetPercent = randomExecutePercent.ResultValue;

        if (percent >= targetPercent)
        {

            if (isExecuteOnce && isExectued)
            {
                RemoveListeners();
            }
            else
            {
                OnShakeIncrease();
            }
        }
        else
        {
            OnShakeDecrease();
        }
    }
    
    bool isExectued = false;
    protected virtual void OnShakeIncrease()
    {
        //Todo:调用RemoveListeners
        onShakeIncrease.Invoke();
        isExectued = true;
    }

    protected virtual void OnShakeDecrease()
    {
        onShakeDecrease.Invoke();
    }

    protected override void OnShakeStop()//通常在开始时调用一次，
    {
        onShakeDecrease.Invoke();
    }
}
