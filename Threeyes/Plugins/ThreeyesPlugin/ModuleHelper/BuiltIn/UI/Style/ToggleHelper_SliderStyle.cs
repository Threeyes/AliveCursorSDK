using UnityEngine;
using UnityEngine.UI;
#if USE_DOTween
using DG.Tweening;
#endif

namespace Threeyes.ModuleHelper
{
    /// <summary>
    /// 通过Slider实现的Toggle
    /// 
    /// PS:不需要监听Toggle事件，而是由UIField_Bool决定调用时机
    /// </summary>
    public class ToggleHelper_SliderStyle : MonoBehaviour
    {
        public Slider slider;//仅作为UI显示，不可交互
        public float tweenDuration = 0.5f;

#if USE_DOTween
        //#Runtime
        Tween tweenPercent;
        float curTweenPercent = 0;
#endif
        public void OnUpdateVisuals(bool input)
        {
#if USE_DOTween
            //PS:参考Slider，通过Anchor控制
            if (tweenPercent == null)
            {
                tweenPercent = DOTween.To(
                    () => curTweenPercent,

                     (percent) =>
                     {
                         curTweenPercent = percent;
                         slider.value = percent;
                         slider.handleRect.localEulerAngles = Vector3.Lerp(Vector3.zero, new Vector3(0, 0, -540), percent);
                     },
                     1, tweenDuration).SetUpdate(true).SetAutoKill(false);//PS:SetUpdate(true)可以忽略TimeScale导致动画卡住
            }
            tweenPercent.Pause();

            if (input)
                tweenPercent.SetEase(Ease.OutBounce).PlayForward();
            else
                tweenPercent.SetEase(Ease.InSine).PlayBackwards();
#endif
        }
    }
}