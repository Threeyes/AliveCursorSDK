using UnityEngine;
using Threeyes.ValueHolder;
#if UNITY_EDITOR
using Threeyes.Editor;
#endif
/// <summary>
/// (PS:该类作为ValueHolder的子类，包含了对TValue的变换方法。后期应整理到同一个命名空间ValueChanger中
/// 更改float，并且能够检测是否到达Range
/// 
/// 注意：
///     重复值的可能不会重复调用特定的事件，需要勾选canRepeatInvoke。如：多次设置min值
/// </summary>
public class ValueChanger_Float : ValueHolder_Float
{
    public bool IsActive { get { return isActive; } set { isActive = value; } }
    [SerializeField] protected bool isActive = true;

    public bool IsReachMax { get { return CurValue == range.MaxValue; } }
    public float Step { get { return step; } set { step = value; } }
    public int StepInt { get { return (int)Step; } set { Step = value; } }
    public float StartValue { get { return startValue; } set { startValue = value; } }
    public float RangeMin { get { return range.MinValue; } set { range.MinValue = value; } }
    public float RangeMax { get { return range.MaxValue; } set { range.MaxValue = value; } }
    public bool IsReverse { get { return isReverse; } set { isReverse = value; } }


    public int CurValueInt
    {
        get { return (int)CurValue; }
        set { CurValue = value; }
    }

    public float CurPercent
    {
        get
        {
            if (isUseRange)
            {
                float percent = CurValue / (range.MaxValue - range.MinValue);
                return percent;
            }
            else
            {
                Debug.LogError("没有使用范围！");
                return 1;
            }
        }
        set
        {
            if (isUseRange)
            {
                CurValue = Mathf.Lerp(range.MinValue, range.MaxValue, value);
            }
        }
    }
    /// <summary>
    /// 
    /// （PS：如果需要在Awake的时候调用，请在程序开始前将curValue设置其为与StartValue不同的值）
    /// </summary>
    public override float CurValue
    {
        set
        {
            if (!IsActive)
                return;

            float newValue = value;

            //计算Clamp值
            if (isClampToNearest && clampIntervalValue > 0)
            {
                newValue = RoundToNearest(newValue);
            }

            //计算范围
            if (isUseRange)
                newValue = Mathf.Clamp(newValue, range.MinValue, range.MaxValue);

            if (canRepeatInvoke || (!canRepeatInvoke && curValue != newValue))
            {
                curValue = newValue;

                //计算输出值
                outputValue = newValue;
                outputDeltaValue = outputValue - curValue;
                outputDeltaValue *= scale;

                onAddSubtract.Invoke(outputDeltaValue);
                if (outputDeltaValue > 0)
                    onAdd.Invoke(outputDeltaValue);
                else if (outputDeltaValue < 0)
                    onSubtract.Invoke(outputDeltaValue);

                //对输出值进行处理
                outputValue += offset;
                outputValue *= scale;
                if (IsReverse)
                {
                    if (isUseRange)
                    {
                        ////PS:值取反的方法：v2=(-1*v1)+sum
                        float offsetValue = range.MinValue + range.MaxValue;//将范围内的值移动到以0为原点的对应位置
                        outputValue = outputValue * -1 + offsetValue;//将值基于0取反，然后移到原来的范围中
                    }
                    else
                    {
                        outputValue = -outputValue;
                    }
                }

                if (isUseRange)
                {
                    if (newValue == range.MaxValue)
                        onReachMax.Invoke(outputValue);
                    else if (newValue == range.MinValue)
                        onReachMin.Invoke(outputValue);
                    else
                    {
                        onReaching.Invoke(outputDeltaValue);
                    }
                }
                onValueChanged_Int.Invoke((int)outputValue);
                onValueChanged.Invoke(outputValue);
                onValueChangedVector2.Invoke(new Vector2(outputValue, range.MaxValue));
            }
        }
    }


    [Header("Input")]
    public bool isSetStartValueOnAwake = true;//注意：设置起始值会设置并调用相关事件。因此如果不想在开始时调用该事件，把该属性关掉
    public float startValue = 0;
    public float step = 0.01f;//每次调用无参加减时所增加的值

    public bool canRepeatInvoke = false;//当设置为相同的值时，能否重复调用相同的事件
    public bool isClampToNearest = false;//靠到最近的值（从range-minValue算起）
    public float clampIntervalValue = 1f;//裁剪的间隔值

    [Header("输入值的裁剪范围")]
    public bool isUseRange = true;
    public Range_Float range = new Range_Float(0, 1);

    [Header("对输出值进行变换.  outputValue = (raw + offset) * scale")]
    public float offset = 0;
    public float scale = 1;
    [SerializeField] private bool isReverse = false;//在有效范围中取反（如果限定了Range，则在Range中取反，否则在有理数中基于0为原点取反）


    //public float curValue = 0;//当前的值（只进行范围裁剪，未经缩放或位移处理）

    //Runtime
    protected float outputValue = 0;//输出的值（经过处理）
    protected float outputDeltaValue = 0;//间隔值（经过处理）


    public IntEvent onValueChanged_Int;
    public Vector2Event onValueChangedVector2;//通过Vector2存储[当前值，最大值]（用途：页码[16/30]）
    public FloatEvent onAdd;
    public FloatEvent onSubtract;
    public FloatEvent onAddSubtract;
    public FloatEvent onReaching;//正在到达的中途（还没到达起/终点），适合做一些计数、动态生成之类的需求
    public FloatEvent onReachMin;
    public FloatEvent onReachMax;
    public FloatEvent onReset;


#if UNITY_EDITOR
    private void OnValidate()
    {
        EditorTool.RepaintHierarchyWindow();
    }
#endif

    private void Awake()
    {
        if (isSetStartValueOnAwake)
            CurValue = StartValue;
    }

    public void Add(bool isAddPositive)
    {
        CurValue += isAddPositive ? step : -step;
    }
    public void Add(float delta)
    {
        CurValue += delta;
    }
    public void Add(int delta)
    {
        CurValue += delta;
    }
    public void Add()
    {
        CurValue += Step;
    }
    public void Subtract()
    {
        CurValue -= Step;
    }

    /// <summary>
    /// 适用于外部更新值，但是不进行事件通知
    /// </summary>
    /// <param name="value"></param>
    public void SetCurValueWithoutNotify(float value)
    {
        curValue = Mathf.Clamp(value, range.MinValue, range.MaxValue);
    }

    public void InitData()
    {
        curValue = StartValue;
    }
    public void ResetData()
    {
        curValue = StartValue;
        onReset.Invoke(CurValue);
    }

    #region Utilitty
    /// <summary>
    /// 计算最接近的值 （Ref：https://stackoverflow.com/questions/326476/rounding-a-number-to-the-nearest-5-or-10-or-x）
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    float RoundToNearest(float value)
    {
        float difValue = value - range.MinValue;//计算与开始值的差值
        float roundValue = Mathf.Round(difValue / clampIntervalValue) * clampIntervalValue;//计算最接近的值
        return range.MinValue + roundValue;
    }
    #endregion
}
