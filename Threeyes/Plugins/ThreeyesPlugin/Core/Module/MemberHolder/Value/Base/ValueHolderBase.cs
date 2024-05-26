using UnityEngine;
using UnityEngine.Events;
namespace Threeyes.Core
{
    /// <summary>
    /// Cache and manage specific types of values
    /// </summary>
    /// <typeparam name="TParam"></typeparam>
    public interface IValueHolder<TParam>
    {
        TParam CurValue { get; set; }
    }
    /// <summary>
    /// Hold or receive specify type of value
    /// </summary>
    public abstract class ValueHolderBase<TUnityEvent, TValue> : MonoBehaviour, IValueHolder<TValue>
        where TUnityEvent : UnityEvent<TValue>
    {
        public virtual TValue CurValue { get { return curValue; } set { curValue = value; NotifyValueChanged(value); } }
        [SerializeField] protected TValue curValue;

        protected virtual void NotifyValueChanged(TValue value)
        {
            onValueChanged.Invoke(value);
        }


        public TUnityEvent onValueChanged;
    }
}