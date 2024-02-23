using UnityEngine;
using UnityEngine.UI;
#if USE_DOTween
using DG.Tweening;
#endif

namespace Threeyes.ModuleHelper
{
    /// <summary>
    /// 通过Tween折叠元素
    /// 
    /// 用途：
    /// -点击折叠/展开UI
    /// </summary>
    public class LayoutElementHelper_TweenFold : MonoBehaviour
    {
        //#Config
        public bool hideOnFold = true;
        public Selectable selectable;//eg: Toggle
        public LayoutElement layoutElement;
        public float tweenEndValue = 300;
        public float tweenDuration = 0.5f;

#if USE_DOTween
        public Ease easeExpand = Ease.OutBack;
        public Ease easeFold = Ease.OutSine;

        //#Runtime
        Tween tween;
#endif


        private void Awake()
        {
            if (selectable is Toggle toggle)
            {
                toggle.onValueChanged.AddListener((b) => OnUpdateVisuals(b));
            }
        }
        public void OnUpdateVisuals(bool isExpand)
        {
#if USE_DOTween
            //ToUpdate:增加Flag枚举，决定需要进行变换的字段
            if (tween != null)
            {
                tween.Kill(false);
            }

            if (isExpand)
                layoutElement.gameObject.SetActive(true);
            tween = DOTween.To(
                () => layoutElement.preferredWidth,
                (f) => layoutElement.preferredWidth = f, isExpand ? tweenEndValue : 0, tweenDuration).SetEase(isExpand ? easeExpand : easeFold);
            tween.onComplete += () =>
              {
                  if (!isExpand)
                      layoutElement.gameObject.SetActive(false);
              };
#endif
        }
    }
}