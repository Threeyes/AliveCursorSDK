using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.Core
{
    /// <summary>
    /// 存储相同的一系列元件
    /// </summary>
    /// <typeparam name="TElement"></typeparam>
    public class ElementGroupBase<TElement> : MonoBehaviour, IElementGroup
        where TElement : class
    {
        public List<TElement> listElement = new List<TElement>();

        public virtual void ResetData()
        {
            listElement.Clear();
        }
        public virtual void ResetElement() { }

        [ContextMenu("Clear")]
        public virtual void Clear()
        {
            //重置数据和元素
            ResetData();
            ResetElement();
        }
    }


    /// <summary>
    /// 带数据的元件
    /// </summary>
    /// <typeparam name="TElement"></typeparam>
    /// <typeparam name="TEleData"></typeparam>
    public abstract class ElementGroupBase<TElement, TEleData> : ElementGroupBase<TElement>
        where TElement : ElementBase<TEleData>
        where TEleData : class
    {
        public virtual TElement InitElement(TEleData data)
        {
            TElement element = CreateElementFunc(data);
            InitData(element, data);
            return element;
        }

        protected abstract TElement CreateElementFunc(TEleData data);
        protected virtual void InitData(TElement element, TEleData data)
        {
            element.Init(data);
        }
        protected virtual void AddElementToList(TElement element)
        {
            listElement.Add(element);
        }

        protected virtual void RemoveElementFromList(TElement element)
        {
            if (listElement.Contains(element))
            {
                listElement.Remove(element);
            }
        }
    }

    public abstract class ElementGroupBase<TManager, TElement, TEleData> : ElementGroupBase<TElement, TEleData>
        where TManager : class
        where TElement : ElementBase<TManager, TElement, TEleData>
        where TEleData : class
    {
        protected override void InitData(TElement element, TEleData data)
        {
            element.Manager = this as TManager;//Set Manager
            base.InitData(element, data);
        }
    }

    public interface IElementGroup
    {
        /// <summary>
        /// Reset Data and Clear all child instance
        /// </summary>
        void Clear();

        /// <summary>
        /// Reset Data
        /// </summary>
        void ResetData();

        /// <summary>
        /// Clear all child instance
        /// </summary>
        void ResetElement();
    }
}