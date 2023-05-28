using Threeyes.Data;
using UnityEngine;
using UnityEngine.Events;
namespace Threeyes.Persistent
{
    public abstract class PersistentRangeNumberDataBase<TValue, TEvent, TOption> : PersistentDataBase<TValue, TEvent, TOption>
        where TEvent : UnityEvent<TValue>
        where TOption : DataOption_RangeBase<TValue>
    {
        //ToAdd:�ü�SetValue�е�ֵ
        protected TValue lastMinValue = default(TValue);
        protected TValue lastMaxValue = default(TValue);

        protected void OnValidate()
        {
            if (dataOption == null)//PS:�״���Ϊ������룬δ��ʼ���ᵼ�±���
                return;

            //ToAdd:�������ߵ������/��С/Ĭ��ֵʱ���Զ������������ֵ���ο�Slider
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
            //#1 ��鵱ǰ�޸ĵ�ֵ��ȷ��ÿ��ֵ�����£��ο�Slider
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

            //#2 ȷ��DefaultValue�ڷ�Χ��
            OnValidate_UpdateDefaultValue();
        }


        protected abstract void OnValidate_OnMinValueChanged();
        protected abstract void OnValidate_OnMaxValueChanged();

        protected abstract void OnValidate_UpdateDefaultValue();

    }
}