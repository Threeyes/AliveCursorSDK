using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// PlayerPref的保存值
///调用方式（以按键为例）：
///  Button-OnButtonClick：调用PlayerPrefSettterBase.SetValue
///  PlayerPrefSettterBase.onSetValue： 引用ColyuPlugin自定义的OnXXXPlayerPrefSetValue方法(只更新UI但不调用其对应的事件（如onclick））
/// </summary>
/// <typeparam name="TValue"></typeparam>
public abstract class PlayerPrefSettterBase<TValue, TEvent> : MonoBehaviour, IPlayerPrefSetterInterface<TValue>
    where TEvent : UnityEvent<TValue>
{
    public string Key
    {
        get
        {
            if (key.IsNullOrEmpty())
            {
                Debug.LogError("Key is Null!");
            }
            return key;
        }

        set
        {
            key = value;
        }
    }
    public bool HasKey { get { return PlayerPrefs.HasKey(key); } }
    public bool IsSave { get { return isSave; } set { isSave = value; } }
    public bool IsLoadOnAwake { get { return isLoadOnAwake; } set { isLoadOnAwake = value; } }
    public virtual TValue PlayerPrefValue { get { return GetValue(); } set { SetValue(value); } }//读/写值
    public string key = "";//对应的key
    public TValue value;//读取的值
    public TValue defaultValue = default(TValue);

    [Header("Config")]
    [SerializeField]
    protected bool isSave = true;//是否保存配置
    [SerializeField]
    protected bool isLoadOnAwake = true;
    [SerializeField]
    protected bool isLoadOnAwakeTillHasKey = true;//当有key的时候才自动加载（需要用户手动调用SetValue或SetDefaultValue）

    public TEvent onSetValue;//加载参数时调用（注意该事件不应该间接调用到该类实例中的Set方法，因为会导致循环调用）

    protected virtual void Awake()
    {
        if (IsLoadOnAwake)
        {
            if (
                (isLoadOnAwakeTillHasKey && HasKey)//当系统中存有该key时才进行读取
                || !isLoadOnAwakeTillHasKey)
            {
                LoadValue();
            }
        }
    }

    /// <summary>
    /// 加载值（可用于外部调用）
    /// </summary>
    public void LoadValue()
    {
        PlayerPrefValue = PlayerPrefValue;//更新值
        Debug.Log("加载配置： " + key + " = " + PlayerPrefValue);
    }

    /// <summary>
    /// 取值
    /// </summary>
    /// <returns></returns>
    public virtual TValue GetValue()
    {
        value = GetValueFunc();
        return value;
    }

    protected abstract TValue GetValueFunc();

    /// <summary>
    /// 存值
    /// </summary>
    /// <param name="value"></param>
    public virtual void SetValue(TValue value)
    {
        SetValueFunc(value);
        SetUIFunc(value);
        onSetValue.Invoke(value);
        PlayerPrefs.Save();//By default Unity writes preferences to disk during OnApplicationQuit().This function will write to disk potentially causing a small hiccup, therefore it is not recommended to call during actual gameplay.
        Debug.Log("Save PlayerPrefs： " + key + " = " + value);
    }

    /// <summary>
    /// 如果没有Key，则设置默认值；有Key则使用已保存值
    /// </summary>
    /// <param name="defaultValue"></param>
    public virtual void SetDefaultValue(TValue defaultValue)
    {
        if (!HasKey)//首次写入:存入默认值
        {
            this.defaultValue = defaultValue;
            SetValue(defaultValue);
            Debug.Log("Save Default PlayerPrefs " + key + " = " + defaultValue);
        }
        else
        {
            LoadValue();
        }
    }


    protected abstract void SetValueFunc(TValue value);
    protected virtual void SetUIFunc(TValue value) { }


}
