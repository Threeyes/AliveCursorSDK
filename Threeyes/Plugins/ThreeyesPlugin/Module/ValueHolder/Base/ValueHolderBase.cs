using Threeyes.Core;
using UnityEngine;
using UnityEngine.Events;
namespace Threeyes.ValueHolder
{
    /// <summary>
    /// Hold or receive specify type of value
    /// </summary>
    public abstract class ValueHolderBase<TUnityEvent, TValue> : MonoBehaviour, IValueHolder<TValue>
        where TUnityEvent : UnityEvent<TValue>
    {
        public virtual TValue CurValue { get { return curValue; } set { curValue = value; onValueChanged.Invoke(value); } }
        [SerializeField] protected TValue curValue;

        public TUnityEvent onValueChanged;
    }
}