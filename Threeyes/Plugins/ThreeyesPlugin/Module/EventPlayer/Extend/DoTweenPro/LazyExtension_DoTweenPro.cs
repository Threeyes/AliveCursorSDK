#if Threeyes_DoTweenPro
using DG.Tweening;
using DG.Tweening.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
public static class LazyExtension_DoTweenPro
{

    static DOTweenAnimation tweenAnimation;//Real
    static DOTweenPath tweenPath;
    static void TryConvert(this ABSAnimationComponent aBSAnimationComponent)
    {
        //tweenAnimation = null;
        tweenAnimation = aBSAnimationComponent as DOTweenAnimation;

        //tweenPath = null;
        tweenPath = aBSAnimationComponent as DOTweenPath;
    }



    #region Set
    public static Tween CreateEditorPreview(this ABSAnimationComponent aBSAnimationComponent)
    {
        aBSAnimationComponent.TryConvert();
        if (tweenAnimation)
        {
            return tweenAnimation.CreateEditorPreview();
        }
        if (tweenPath)
        {
            tweenPath.Invoke("Awake", 0);//Create Tween ( �ο� DoTweenPro source Code��
        }
        return null;
    }


    public static void ReCreateTween(this ABSAnimationComponent aBSAnimationComponent)
    {
        aBSAnimationComponent.TryConvert();
        if (tweenAnimation)
        {
            //tweenAnimation.DOKill();//Warning:Kills all tweens with the given ID or target and returns the number of actual
            if (tweenAnimation.tween != null)
                tweenAnimation.tween.Kill();
            tweenAnimation.tween = null;
            tweenAnimation.CreateTween();
        }
        //if (tweenPath)
        //{
        //    //�ο�DOTweenModuleUtils.CreateDOTweenPathTween()
        //    Comp.tween = motionTarget.transform.DOPath(tweenPath.path, tweenPath.duration, tweenPath.pathMode);
        //    tweenPath.Invoke("Awake", 0);//Create Tween ( �ο� DoTweenPro source Code��
        //}
    }

    public static void SetAutoPlay(this ABSAnimationComponent aBSAnimationComponent, bool isOn)
    {
        aBSAnimationComponent.TryConvert();
        if (tweenAnimation)
            tweenAnimation.autoPlay = isOn;
        if (tweenPath)
            tweenPath.autoPlay = isOn;
    }

    public static void SetAutoKill(this ABSAnimationComponent aBSAnimationComponent, bool isOn)
    {
        aBSAnimationComponent.TryConvert();
        if (tweenAnimation)
            tweenAnimation.autoKill = isOn;
        if (tweenPath)
            tweenPath.autoKill = isOn;
    }

    public static void SetTarget(this ABSAnimationComponent aBSAnimationComponent, GameObject target, bool isSelf, bool isTargetChanged)
    {
        aBSAnimationComponent.TryConvert();
        if (tweenAnimation)
        {
            tweenAnimation.targetIsSelf = isSelf;
            tweenAnimation.targetGO = target;

            //PS:DOTweenAnimation������ΪComponent��target����Ŀ��
            //Bug&&ToFix�� ��Ϊֻ��DOTweenAnimationInspector�вŻ����target��ֵ������ֻ��ѡ�е�Clip�Żῴ������
            //����취��
            //1.�����ߵȻظ���https://github.com/Demigiant/dotween/issues/500
            //2.ÿ��SetTarget��ǿ�Ƶ���DOTweenAnimationInspector�ĵ��÷���
            //�ο�DOTweenAnimationInspector.Validate��650�У� ��Ҫ�ֶ���ȡComponent
            //#ToReplace�������ܸ���ȫ�����ͣ�ֻ����ʱʹ�ã�����ʹ�������ṩ�ķ���
            bool isValid = Validate(tweenAnimation, target);//�����Ǹ���target��ֵ

            //            //ToDelete(��Ч): ���Ե���DOTweenAnimationInspector��OnInspectorGUI()
            //#if UNITY_EDITOR
            //            if (!Application.isPlaying && !isValid && isTargetChanged)
            //            {
            //                tweenAnimation.target = null;
            //                GameObject cacheObj = UnityEditor.Selection.activeGameObject;
            //                UnityEditor.Selection.activeGameObject = tweenAnimation.gameObject;//ǿ�Ƹ���

            //                var cacheType = tweenAnimation.animationType;
            //                tweenAnimation.animationType = DOTweenAnimation.AnimationType.None;
            //                UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            //                UnityEditor.SceneView.RepaintAll();
            //                UnityEditor.Selection.activeGameObject = null;//ǿ�Ƹ���
            //                tweenAnimation.animationType = cacheType;
            //                UnityEditor.Selection.activeGameObject = tweenAnimation.gameObject;//ǿ�Ƹ���
            //                UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            //                UnityEditor.SceneView.RepaintAll();

            //                //EditorTool.RepaintAllViews();
            //                //EditorTool.RepaintAllViews();
            //                //UnityEditor.Selection.activeGameObject = cacheObj;

            //                Debug.Log("Target Changed");
            //            }
            //#endif
        }
        ////To Impl: ��Ϊ��������
        //if (tweenPath)
        //{
        //}
    }


    public static void SetDuration(this ABSAnimationComponent aBSAnimationComponent, float duration)
    {
        aBSAnimationComponent.TryConvert();
        if (tweenAnimation)
            tweenAnimation.duration = duration;
        if (tweenPath)
            tweenPath.duration = duration;
    }

    public static void SetLoops(this ABSAnimationComponent aBSAnimationComponent, int value)
    {
        aBSAnimationComponent.TryConvert();
        if (tweenAnimation)
            tweenAnimation.loops = value;
        if (tweenPath)
            tweenPath.loops = value;
    }

    public static void SetLoopType(this ABSAnimationComponent aBSAnimationComponent, LoopType value)
    {
        aBSAnimationComponent.TryConvert();
        if (tweenAnimation)
            tweenAnimation.loopType = value;
        if (tweenPath)
            tweenPath.loopType = value;
    }

    #endregion

    #region Get

    public static GameObject GetTarget(this ABSAnimationComponent aBSAnimationComponent)
    {
        aBSAnimationComponent.TryConvert();
        if (tweenAnimation)
            return tweenAnimation.targetGO;
        return null;
    }

    public static Tween GetTween(this ABSAnimationComponent aBSAnimationComponent)
    {
        if (!aBSAnimationComponent)
            return null;
        aBSAnimationComponent.TryConvert();
        return aBSAnimationComponent.tween;
    }

    public static int GetLoops(this ABSAnimationComponent aBSAnimationComponent)
    {
        aBSAnimationComponent.TryConvert();
        if (tweenAnimation)
            return tweenAnimation.loops;
        if (tweenPath)
            return tweenPath.loops;
        LogNullError();
        return 0;
    }

    public static LoopType GetLoopType(this ABSAnimationComponent aBSAnimationComponent)
    {
        aBSAnimationComponent.TryConvert();
        if (tweenAnimation)
            return tweenAnimation.loopType;
        if (tweenPath)
            return tweenPath.loopType;

        LogNullError();
        return default(LoopType);
    }

    public static float GetDuration(this ABSAnimationComponent aBSAnimationComponent)
    {
        aBSAnimationComponent.TryConvert();
        if (tweenAnimation)
            return tweenAnimation.duration;
        if (tweenPath)
            return tweenPath.duration;
        //Debug.LogError("Null!");
        return 0;
    }

    public static void Goto(this ABSAnimationComponent aBSAnimationComponent, float time)
    {
        aBSAnimationComponent.TryConvert();

        if (aBSAnimationComponent && aBSAnimationComponent.tween != null)
        {
            aBSAnimationComponent.tween.Goto(time);
        }
    }


    static void LogNullError()
    {
        Debug.LogError("Null ABSAnimationComponent !");
    }

    #endregion

    #region Copy From DoTweenAnimationInspector

    /// <summary>
    ///  if a Component that can be animated with the given animationType is attached to the src
    ///  PS: Ref from: 
    /// </summary>
    /// <param name="_src"></param>
    /// <param name="targetGO"></param>
    /// <returns></returns>
    // Checks
    static bool Validate(DOTweenAnimation _src, GameObject targetGO)
    {
        if (_src.animationType == DOTweenAnimation.AnimationType.None) return false;

        Component srcTarget;
        // First check for external plugins
#if false // TK2D_MARKER
            if (_Tk2dAnimationTypeToComponent.ContainsKey(_src.animationType)) {
                foreach (Type t in _Tk2dAnimationTypeToComponent[_src.animationType]) {
                    srcTarget = targetGO.GetComponent(t);
                    if (srcTarget != null) {
                        _src.target = srcTarget;
                        _src.targetType = DOTweenAnimation.TypeToDOTargetType(t);
                        return true;
                    }
                }
            }
#endif
#if false // TEXTMESHPRO_MARKER
            if (_TMPAnimationTypeToComponent.ContainsKey(_src.animationType)) {
                foreach (Type t in _TMPAnimationTypeToComponent[_src.animationType]) {
                    srcTarget = targetGO.GetComponent(t);
                    if (srcTarget != null) {
                        _src.target = srcTarget;
                        _src.targetType = DOTweenAnimation.TypeToDOTargetType(t);
                        return true;
                    }
                }
            }
#endif
        // Then check for regular stuff
        if (_AnimationTypeToComponent.ContainsKey(_src.animationType))
        {
            foreach (Type t in _AnimationTypeToComponent[_src.animationType])
            {
                srcTarget = targetGO.GetComponent(t);
                if (srcTarget != null)
                {
                    _src.target = srcTarget;
                    _src.targetType = DOTweenAnimation.TypeToDOTargetType(t);
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// PS:ֻ����ʱ����ʹ�ã���Ϊ��DoTweenUtilityPanel�������֧�ֻ����������°�󣬿��ܻᵼ�����ö�ʧ������һ�ɽ��ÿ��ܲ����ڵ�
    /// </summary>
    static readonly Dictionary<DOTweenAnimation.AnimationType, Type[]> _AnimationTypeToComponent = new Dictionary<DOTweenAnimation.AnimationType, Type[]>() {
            {
                DOTweenAnimation.AnimationType.Move,
                new[]
                {
    //#if true // PHYSICS_MARKER
    //                typeof(Rigidbody),
    //#endif
    //#if true // PHYSICS2D_MARKER
    //                typeof(Rigidbody2D),
    //#endif
    //#if true // UI_MARKER
    //                typeof(RectTransform),
    //#endif
                    typeof(Transform)
                }},
                {
            DOTweenAnimation.AnimationType.Rotate,
            new[]
            {
#if true // PHYSICS_MARKER
                    typeof(Rigidbody),
#endif
#if true // PHYSICS2D_MARKER
                    typeof(Rigidbody2D),
#endif
                    typeof(Transform)
                }
            },
            { DOTweenAnimation.AnimationType.LocalMove, new[] { typeof(Transform) } },
            { DOTweenAnimation.AnimationType.LocalRotate, new[] { typeof(Transform) } },
            { DOTweenAnimation.AnimationType.Scale, new[] { typeof(Transform) } },
            { DOTweenAnimation.AnimationType.Color, new[] {
                typeof(Light),
//#if true // SPRITE_MARKER
//                typeof(SpriteRenderer),
//#endif
//#if true // UI_MARKER
//                typeof(Image), typeof(Text), typeof(RawImage), typeof(Graphic),
//#endif
                typeof(Renderer),
            }},
            { DOTweenAnimation.AnimationType.Fade, new[] {
                typeof(Light),
//#if true // SPRITE_MARKER
//                typeof(SpriteRenderer),
//#endif
//#if true // UI_MARKER
//                typeof(Image), typeof(Text), typeof(CanvasGroup), typeof(RawImage), typeof(Graphic),
//#endif
                typeof(Renderer),
            }},
//#if true // UI_MARKER
//            { DOTweenAnimation.AnimationType.Text, new[] { typeof(Text) } },
//#endif
            { DOTweenAnimation.AnimationType.PunchPosition, new[] {
//#if true // UI_MARKER
//                typeof(RectTransform),
//#endif
                typeof(Transform)
            }},
            { DOTweenAnimation.AnimationType.PunchRotation, new[] { typeof(Transform) } },
            { DOTweenAnimation.AnimationType.PunchScale, new[] { typeof(Transform) } },
            { DOTweenAnimation.AnimationType.ShakePosition, new[] {
//#if true // UI_MARKER
//                typeof(RectTransform),
//#endif
                typeof(Transform)
            }},
            { DOTweenAnimation.AnimationType.ShakeRotation, new[] { typeof(Transform) } },
            { DOTweenAnimation.AnimationType.ShakeScale, new[] { typeof(Transform) } },
            { DOTweenAnimation.AnimationType.CameraAspect, new[] { typeof(Camera) } },
            { DOTweenAnimation.AnimationType.CameraBackgroundColor, new[] { typeof(Camera) } },
            { DOTweenAnimation.AnimationType.CameraFieldOfView, new[] { typeof(Camera) } },
            { DOTweenAnimation.AnimationType.CameraOrthoSize, new[] { typeof(Camera) } },
            { DOTweenAnimation.AnimationType.CameraPixelRect, new[] { typeof(Camera) } },
            { DOTweenAnimation.AnimationType.CameraRect, new[] { typeof(Camera) } },
//#if true // UI_MARKER
//            { DOTweenAnimation.AnimationType.UIWidthHeight, new[] { typeof(RectTransform) } },
//#endif
        };
    #endregion
}
#endif