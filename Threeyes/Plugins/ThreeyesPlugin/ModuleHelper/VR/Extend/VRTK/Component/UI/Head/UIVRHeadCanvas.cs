using UnityEngine;
using UnityEngine.UI;
#if USE_DOTween
using DG.Tweening;
#endif
using UnityEngine.Events;
#if USE_VIU
using HTC.UnityPlugin.VRModuleManagement;
#endif
/// <summary>
/// HMD前方的UI，用于显示及播放2D声效
/// </summary>
public class UIVRHeadCanvas : InstanceBase<UIVRHeadCanvas>, IUITipsManager
{
    Canvas canvas;
    public CanvasGroup canvasGroup;
    public AudioSource audioSource;
    public AudioHelper audioHelper
    {
        get
        {
            if (!_audioHelper)
            {
                if (audioSource)
                    _audioHelper = audioSource.gameObject.AddComponentOnce<AudioHelper>();
            }
            return _audioHelper;
        }
    }
    AudioHelper _audioHelper;
    bool isListenToEvent = false;

    public override void SetInstance()
    {
        base.SetInstance();
        UITipsManager.Instance = this;
    }
    private void Awake()
    {
        canvas = GetComponent<Canvas>();

#if USE_VRTK
        if (VRTK.VRTK_SDKManager.instance)
        {
            VRTK.VRTK_SDKManager.instance.LoadedSetupChanged +=
    delegate (VRTK.VRTK_SDKManager sender, VRTK.VRTK_SDKManager.LoadedSetupChangeEventArgs e)
    {
        if (VRInterface.vrCamera)
        {
            //保持UI贴在当前的相机前
            transform.SetParent(VRInterface.vrCamera.transform);
            canvas.worldCamera = VRInterface.vrCamera;
            canvas.planeDistance = 0.2f;
        };
    };
        }
        else
        {
            Debug.LogError("无 VRTK_SDKManager.instance 实例");
        }

#elif USE_VIU
        //可用，但是效果不好
        VRModule.onNewPoses += OnNewPoses;
        isListenToEvent = true;
#endif

        ResetCanvasGroup();
    }

#if USE_VIU
    private void OnNewPoses()
    {
        if (HTC.UnityPlugin.Vive.VivePose.IsValidEx(HTC.UnityPlugin.Vive.DeviceRole.Hmd))
        {
            if (VRInterface.vrCamera)
            {
                canvas.renderMode = renderMode;
                switch (renderMode)
                {
                    case RenderMode.ScreenSpaceCamera:
                        //保持UI贴在当前的相机前
                        transform.SetParent(VRInterface.vrCamera.transform);
                        canvas.worldCamera = VRInterface.vrCamera;
                        canvas.planeDistance = 0.5f;
                        break;
                    case RenderMode.WorldSpace:
                        transform.SetParent(VRInterface.vrCamera.transform);
                        canvas.worldCamera = VRInterface.vrCamera;
                        canvas.transform.localEulerAngles = default(Vector3);
                        canvas.transform.localPosition = distanceToCamera;
                        //canvas的推荐缩放值为0.0005
                        break;
                }
            };
            VRModule.onNewPoses -= OnNewPoses;
            isListenToEvent = false;
        }
    }
    private void OnDestroy()
    {
        if (isListenToEvent)
            VRModule.onNewPoses -= OnNewPoses;//及时注销
    }
#endif


    public Image image;
    public Text text;
    public Sprite defaultSprite;
    public float tweenDuartion = 0.8f;
    public float defaultAlpha = 0f;
    public Vector2 alphaRange = new Vector2(0.4f, 0.8f);
    public bool isIndependentUpdate = false;//可用于游戏暂停时的提示效果。 If TRUE the tween will ignore Unity's Time.timeScale

    ////PC端VR：ScreenSpaceCamera ； 一体机端VR：World Space
    //一体机unitScale：0.0005，width：1000左右
    public RenderMode renderMode = RenderMode.ScreenSpaceCamera;
    [Header("RenderMode-SceneSpace")]
    public float defaultPlaneDistance = 0.5f;
    [Header("RenderMode-WorldSpace")]
    public Vector3 distanceToCamera = new Vector3(0, 0, 0.3f);
    public void SetTips(SOTipsInfo tipsInfo, UnityAction<float> actOnPlayFinish = null)
    {
        //Reset All
        ResetCanvasGroup();
        StopAudio();

        if (tipsInfo)//排除tipsInfo为空的情况
        {
            //Audio
            AudioClip clip = tipsInfo.audioClip;

            if (clip)
            {
                PlayAudio(clip, actOnPlayFinish);
            }

            if (tipsInfo.HasMessage)
            {
                if (image)
                {
                    image.sprite = tipsInfo.sprite != null ? tipsInfo.sprite : defaultSprite;//Image
                }
                if (text)
                {
                    text.text = tipsInfo.tips;//Text
                }

                canvasGroup.alpha = alphaRange.x;

#if USE_DOTween
                Tweener tweener = null;
                int tweenCount = tipsInfo.blinkCount;
                if (tweenCount != 0)//>0表示闪烁,tweenCount=-1表示无限循环
                {
                    if (tweenCount > 0)
                        tweenCount *= 2;//正确设置Tween的频率
                    tweener = canvasGroup.DOFade(alphaRange.y, tweenDuartion).SetLoops(tweenCount, LoopType.Yoyo);
                    if (isIndependentUpdate)
                        tweener.SetUpdate(true);
                    tweener.onComplete += ResetCanvasGroup;//PS:blinkCount加倍，避免太快消失
                }
                else if (tweenCount == 0)//0表示只显示
                {
                    tweener = canvasGroup.DOFade(alphaRange.y, tweenDuartion);
                }
#endif
            }
        }
    }

    public void FadeInCanvasGroup()
    {
#if USE_DOTween
        DOTween.Kill(canvasGroup);
        canvasGroup.DOFade(alphaRange.y, tweenDuartion);
#endif
    }


    public void FadeOutCanvasGroup()
    {
#if USE_DOTween
        DOTween.Kill(canvasGroup);
        canvasGroup.DOFade(0, tweenDuartion);
#endif
    }

    public void ResetCanvasGroup()
    {
#if USE_DOTween
        DOTween.Kill(canvasGroup);
        canvasGroup.alpha = defaultAlpha;
#endif
    }


    public void PlayAudio(AudioClip clip, UnityAction<float> actOnPlayFinish = null)
    {
        audioHelper.Play(clip, actOnPlayFinish);
    }


    /// <summary>
    /// 停止指定的音频
    /// </summary>
    /// <param name="clip">指定音频，可为空</param>
    public void StopAudio(AudioClip clip)
    {
        bool canStop = false;
        if (!clip)//clip为空，相当于不作判断，直接暂停
            canStop = true;
        else
        {
            if (audioSource.clip == clip && audioSource.isPlaying)//当前clip等于正在播放的clip
                canStop = true;
        }

        if (canStop)
            StopAudio();
    }

    public void StopAudio()
    {
        audioHelper.Stop();
    }

}
