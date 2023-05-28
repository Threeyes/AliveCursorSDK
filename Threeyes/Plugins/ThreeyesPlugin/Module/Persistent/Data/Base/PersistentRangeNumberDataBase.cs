using Threeyes.Data;
using UnityEngine;
using UnityEngine.Events;
namespace Threeyes.Persistent
{
    public abstract class PersistentRangeNumberDataBase<TValue, TEvent, TOption> : PersistentDataBase<TValue, TEvent, TOption>
        where TEvent : UnityEvent<TValue>
        where TOption : DataOption_RangeBase<TValue>
    {
        //ToAdd:裁剪SetValue中的值
        protected TValue lastMinValue = default(TValue);
        protected TValue lastMaxValue = default(TValue);

        protected void OnValidate()
        {
            if (dataOption == null)//PS:首次作为组件加入，未初始化会导致报错
                return;

            //ToAdd:当开发者调整最大/最小/默认值时，自动设置其他相关值，参考Slider
            if (dataOption.UseRange)
            {
                //Init
                if (lastMinValue.Equals(lastMaxValue) && lastMinValue.Equals(default(TValue)))
                {
                    lastMinValue = dataOption.MinValue;
                    lastMaxValue = dataOption.MaxValue;
                    return;
                }

                //UpdateRangeValue
                OnValidate_UpdateRange();
            }
        }

        protected virtual void OnValidate_UpdateRange()
        {
            //#1 检查当前修改的值，确保每个值都更新，参考Slider
            if (!dataOption.MinValue.Equals(lastMinValue))
            {
                OnValidate_OnMinValueChanged();
                lastMinValue = dataOption.MinValue;
            }
            else if (!dataOption.MaxValue.Equals(lastMaxValue))
            {
                OnValidate_OnMaxValueChanged();
                lastMaxValue = dataOption.MaxValue;
            }

            //#2 确保DefaultValue在范围中
            OnValidate_UpdateDefaultValue();
        }


        protected abstract void OnValidate_OnMinValueChanged();
        protected abstract void OnValidate_OnMaxValueChanged();

        protected abstract void OnValidate_UpdateDefaultValue();

    }
}