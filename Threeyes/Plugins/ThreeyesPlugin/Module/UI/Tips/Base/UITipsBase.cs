using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Threeyes.Coroutine;
using Threeyes.IO;
using Threeyes.Decoder;
#if USE_DOTween
using DG.Tweening;
#endif
/// <summary>
/// 通用提示的基类
/// Todo:移除对USE_Excelsior的依赖
/// </summary>
/// <typeparam name="T"></typeparam>
public class UITipsBase<T> : ShowAndHideBase where T : SOTipsInfo
{
    #region Property & Field

    public T tipsInfo;//提示信息

#if USE_Excelsior
    public InterfaceAnimManager interfaceAnimManager;
#endif

    [Header("参数")]
    public bool isSetTipsOnAwake = false;//Awake设置提示(PS:如果出现在程序运行时UI不显示的问题，可以把这个设置为false）
    public bool isPlayAudioOnAwake = false;//Awake设置提示时播放音频
    public bool isHideOnStart = true;//Start的时候隐藏
    public bool isPlayAudio = true;//是否播放音频

    [Header("外部引用")]
    public Text title;//标题
    public Text text;//内容
    public List<Text> listExtraText = new List<Text>();//额外的信息（如提示、备注等）

    public Image image;//用于设置UI（可空）
    public List<Image> listExtraImage = new List<Image>();//额外的Image

    public CanvasGroup canvasGoup;//用于设置UI组的整体透明度，用于更新Tips时增加流畅度（可空）
    public RawImage rawImage;//用于设置贴图（可空）

    public UnityEvent onSetTips;
    public AudioSource audioSource;//（可空）外部音源，如果为空则使用UIVRHeadCanvas

    //UI配置
    public BoolEvent onHasTitleContent;//是否包含Title内容，适用于设置UI组
    public virtual T TipsInfo { get { return tipsInfo; } set { tipsInfo = value; } }

    public AudioHelper audioHelper
    {
        get
        {
            if (!_audioHelper)
            {
                if (audioSource)
                {
                    _audioHelper = audioSource.gameObject.AddComponentOnce<AudioHelper>();
                }
            }
            return _audioHelper;
        }
    }

    public bool IsPlayAudio
    {
        get
        {
            return isPlayAudio;
        }

        set
        {
            isPlayAudio = value;
        }
    }

    AudioHelper _audioHelper;

    public FloatEvent onPlayAudioFinish;


    bool isAppearedAndSetTips;//是否已经显示并设置提示
    #endregion

    #region Public Func

    [ContextMenu("ShowAndPlay")]
    public void ShowAndPlay()
    {
        ShowAndPlay(true);
    }

    /// <summary>
    /// 显示并播放提示
    /// </summary>
    public void ShowAndPlay(bool isShow)
    {
        Show(isShow);
        if (isShow)
            SetTips(this.TipsInfo);//相当于用缓存的数据刷新，同时能调用音频
    }

    public void SetTipsAndShow(T tipsInfo)
    {
        if (!isAppearedAndSetTips)
            Show();
        SetTips(tipsInfo);
    }

    /// <summary>
    /// 设置提示并播放音频
    /// </summary>
    /// <param name="tipsInfo"></param>
    public void SetTips(T tipsInfo)
    {
        SetTips(tipsInfo, true);
    }

    public UnityAction<float> actionOnPlayAudioFinish;//音频结束时的事件，需要在SetTips后调用（不需要Remove，会自动清掉旧的）

    /// <summary>
    /// 只设置提示，不播放音频
    /// </summary>
    /// <param name="tipsInfo"></param>
    public void SetTipsWithoutAudio(T tipsInfo)
    {
        SetTips(tipsInfo, false);
    }

    /// <summary>
    /// 可用于手动播放
    /// PS:因为场景上有多个UITips，所以不能同时调用Audios
    /// </summary>
    [ContextMenu("PlayAudio")]
    public void PlayAudio()
    {
        PlayAudioFunc(true);
    }

    /// <summary>
    /// 停止当前tips对应的声音
    /// </summary>
    public void StopAudio()
    {
        PlayAudioFunc(false);
    }

    #endregion

    #region Inner Method

    protected void SetTips(T tipsInfo, bool isPlayAudio)
    {
#if UNITY_EDITOR
        //编辑器调试情况
        if (!Application.isPlaying)
        {
            SetTipsFunc(tipsInfo, isPlayAudio);
            return;
        }
#endif

        //ResetData
        actionOnPlayAudioFinish = null;

#if USE_DOTween
        //如果已经有CanvasGroup，则可以通过 渐隐再渐显 切换内容
        if (canvasGoup)
        {
            if (isAppearedAndSetTips)//该UI已经显示过至少一次（实现渐隐效果的条件）
            {
                if (fadeBetweenTips)
                {
                    TryStopCoroutine();
                    cacheEnumFadeInOut = CoroutineManager.StartCoroutineEx(IEFadeInOutThenSetTips(tipsInfo, isPlayAudio));
                    return;//成功设置，提前退出
                }
            }
        }
#endif

        //最基本无动画情况
        SetTipsFunc(tipsInfo, isPlayAudio);
    }
    protected Coroutine cacheEnumFadeInOut;
    protected void TryStopCoroutine()
    {
        if (cacheEnumFadeInOut != null)
            CoroutineManager.StopCoroutineEx(cacheEnumFadeInOut);
    }

    /// <summary>
    /// 切换Tips时的过度
    /// </summary>
    /// <param name="tipsInfo"></param>
    /// <param name="isPlayAudio"></param>
    /// <returns></returns>
    IEnumerator IEFadeInOutThenSetTips(T tipsInfo, bool isPlayAudio)
    {
#if USE_DOTween
        float tweenTime = fadeInOutDuration / 2;
        DOTween.To(() => canvasGoup.alpha, (x) => canvasGoup.alpha = x, 0, tweenTime).SetEase(easefadeIn);//Fade In
        yield return new WaitForSeconds(tweenTime);
        SetTipsFunc(tipsInfo, isPlayAudio);
        DOTween.To(() => canvasGoup.alpha, (x) => canvasGoup.alpha = x, 1, tweenTime).SetEase(easefadeOut);//Fade Out
#else
        yield return null;
        SetTipsFunc(tipsInfo, isPlayAudio);
#endif
    }

    //ToAdd:Async版本，调用SetTipsFunc（PS：可能会导致一系列问题，建议还是用同步实现）
    //ToAdd：增加一个回调，完成后设置图片
    protected virtual void SetTipsFunc(T tipsInfo, bool isPlayAudio)
    {
        if (tipsInfo)
        {
            this.TipsInfo = tipsInfo;

            //PS:按需加载外部资源，而不是SO被加载后全部加载到内存中，可避免程序开始前的卡顿及占用
            ///ToUpdate:
            ///1.使用ObjectCache，只需要加载一次
            if (Application.isPlaying)
            {
                string textureFilePath = tipsInfo.GetAbsPersistentAssetFilePath(tipsInfo.textureFilePath);
                if (textureFilePath.NotNullOrEmpty() && FileIO.Exists(textureFilePath))
                {
                    try
                    {
                        Texture result = FileIO.ReadAllBytes(textureFilePath).ToTexture();//自动从对应路径读取bytes
                        if (result)
                            tipsInfo.texture = result;//Update tipsinfo's texture
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
            }

            if (title)
            {
                bool hasTitle = tipsInfo.title.NotNullOrEmpty();
                onHasTitleContent.Invoke(hasTitle);
                title.gameObject.SetActive(hasTitle);
                title.text = tipsInfo.title;
            }

            //Text
            if (text)
            {
                bool isShow = GetTips().NotNullOrEmpty() && tipsInfo.displayContent.isShowText && text.enabled;//如果Text组件不显示，那就不出现（方便临时隐藏某条Text）
                text.gameObject.SetActive(isShow);
                text.text = GetTips();
                text.alignment = tipsInfo.textAnchor;
            }

            //RawImage
            if (rawImage)
            {
                bool isShow = tipsInfo.texture.NotNull() && tipsInfo.displayContent.isShowTexture && rawImage.enabled;
                rawImage.gameObject.SetActive(isShow);
                rawImage.texture = tipsInfo.texture;
            }

            if (image)
            {
                bool isShow = GetSprite().NotNull() && tipsInfo.displayContent.isShowSprite && image.enabled;
                image.gameObject.SetActive(isShow);
                image.sprite = GetSprite();
            }

            //额外信息
            SetExtraContent(tipsInfo, listExtraText, tipsInfo.listExtraTips, (inst, content) => inst.text = content);//Text
            SetExtraContent(tipsInfo, listExtraImage, tipsInfo.listExtraSprite, (inst, content) => inst.sprite = content);//Image

            if (isPlayAudio && IsShowing)
                PlayAudio();//尝试播放音乐

            ////统一用UIVRHeadCanvas播放音频，防止多个音频播放
            //if (tipsInfo.HasAudio && Application.isPlaying && isPlayAudio)
            //    UIVRHeadCanvas.Instance.PlayAudio(tipsInfo.audioClip);
            onSetTips.Invoke();

            isAppearedAndSetTips = true;
        }
    }





    private void SetExtraContent<TInst, TContent>(T tipsInfo, List<TInst> listExtraInst, List<TContent> listExtraContent, UnityAction<TInst, TContent> actionSetValue)
        where TInst : Behaviour
    {
        if (listExtraContent == null)//避免PD读取后为空的情况
        {
            listExtraInst.ForEach(inst =>
            {
                if (inst)
                {
                    inst.gameObject.SetActive(false);
                }
            });
            return;
        }

        for (int i = 0; i != listExtraInst.Count; i++)
        {
            bool hasContent = listExtraContent.Count > i;
            var subInst = listExtraInst[i];

            if (subInst == null)
                continue;
            if (!subInst.enabled)
                return;

            subInst.gameObject.SetActive(hasContent);//隐藏无内容的实例

            if (!hasContent)
            {
                continue;
            }

            actionSetValue(subInst, listExtraContent[i]);
        }
    }

    protected virtual string GetTips()
    {
        return tipsInfo.tips;
    }

    protected virtual Sprite GetSprite()
    {
        return tipsInfo.sprite;
    }

    void PlayAudioFunc(bool isPlay)
    {
        if (!TipsInfo)
            return;

        if (!(TipsInfo.HasAudio && Application.isPlaying && IsPlayAudio))//不满足条件：不播放
            return;

        AudioClip clip = null;
        if (TipsInfo.NotNull())
            clip = TipsInfo.audioClip;

        UnityAction<float> tempActionOnPlayAudioFinish = (duration) =>
        {
            onPlayAudioFinish.Invoke(duration);
            actionOnPlayAudioFinish.Execute(duration);
        };

        if (audioHelper)//使用自身的音源
        {
            audioHelper.Stop();
            if (isPlay)
            {
                if (clip)
                {
                    audioHelper.Play(clip, tempActionOnPlayAudioFinish);
                }
            }
        }
        else//使用UIVRHeadCanvas的音源（ToDelete）
        {
            //Todo:改为统一的Instance
            if (UITipsManager.Instance != null)
            {

                if (isPlay)
                    UITipsManager.Instance.PlayAudio(clip, tempActionOnPlayAudioFinish);//统一用UIVRHeadCanvas播放音频，可防止多个音频同时播放
                else
                    UITipsManager.Instance.StopAudio(TipsInfo ? TipsInfo.audioClip : null);//隐藏的时候关闭声效
            }
        }
    }

    #endregion

    #region Unity Method

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (TipsInfo && TipsInfo.isAutoUpdateSceneTips)
                //通知更新提示
                UpdateTips();
        }
#endif
    }

    protected override void Awake()
    {
        base.Awake();
#if USE_Excelsior
        if (!interfaceAnimManager)
            interfaceAnimManager = GetComponent<InterfaceAnimManager>();
#endif

        //Init
        isAppearedAndSetTips = false;
        IsShowing = !isHideOnStart;

        if (isSetTipsOnAwake)
            SetTipsFunc(TipsInfo, isPlayAudioOnAwake);
    }

    protected virtual void Start()
    {
        //延迟到Start才隐藏，避免InterfaceAnimManager未初始化完毕
        if (isHideOnStart)
        {
#if USE_Excelsior
            //Todo:要把InterfaceAnimManager的物体整个隐藏掉，或者直接设置为动画的最终状态
            if (interfaceAnimManager)
                interfaceAnimManager.DisappearAtOnce();
#endif

            gameObject.SetActive(false);
        }
        else
        {
            Show();
        }
    }

    #endregion

    #region Override IShowHideInterface

    public override bool IsShowing
    {
        get
        {
            return base.IsShowing;
        }
        set
        {
            base.IsShowing = value;
        }
    }
    public float fadeInOutDuration = 1f;//渐隐再渐显的总时长
    public bool fadeBetweenTips = true;//在切换Tips时使用渐变
#if USE_DOTween
    //Todo：改为使用CanvasGroupHelper
    public Ease easefadeIn = Ease.InOutSine;
    public Ease easefadeOut = Ease.InOutSine;
    public Tween tweener;

#endif

    void FadeIn(bool isStartFromZero = false, UnityAction actOnComplete = null)
    {
#if USE_DOTween
        float tweenDuration = fadeInOutDuration / 2;
        if (isStartFromZero)
            canvasGoup.alpha = 0;

        TryKillTween();

        tweener = DOTween.To(() => canvasGoup.alpha, (x) => canvasGoup.alpha = x, 1, tweenDuration).SetEase(easefadeIn);
        if (actOnComplete != null)
            tweener.onComplete += () => actOnComplete();//Fade In
#endif
    }

    void FadeOut(UnityAction actOnComplete)
    {
#if USE_DOTween
        float tweenDuration = fadeInOutDuration / 2;

        TryKillTween();

        tweener = DOTween.To(() => canvasGoup.alpha, (x) => canvasGoup.alpha = x, 0, tweenDuration).SetEase(easefadeIn);
        tweener.onComplete += () => actOnComplete();//Fade In
#endif
    }

    protected void TryKillTween()
    {
#if USE_DOTween
        if (tweener != null)
            tweener.Kill();
#endif
    }


    protected override void ShowFunc(bool isShow)
    {
        if (!isShow)
            isAppearedAndSetTips = false;
        StopAudio();

#if UNITY_EDITOR
        //编辑器调试情况
        if (!Application.isPlaying)
        {
            base.ShowFunc(isShow);
            return;
        }
#endif

#if USE_Excelsior
        if (interfaceAnimManager)
            interfaceAnimManager.Play(isShow);
        else
#endif      
        if (fadeInOutDuration > 0 && canvasGoup)//使用Tween
        {
            if (isShow)
            {
                base.ShowFunc(true);

                FadeIn(true);
            }
            else
            {
                FadeOut(() => base.ShowFunc(false));
            }
        }
        else
            base.ShowFunc(isShow);
    }

    #endregion

    #region Editor
#if UNITY_EDITOR

    /// <summary>
    /// 用于Editor按键更新
    /// </summary>
    public virtual void UpdateTips()
    {
        SetTips(TipsInfo);
    }
#endif
    #endregion
}
