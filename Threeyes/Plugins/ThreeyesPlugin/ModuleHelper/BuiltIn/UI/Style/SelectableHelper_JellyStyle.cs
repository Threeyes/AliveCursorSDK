using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if USE_DOTween
using DG.Tweening;
#endif

namespace Threeyes.ModuleHelper
{
    /// <summary>
    /// 模拟点击的果冻效果
    /// </summary>
    public class SelectableHelper_JellyStyle : MonoBehaviour
    {
        public Selectable selectable;//eg: Button、Toggle
        public Transform tweenTarget;
        public float tweenDuration = 0.5f;
        public Vector3 tweenEndValue = new Vector3(0.2f, 0.2f, 0.2f);
        public int vibrato = 10;
        public float elasticity = 0.2f;
        public bool tweenOnToggleOff = true;

#if USE_DOTween
        //#Runtime
        Tween tween;
#endif
        private void Awake()
        {
            if (selectable == null)
                selectable = GetComponent<Selectable>();
            if (!selectable)
                return;
            if (!tweenTarget)
                tweenTarget = selectable.transform;

            if (selectable is Button button)
                button.onClick.AddListener(OnUpdateVisuals);
            else if (selectable is Toggle toggle)
            {
                toggle.onValueChanged.AddListener((b) =>
                {
                    bool isTween = !(!b && !tweenOnToggleOff);
                    if (isTween)
                        OnUpdateVisuals();
                });
            }
        }
        public void OnUpdateVisuals()
        {
#if USE_DOTween
            if (tweenTarget == null)
                return;

            //DOTween.Punch()
            if (tween == null)
            {
                tween = tweenTarget.transform.DOPunchScale(tweenEndValue, tweenDuration, vibrato, elasticity).SetAutoKill(false);
            }
            tween.Rewind();
            tween.Restart();
#endif
        }
    }
}