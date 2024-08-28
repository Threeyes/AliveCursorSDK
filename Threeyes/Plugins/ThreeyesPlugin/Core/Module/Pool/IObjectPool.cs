using System;

namespace Threeyes.Core
{
    public interface IObjectPoolHolder
    {
        IObjectPool Pool { get; set; }//The related pool
    }

    public interface IObjectPool { }
    /// <summary>
    /// Ref: UnityEngine.Pool.IObjectPool，为了兼容旧版本所以进行原接口复制
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IObjectPool<T> : IObjectPool where T : class
    {
        int CountInactive
        {
            get;
        }

        T Get();

        PooledObject<T> Get(out T v);

        void Release(T element);

        void Clear();
    }
}