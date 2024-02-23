using System;
using UnityEngine;
using UnityEngine.Events;
using Threeyes.Core;
#if USE_JsonDotNet
using Newtonsoft.Json;
#endif
/// <summary>
/// ToUpdate:
/// -参考UnityEngine.Rendering.VolumeParameter<T>，完善Equals等接口实现
/// </summary>
namespace Threeyes.Data
{
    //PS：该namespace存储所有Module公用的class

    public enum BasicDataState
    {
        Init = 1,
        Update = 2,
        DeInit = 3,
    }

    /// <summary>
    /// Turn common struct type into class, and get their value change event
    /// </summary>
#if USE_JsonDotNet
    [JsonObject(MemberSerialization.Fields)]
#endif
    public abstract class BasicData
    {
        /// <summary>
        /// 将字段克隆给其他物体，同时会调用事件更新
        /// </summary>
        /// <param name="other"></param>
        public abstract void CloneTo(ref object other);
        public abstract void NotifyValueChanged(BasicDataState state = BasicDataState.Update);
        public abstract void NotifyResetToDefaultValue();
        public abstract void ResetToDefaultValue();
        public abstract void ClearEvent();
    }

    //基本数据类
    public class BasicData<TValue> : BasicData
    {
        public virtual TValue Value
        {
            get { return GetValueFunc(); }
            set
            {
                if (!Equals(value))
                {
                    SetValueFunc(value);
                    NotifyValueChanged();
                }
            }
        }
        public virtual TValue ValueForceUpdate
        {
            get { return GetValueFunc(); }
            set
            {
                //PS:不检测是否与旧值相同，强制更新事件
                SetValueFunc(value);
                NotifyValueChanged();
            }
        }



        [SerializeField] protected TValue value;
        [SerializeField] protected TValue defaultValue;

        [JsonIgnore] public UnityAction<TValue> actionValueChanged;
        [JsonIgnore] public UnityAction<TValue> actionValueReset;
        [JsonIgnore] public UnityAction<TValue, BasicDataState> actionValueChangedEx;

        public BasicData()//用于反射调用
        {
        }
        public BasicData(TValue value)
        {
            this.value = value;
        }

        public override void CloneTo(ref object other)
        {
            BasicData<TValue> realOther = other as BasicData<TValue>;
            if (realOther != null)
            {
                realOther.SetValueFunc(GetValueFunc());
                realOther.defaultValue = defaultValue;
                //PS:不能覆盖其UnityAction
            }
            else
            {
                Debug.LogError("The other is Null!");
            }
        }
        public void SetValueWithoutNotify(TValue value)
        {
            SetValueFunc(value);
        }

        protected virtual TValue GetValueFunc()
        {
            return value;
        }
        protected virtual void SetValueFunc(TValue value)
        {
            this.value = value;
        }
        public override void NotifyResetToDefaultValue()
        {
            actionValueReset.Execute(defaultValue);
        }
        public override void NotifyValueChanged(BasicDataState state = BasicDataState.Update)
        {
            actionValueChanged.Execute(Value);
            actionValueChangedEx.Execute(Value, state);
        }


        public override void ResetToDefaultValue()
        {
            Value = defaultValue;
            NotifyResetToDefaultValue();
        }
        public override void ClearEvent()
        {
            actionValueChanged = null;
            actionValueChangedEx = null;
        }

        #region Utility
        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
        public override bool Equals(object other)
        {
            //Check for null and compare run-time types.
            if ((other == null) || !GetType().Equals(other.GetType()))
            {
                return false;
            }
            else
            {
                return EqualsFunc(((BasicData<TValue>)other).value);
            }
        }

        /// <summary>
        /// 值是否相同
        /// </summary>
        /// <param name="otherValue"></param>
        /// <returns></returns>
        protected virtual bool EqualsFunc(TValue otherValue)
        {
            return value.Equals(otherValue);
        }
        #endregion
    }

    public abstract class BasicData<TValue, TDataOption> : BasicData<TValue>, IDataOptionContainer<TDataOption>
        where TDataOption : IDataOption
    {
        public virtual IDataOption BaseDataOption { get { return option; } }
        public virtual TDataOption DataOption { get { return option; } }
        [SerializeField] protected TDataOption option;

        public BasicData() : base() { }
        public BasicData(TValue value) : base(value) { }
        public BasicData(TValue value, TDataOption option) : base(value)
        {
            this.option = option;
        }
    }

    public abstract class BasicNumberData<TValue, TOption> : BasicData<TValue, TOption>
        where TOption : DataOption_RangeBase<TValue>
    {
        protected BasicNumberData() : base() { }
        protected BasicNumberData(TValue value) : base(value) { }
        protected BasicNumberData(TValue value, TOption option) : base(value, option) { }

        public override TValue Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                TValue tempValue = value;

                if (option.UseRange)
                    tempValue = Clamp(value, option.MinValue, option.MaxValue);
                base.Value = tempValue;
            }
        }

        /// <summary>
        /// 外部调用并检测值是否在有效范围内
        /// </summary>
        /// <param name="value"></param>
        /// <returns>是否进行了裁剪</returns>
        public bool Clamp(TValue value, out TValue valueResult)
        {
            valueResult = Clamp(value, option.MinValue, option.MaxValue);
            return !EqualsFunc(valueResult);
        }

        protected abstract TValue Clamp(TValue value, TValue min, TValue max);

        public override void CloneTo(ref object other)
        {
            base.CloneTo(ref other);

            BasicData<TValue, TOption> realOther = other as BasicData<TValue, TOption>;
            if (realOther != null)
            {
                realOther.DataOption.UseRange = DataOption.UseRange;
                realOther.DataOption.MinValue = DataOption.MinValue;
                realOther.DataOption.MaxValue = DataOption.MaxValue;
            }
        }
    }

    /// <summary>
    /// Bug: value 通过Json进行反序列化时会报错，暂不直接用于Json序列化（因为读取的为string而不是Enum格式）
    /// </summary>
    [Serializable]
    public class EnumData : BasicData<Enum, DataOption_Enum>
    {
        public EnumData() { }
        public EnumData(Enum value) : base(value) { }

        public EnumData(Enum value, DataOption_Enum option) : base(value, option) { }
    }

    [Serializable]
    public class BoolData : BasicData<bool>
    {
        public BoolData() : base() { }
        public BoolData(bool value) : base(value) { }
    }

    [Serializable]
    public class StringData : BasicData<string>
    {
        public StringData() : base() { }
        public StringData(string value) : base(value) { }
    }

    [Serializable]
    public class Color32Data : BasicData<Color32>
    {
        public int r;
        public int g;
        public int b;
        public int a;
        public Color32Data() : base() { }
        public Color32Data(Color32 value) : base(value)
        {
            r = value.r;
            g = value.g;
            b = value.b;
            a = value.a;
        }
    }
    [Serializable]
    public class FloatData : BasicNumberData<float, DataOption_Float>
    {
        public FloatData() : base() { }
        public FloatData(float value) : base(value) { }
        public FloatData(float value, DataOption_Float option) : base(value, option) { }

        protected override float Clamp(float value, float minValue, float maxValue)
        {
            return Mathf.Clamp(value, minValue, maxValue);
        }
    }
    [Serializable]
    public class IntData : BasicNumberData<int, DataOption_Int>
    {
        public IntData() : base() { }
        public IntData(int value) : base(value) { }
        public IntData(int value, DataOption_Int option) : base(value, option) { }

        protected override int Clamp(int value, int min, int max)
        {
            return Mathf.Clamp(value, min, max);
        }
    }
}