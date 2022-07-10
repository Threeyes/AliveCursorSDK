using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PPMode = PostProcessingProfileHelper.PPMode;

#if USE_PostProcessing
using UnityEngine.PostProcessing;
#elif USE_PostProcessingV2
using UnityEngine.Rendering.PostProcessing;
#endif

/// <summary>
/// 保存UI的配置信息到持久化数据库中
/// </summary>
public class UIPlayerPrefElementBase<TOptionData, TPlayerPrefSetter, TValue> : ElementBase<TOptionData>
           where TOptionData : PlayerPrefDataBase<TValue>
 where TPlayerPrefSetter : IPlayerPrefSetterInterface<TValue>
{
    public TPlayerPrefSetter playerPrefSetter;

    public override void InitFunc(TOptionData data)
    {
        base.InitFunc(data);
        gameObject.SetActive(data.IsShow);
        if (data.IsShow)
            playerPrefSetter.SetDefaultValue(data.DefaultValue);

    }
}

#region Define

/// <summary>
/// 选项
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class PlayerPrefDataBase<T>
{
    public PlayerPrefDataBase()
    {
        IsShow = true;
    }
    public PlayerPrefDataBase(T tempDefaultValue)
    {
        DefaultValue = tempDefaultValue;
        IsShow = true;
    }
    public PlayerPrefDataBase(T tempDefaultValue, bool tempIsShow)
    {
        DefaultValue = tempDefaultValue;
        IsShow = tempIsShow;
    }

    /// <summary>
    /// 对应物体是否显示（Todelete：移动到UIPlayerPrefElementBase中）
    /// </summary>
    public bool IsShow { get { return isShow; } set { isShow = value; } }
    public T DefaultValue { get { return defaultValue; } set { defaultValue = value; } }

    [SerializeField]
    protected bool isShow = true;
    [SerializeField]
    protected T defaultValue;
}


[System.Serializable]
public class BoolPlayerPrefData : PlayerPrefDataBase<bool>
{
    public BoolPlayerPrefData() : base() { }
    public BoolPlayerPrefData(bool tempDefaultValue) : base(tempDefaultValue) { }
    public BoolPlayerPrefData(bool tempDefaultValue, bool tempIsShow) : base(tempDefaultValue, tempIsShow) { }
}
[System.Serializable]
public class IntPlayerPrefData : PlayerPrefDataBase<int>
{
    public IntPlayerPrefData(int tempDefaultValue) : base(tempDefaultValue) { }
    public IntPlayerPrefData(int tempDefaultValue, bool tempIsShow) : base(tempDefaultValue, tempIsShow) { }
}
[System.Serializable]
public class FloatPlayerPrefData : PlayerPrefDataBase<float>
{
    public FloatPlayerPrefData(float tempDefaultValue) : base(tempDefaultValue) { }
    public FloatPlayerPrefData(float tempDefaultValue, bool tempIsShow) : base(tempDefaultValue, tempIsShow) { }
}

[System.Serializable]
public class BoolPlayerPrefDataPostProcessing : BoolPlayerPrefData
{
    public PPMode ppMode = PPMode.Null;//需要显示的Mode
#if USE_PostProcessing
    public PostProcessingProfile profileOverride;//复写的Profile
#endif


    public BoolPlayerPrefDataPostProcessing() : base() { }
    public BoolPlayerPrefDataPostProcessing(bool tempDefaultValue) : base(tempDefaultValue) { }
    public BoolPlayerPrefDataPostProcessing(bool tempDefaultValue, bool tempIsShow) : base(tempDefaultValue, tempIsShow) { }
}

public abstract class ModuleBase
{
    public bool IsShow { get { return isShow && IsAnyOptionNeedToShow(); } set { isShow = value; } }
    [SerializeField]
    protected bool isShow = true;

    protected abstract bool IsAnyOptionNeedToShow();
}

#endregion
