#if USE_DOTween
using DG.Tweening;
#endif

using System.Collections;
using System.Collections.Generic;
using Threeyes.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Threeyes.ModuleHelper
{
    /// <summary>
    /// 管理一个Panel，可以与其他CanvasGroupHelper联合使用，也可以单独使用
    /// 
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class CanvasGroupHelper : ComponentHelperBase<CanvasGroup>, IShowHideEx
    {

        CanvasGroupGroupHelper canvasGroupGroupHelper
        {
            get
            {
                if (!m_canvasGroupGroupHelper)
                {
                    if (transform.parent)
                    {
                        m_canvasGroupGroupHelper = transform.parent.GetComponent<CanvasGroupGroupHelper>();
                    }
                }
                return m_canvasGroupGroupHelper;
            }
        }
        CanvasGroupGroupHelper m_canvasGroupGroupHelper;


        #region Show and Hide

        public bool isShowOnAwake = false;
        public bool isHideOnAwake = false;
        public bool canRepeatShowHide = true;//能否重复Show/Hide，true可避免显隐错误，保证CanvasGroup的唯一显示；false可保证事件只调用一次
        [SerializeField] bool isShowing = false;
        public bool isUseStartValue = true;//在动画执行前使用开始值。true：重新开始动画

        public BoolEvent onShowHide;
        public UnityEvent onShow;
        public UnityEvent onHide;

        public BoolEvent onBeginShowHide;//适用准备显示/隐藏前重置数据或播放Tween
        public UnityEvent onBeginShow;
        public UnityEvent onBeginHide;

        public bool IsShowing { get { return isShowing; } set { isShowing = value; } }

        private void Awake()
        {
            if (isShowOnAwake)
            {
                ShowFunc(true);
                IsShowing = true;
            }

            if (isHideOnAwake)
            {
                ShowFunc(false);
                IsShowing = false;
            }
        }

        public virtual void Show(bool isShow)
        {
            //判断是否有组，若有组则调用组
            if (canvasGroupGroupHelper)
            {
                if (isShow)
                {
                    canvasGroupGroupHelper.ShowCanvasGroup(this);//通知父组件
                }
                else//针对需要全局隐藏
                {
                    BeginShowHide(false);
                }
            }
            else
            {
                BeginShowHide(isShow);
            }
        }

        public void BeginShowHide(bool isShow)
        {
            ////！要注意IsShowing要与当前状态保持一致，否则在程序开始时，可能会因为状态不对而不隐藏
            if (!canRepeatShowHide && isShow == isShowing)//防止多次调用//Totest
                return;

#if USE_DOTween
            BeginShowHideFunc_Tween(isShow);
#else//立即显隐
        ShowAtOnce(isShow);
#endif
        }

        #region ShowHide with Tween

        //保留参数
        public Vector2 alphaRange = new Vector2(0, 1);
        public float tweenDuration = 0.8f;
        public bool ignoreTimeScale = true;

#if USE_DOTween
        public Ease ease = Ease.Linear;
        protected Tweener tweener;
        protected virtual void BeginShowHideFunc_Tween(bool isShow)
        {
            TryKillTween();

            if (isUseStartValue)
                Comp.alpha = isShow ? alphaRange.x : alphaRange.y;//Init

            tweener = Comp.DOFade(isShow ? alphaRange.y : alphaRange.x, tweenDuration).SetEase(ease).SetUpdate(ignoreTimeScale);

            if (isShow)
            {
                ShowFunc(true);
                onBeginShowHide.Invoke(true);
                onBeginShow.Invoke();
            }
            else
            {
                onBeginShowHide.Invoke(false);
                onBeginHide.Invoke();
                Comp.interactable = false;
            }

            //调用相关事件
            tweener.onComplete += () =>
            {
                IsShowing = isShow;

                if (isShow)
                {
                    onShow.Invoke();
                    Comp.interactable = true;
                }
                else
                {
                    onHide.Invoke();
                    ShowFunc(false);
                }
                onShowHide.Invoke(isShow);
            };
        }

        protected void TryKillTween()
        {
            if (tweener != null)
                tweener.Kill();
        }

#endif
        #endregion


        public void ToggleShow()
        {
            if (IsShowing)
                Hide();
            else
                Show();
        }

        [ContextMenu("Show")]
        public void Show()
        {
            Show(true);
        }
        [ContextMenu("Hide")]
        public void Hide()
        {
            Show(false);
        }

        public void ShowAtOnce(bool isShow)
        {
            IsShowing = isShow;
            onShowHide.Invoke(isShow);
            if (isShow)
            {
                onBeginShow.Invoke();
                Comp.alpha = alphaRange.y;
                Comp.interactable = true;

                ShowFunc(true);
                if (onShow != null)
                    onShow.Invoke();
            }
            else
            {
                onBeginHide.Invoke();
                Comp.alpha = alphaRange.x;
                Comp.interactable = false;

                ShowFunc(false);
                if (onHide != null)
                    onHide.Invoke();
            }
        }

        void ShowFunc(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        #endregion
    }
}