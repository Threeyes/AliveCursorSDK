using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Threeyes.ShowHide
{
    /// <summary>
    /// 显示/隐藏 特定物品/组件
    /// </summary>
    public class ShowAndHideBase : MonoBehaviour, IShowHide
    {
        [Header("编辑器调试信息")]
        [SerializeField] protected bool isLogOnChange = false;

        [Space]
        [SerializeField] protected bool isShowing = false;
        public virtual bool IsShowing { get { return isShowing; } set { isShowing = value; } }

        public Toggle.ToggleEvent onShowHide;
        public UnityEvent onShow;
        public UnityEvent onHide;

        protected virtual void Awake()
        {
            InitState();
        }

        /// <summary>
        /// 初始化默认状态
        /// 
        /// </summary>
        void InitState()
        {
            //ToUpdate: 只有在程序首次运行时才会执行，还要避免因为场景加载导致，将IsShowing更新为当前值
        }

        public void ToggleShow()
        {
            Show(!IsShowing);
        }

        [ContextMenu("Show")]
        public void Show()
        {
            if (isLogOnChange)
                print("Show");
            Show(true);
        }
        [ContextMenu("Hide")]
        public void Hide()
        {
            if (isLogOnChange)
                print("Hide");
            Show(false);
        }

        public virtual void Show(bool isShow)
        {
            IsShowing = isShow;

            if (isShow)
                onShow.Invoke();
            else
                onHide.Invoke();
            onShowHide.Invoke(isShow);

            ShowFunc(isShow);
        }

        protected virtual void ShowFunc(bool isShow)
        {
            gameObject.SetActive(isShow);
        }

        public void HideAtOnce()
        {
            IsShowing = false;
            HideAtOnceFunc();
        }

        /// <summary>
        /// 立即隐藏，适用于重置
        /// </summary>
        protected virtual void HideAtOnceFunc()
        {
            gameObject.SetActive(false);
        }

    }
}