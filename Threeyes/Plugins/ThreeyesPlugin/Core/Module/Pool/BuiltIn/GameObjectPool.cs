using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Threeyes.Core
{
    /// <summary>
    /// Manage GameObject
    /// 
    /// Ref：https://thegamedev.guru/unity-cpu-performance/object-pooling/#object-pooling-in-unity-2021-your-options
    /// </summary>
    public class GameObjectPool : ObjectPool<GameObject>
    {
        public GameObject GOPoolManager
        {
            get
            {
                if (goPoolManager == null)
                {
                    goPoolManager = new GameObject("PoolManager" + (string.IsNullOrEmpty(tag) ? "" : $" [{tag}]"));
                    if (hiddenByDefault)
                        goPoolManager.SetActive(false);
                }
                return goPoolManager;
            }
        }
        GameObject goPoolManager;

        public string Tag { get { return tag; } }

        string tag = "";//自定义标签
        bool hiddenByDefault = false;//根物体默认隐藏
        public GameObjectPool(Func<GameObject> createFunc = null, UnityAction<GameObject> actionOnGet = null, UnityAction<GameObject> actionOnRelease = null, UnityAction<GameObject> actionOnDestroy = null, bool collectionCheck = false, int defaultCapacity = 10, int maxSize = 10000, string tag = "", bool hiddenByDefault = false) : base(createFunc, actionOnGet, actionOnRelease, actionOnDestroy, collectionCheck, defaultCapacity, maxSize)
        {
            this.tag = tag;
            this.hiddenByDefault = hiddenByDefault;
        }
        protected override bool IsElementNull(GameObject ele)
        {
            return ele == null && !ReferenceEquals(ele, null);
        }

        #region Default Method
        protected override GameObject DefaultCreateFunc()
        {
            return new GameObject("PooledObject");
        }
        protected override void DefaultOnGetFunc(GameObject target)
        {
            if (!target) return;
            target.SetActive(true);
        }
        protected override void DefaultOnReleaseFunc(GameObject target)
        {
            if (!target) return;

            if (!hiddenByDefault)//Warning:需要判断，否则后续SetParent会报错：GameObject is already being activated or deactivated.
                target.SetActive(false);
            target.transform.SetParent(GOPoolManager ? GOPoolManager.transform : null);//存放到一个Manager中
        }
        protected override void DefaultOnDestroyFunc(GameObject target)
        {
            UnityEngine.Object.Destroy(target);
        }
        #endregion


        ///PS：
        ///1.如果使用了Pool技术，那就会调用IPoolHandler接口对应的方法（参考LeanGameObjectPool.InvokeOnDespawn)
        ///2.不应该实现任何非通用代码（如设置父物体），以避免不通用。如果需要，请自行传入默认的方法

        static UnityAction<IPoolableHandler> actOnSpawn = (ele) => ele.OnSpawn();
        static UnityAction<IPoolableHandler> actOnDespawn = (ele) => ele.OnDespawn();


        protected override void InvokeOnGetFunc(GameObject element)
        {
            SendMessage(element, actOnSpawn, e => e.Pool = this);//调用IPoolableHandler.OnDespawn
            base.InvokeOnGetFunc(element);
        }
        protected override void InvokeOnReleaseFunc(GameObject element)
        {
            SendMessage(element, actOnDespawn);//调用IPoolableHandler.OnSpawn
            base.InvokeOnReleaseFunc(element);
        }

        #region Utility
        static List<IPoolableHandler> tempPoolableHandlers = new List<IPoolableHandler>();
        static List<IObjectPoolHolder> tempPoolableHolders = new List<IObjectPoolHolder>();
        static void SendMessage(GameObject target, UnityAction<IPoolableHandler> actHandler, UnityAction<IObjectPoolHolder> actHolder = null)
        {
            if (!target) return;
            if (actHandler != null)
            {
                target.GetComponents(tempPoolableHandlers);
                for (var i = tempPoolableHandlers.Count - 1; i >= 0; i--)
                    actHandler?.Invoke(tempPoolableHandlers[i]);
            }

            //标记Pool
            if (actHolder != null)
            {
                target.GetComponents(tempPoolableHolders);
                for (var i = tempPoolableHolders.Count - 1; i >= 0; i--)
                    actHolder.Invoke(tempPoolableHolders[i]);
            }
        }

        #endregion
    }
}