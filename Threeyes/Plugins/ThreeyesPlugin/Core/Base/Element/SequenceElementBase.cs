using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.Core
{
    /// <summary>
    /// 基于Content的元素
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public class SequenceElementBase<TManager, TElement, TData> : ElementBase<TManager, TElement, TData>, ISequenceElement<TManager>
        where TManager : SequenceElementManagerBase<TElement, TData>
        where TElement : ElementBase<TData>
            where TData : class
    {
        /// <summary>
        /// Index in content
        /// </summary>
        public int Index { get { return index; } set { index = value; } }
        [SerializeField] protected int index;
    }

    public interface ISequenceElement<TManager> : IElement<TManager>
    {
        int Index { get; set; }
    }
}