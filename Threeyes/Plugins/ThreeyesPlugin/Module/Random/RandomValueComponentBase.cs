using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 设置随机值
/// </summary>
/// <typeparam name="TRandom"></typeparam>
/// <typeparam name="T2"></typeparam>
public abstract class RandomValueComponentBase<TRandom, T2> : MonoBehaviour where TRandom : RandomBase<T2> where T2 : struct
{
    public TRandom randomValue = default(TRandom);
    public bool IsSetOnAwake = false;//Awake时设置值
    public bool IsNewValue = false;//每次调用时都设置新值

    public UnityEvent onBeforeSet;
    public UnityEvent onSet;

    /// <summary>
    /// 根据设置返回的新值
    /// </summary>
    protected T2 ResultValue
    {
        get
        {
            if (IsNewValue)
                randomValue.ResultValue = randomValue.RandomValue;
            return randomValue.ResultValue;
        }
    }

    protected virtual void Awake()
    {
        randomValue.Init();//初始化随机值

        //开始的时候设置新值
        if (IsSetOnAwake)
            SetValue();
    }

    [ContextMenu("SetValue")]
    public void SetValue()
    {
        onBeforeSet.Invoke();

        OnSet();
    }

    public virtual void OnSet()
    {
        onSet.Invoke();
    }

}
