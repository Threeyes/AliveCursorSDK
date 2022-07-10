using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ShakeManager : InstanceBase<ShakeManager>
{
    public Slider.SliderEvent onShaking;

    public UnityEvent onShakeStop;
    public float targetShakePercent;
    public float curShakePercent;
    public float frequency = 0.02f;

    bool isInvokeShakeStop = false;//是否已经调用了ShakeStop

    public void SetShakePercent(float value)
    {
        targetShakePercent = value;
    }

    public void StartShake(bool isOn)
    {
        if (isOn)
        {
            StartShake(tempRandomShake.ResultValue);
        }
        else
            StopShake();
    }

    /// <summary>
    /// 开始摇晃
    /// </summary>
    /// <param name="percent"></param>
    public void StartShake(float percent)
    {
        targetShakePercent = percent;
    }
    public void StopShake()
    {
        targetShakePercent = 0;
    }
    public void StopShakeImmediate()
    {
        targetShakePercent = 0;
        curShakePercent = 0;
    }
    public Random_Float tempRandomShake = new Random_Float(0.4f, 0.6f);
    //余震
    public void StartSmallShake(float durationTime)
    {
        StopAllCoroutines();
        StartCoroutine(IEStartTempShake(durationTime, tempRandomShake.ResultValue));
    }
    //临时震动
    IEnumerator IEStartTempShake(float durationTime, float shakePercent)
    {
        targetShakePercent = shakePercent;
        //Todo : 提醒余震发生！
        yield return new WaitForSeconds(durationTime);
        targetShakePercent = 0;
    }


    [Header("Test")]
    public float testShakePercent = 0.8f;
    public float testShakeTime = 3f;


    float lastShakeTime = 0;
    void FixedUpdate()
    {
        #region Input Test Shake

#if UNITY_EDITOR
        // Set Target Shake
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            StartSmallShake(testShakeTime);
        }
        float step = 0.1f * Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            targetShakePercent = targetShakePercent == 1 ? 0 : 1;
        }

        if (Input.GetKeyDown(KeyCode.Equals))
            targetShakePercent += step;
        if (Input.GetKeyDown(KeyCode.Minus))
            targetShakePercent -= step;
        targetShakePercent = Mathf.Clamp(targetShakePercent, 0, 1);
#endif

        #endregion

        curShakePercent = Mathf.Lerp(curShakePercent, targetShakePercent, Time.deltaTime);
        if (Mathf.Abs(curShakePercent - targetShakePercent) < 0.05f)
            curShakePercent = targetShakePercent;

        if (curShakePercent <= 0)//Stop Shake
        {
            if (!isInvokeShakeStop)
            {
                onShakeStop.Invoke();
                isInvokeShakeStop = true;
            }
            return;
        }
        else//Shaking
        {
            var curTime = Time.time;

            if (curTime - lastShakeTime > frequency)
            {
                onShaking.Invoke(curShakePercent);
                lastShakeTime = curTime;
            }
            isInvokeShakeStop = false;
        }
    }


}
