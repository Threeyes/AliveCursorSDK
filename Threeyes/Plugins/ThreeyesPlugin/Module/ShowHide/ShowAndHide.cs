using System.Collections.Generic;
using Threeyes.Core;
using UnityEngine;
using UnityEngine.Events;

namespace Threeyes.ShowHide
{
    /// <summary>
    /// 显示/隐藏特定物体/组件
    /// </summary>
    public class ShowAndHide : ShowAndHideBase
    {
        /// <summary>
        /// 需要隐藏的类型
        /// </summary>
        public enum HideType
        {
            Others = -1,//自定义，可使用事件、Tween等控制物体的显隐
            GameObject = 0,
            Collider,//碰撞体（常用于暂时禁用VR交互物体）
            Behaviours,//组件（如Light）
            Components = 10,//[Todo]不继承Behaviours的组件（Collider、Renderer）
            Renderer = 12,
        }
        public HideType hideType = HideType.GameObject;

        public virtual HideType HideMode { get { return hideType; } }
        /// <summary>
        /// 需要被禁用的组件
        /// </summary>
        public List<Behaviour> listBehaviour = new List<Behaviour>();
        public List<Component> listComponent = new List<Component>();

        //针对子物体的组件
        public bool isIncludeHide = true;
        public bool isRecursive = true;
        public bool includeSelf = true;

        protected override void ShowFunc(bool isShow)
        {
            switch (HideMode)
            {
                case HideType.Others:
                    break;
                case HideType.GameObject:
                    gameObject.SetActive(isShow); break;
                case HideType.Collider:
                    //隐藏所有的子碰撞体
                    ForEachChildComponent<Collider>(c => c.enabled = isShow);
                    //tfThis.Recursive((t) => t.GetComponents<Collider>().ToList().ForEach((c) => c.enabled = isShow)); 
                    break;
                case HideType.Renderer:
                    ForEachChildComponent<Renderer>(c => c.enabled = isShow);
                    //tfThis.Recursive((t) => t.GetComponents<Renderer>().ToList().ForEach((r) => r.enabled = isShow)); 
                    break;
                case HideType.Behaviours:
                    foreach (Behaviour b in listBehaviour)
                    {
                        b.enabled = isShow;
                    }
                    break;
            }
        }

        public virtual void ForEachChildComponent<T2>(UnityAction<T2> func) where T2 : Component
        {
            transform.ForEachChildComponent(func, isIncludeHide, includeSelf, isRecursive);
        }
    }
}