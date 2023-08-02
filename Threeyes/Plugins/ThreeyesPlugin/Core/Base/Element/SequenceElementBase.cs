using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 基于Content的元素
/// </summary>
/// <typeparam name="TData"></typeparam>
public class SequenceElementBase<TManager, TElement, TData> : ElementBase<TData>, SequenceElement<TManager>
    where TManager : SequenceElementManagerBase<TElement, TData>
    where TElement : ElementBase<TData>
        where TData : class
{
    public TManager Manager
    {
        get
        {
            return manager;
        }
        set
        {
            manager = value;
        }
    }
    [SerializeField] protected TManager manager;


    /// <summary>
    /// Index in content
    /// </summary>
    public int Index
    {
        get
        {
            return index;
        }
        set
        {
            index = value;
        }
    }

    [SerializeField] protected int index;
}

public interface SequenceElement<TManager>
{
    TManager Manager { get; set; }
    int Index { get; set; }
}