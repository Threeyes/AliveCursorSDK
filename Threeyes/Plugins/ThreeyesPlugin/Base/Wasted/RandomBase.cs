using UnityEngine;
/// <summary>
/// [Obsolete]
/// 配置Random及存储随机值的基类
/// (早期脚本，写得很烂，请勿使用！请使用RangeBase代替！）
/// </summary>
/// <typeparam name="TValue"></typeparam>
public abstract class RandomBase<TValue> : RangeBase<TValue> where TValue : struct
{
    [SerializeField] protected TValue resultValue = default(TValue);

    public RandomBase(TValue min, TValue max) : base(min, max) { }

    public TValue ResultValue
    {
        get
        {
            if (!IsValueInit(resultValue))//如果值未初始化，则设置一个随机默认值
                resultValue = RandomValue;
            return resultValue;
        }
        set
        {
            resultValue = value;
        }
    }

    public virtual void Init()
    {
        TValue tempValue = ResultValue;//强制resultValue初始化
    }
    /// <summary>
    /// 是否可用（参数已经有效设置）
    /// </summary>
    /// <returns></returns>
    public virtual bool IsAvaliable
    {
        get
        {
            return true;
        }
    }

    /// <summary>
    /// 值是否已经初始化
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    protected virtual bool IsValueInit(TValue value)
    {
        return !value.Equals(default(TValue));
    }
    public TValue GetAndSetNewRandomValue()
    {
        resultValue = RandomValue;
        return resultValue;
    }

    public abstract float Percent { get; }
}

[System.Serializable]
public class Random_Float : RandomBase<float>
{
    public override float Range { get { return MaxValue - MinValue; } }
    public override bool IsAvaliable { get { return MaxValue > MinValue; } }
    public Random_Float(float min, float max) : base(min, max)
    {
    }

    float cacheMinValue = 0;
    float cacheMaxValue = 0;

    /// <summary>
    /// 保持数据的可行性（常用于更改某一个数据时调用此方法更新其他数据）
    /// </summary>
    public void UpdateToStayAvaliable()
    {
        if (cacheMinValue == 0)
            cacheMinValue = MinValue;
        if (cacheMaxValue == 0)
            cacheMaxValue = MaxValue;

        if (MinValue > MaxValue)
        {
            if (MinValue != cacheMinValue)
            {
                MaxValue = MinValue;
                cacheMinValue = MinValue;
            }
            else if (MaxValue != cacheMaxValue)
            {
                MinValue = MaxValue;
                cacheMaxValue = MaxValue;
            }
        }
        else
        {
            cacheMinValue = MinValue;
            cacheMaxValue = MaxValue;
        }
    }

    protected override bool IsValueInit(float value)
    {
        return value > 0;//Todo:可以加个是否允许小于0的选项
    }
    public override float RandomValue { get { return Random.Range(MinValue, MaxValue); } }

    public override float Percent
    {
        get
        {
            if (IsAvaliable && Range != 0)
            {
                return (ResultValue - MinValue) / Range;
            }
            return 0;
        }
    }
}

[System.Serializable]
public class Random_Int : RandomBase<int>
{
    public override int Range { get { return MaxValue - MinValue; } }
    public override bool IsAvaliable { get { return MaxValue > MinValue; } }

    public Random_Int(int min, int max) : base(min, max)
    {

    }
    public override int RandomValue
    {
        get
        {
            return Random.Range(MinValue, MaxValue + 1);// Random.Range    Returns a random integer number between min [inclusive] and max [exclusive]
        }
    }

    public override float Percent
    {
        get
        {
            if (IsAvaliable && Range != 0)
            {
                return (ResultValue - MinValue) / Range;
            }
            return 0;
        }
    }
}

[System.Serializable]
public class Random_Vector3 : RandomBase<Vector3>
{
    public override Vector3 Range { get { return MaxValue - MinValue; } }
    public override bool IsAvaliable { get { return MaxValue != MinValue; } }

    public Random_Vector3(Vector3 min, Vector3 max) : base(min, max) { }
    public override Vector3 RandomValue
    {
        get
        {
            return new Vector3(Random.Range(MinValue.x, MaxValue.x), Random.Range(MinValue.y, MaxValue.y), Random.Range(MinValue.z, MaxValue.z));
        }
    }

    public override float Percent
    {
        get
        {
            if (IsAvaliable && Range != Vector3.zero)
            {
                return (ResultValue.magnitude - MinValue.magnitude) / Range.magnitude;
            }
            return 0;
        }
    }
}