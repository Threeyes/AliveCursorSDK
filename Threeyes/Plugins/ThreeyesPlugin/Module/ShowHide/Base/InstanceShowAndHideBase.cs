using Threeyes.Core;
using UnityEngine;

namespace Threeyes.ShowHide
{
    /// <summary>
    /// 带显隐功能的单例，适用于组物体
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [RequireComponent(typeof(InstanceManager))]
    public abstract class InstanceShowAndHideBase<T> : ShowAndHideBase, ISetInstance
            where T : InstanceShowAndHideBase<T>
    {
        public static T Instance;
        bool isInit = false;
        public virtual void SetInstance()
        {
            if (isInit)
                return;

            SetInstanceFunc();
        }

        protected virtual void SetInstanceFunc()
        {
            Instance = this as T;
            isInit = true;
        }
    }
}