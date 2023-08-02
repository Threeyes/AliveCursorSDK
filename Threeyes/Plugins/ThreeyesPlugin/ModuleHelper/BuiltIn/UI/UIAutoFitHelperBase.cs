using System.Collections;
using System.Collections.Generic;
using Threeyes.Coroutine;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 根据图像，设置对应组件比例
/// </summary>
/// <typeparam name="TComponent"></typeparam>
public abstract class UIAutoFitHelperBase<TComponent> : ComponentHelperBase<TComponent>
    where TComponent : Component
{
    public Graphic TargetGraphic
    {
        get
        {
            if (!targetGraphic)
                targetGraphic = GetComponent<Graphic>();
            return targetGraphic;
        }

        set
        {
            targetGraphic = value;
        }
    }
    public Graphic targetGraphic;//图片等

    protected RectTransform RectTransform
    {
        get
        {
            if (!rectTransform)
                rectTransform = TargetGraphic.GetComponent<RectTransform>();
            return rectTransform;
        }

        set
        {
            rectTransform = value;
        }
    }
    RectTransform rectTransform;

    [Tooltip("有元素时才会显示")]
    public bool isOnlyShowWithElement = true;//有元素时才会显示

    [ContextMenu("SetAspect")]
    public virtual void SetAspect()
    {
        bool hasElement = false;
        float aspect = 0;
        //设置 Aspect
        //Text
        ////Texture
        RawImage rawImage = TargetGraphic as RawImage;
        if (rawImage)
        {
            hasElement = rawImage.texture != null;
            if (hasElement)
                aspect = (float)rawImage.texture.height / rawImage.texture.width;
        }
        Image image = TargetGraphic as Image;
        if (image)
        {
            hasElement = image.sprite != null;
            if (hasElement)
                aspect = (float)image.sprite.rect.height / image.sprite.rect.width;
        }

        if (isOnlyShowWithElement)
        {
            gameObject.SetActive(hasElement);
        }
        if (hasElement)
        {
            if (Application.isPlaying)
            {
                TryStopCoroutine();
                cacheEnum = CoroutineManager.StartCoroutineEx(IESetAspect(aspect));
            }
            else
            {
                SetAspectAtOnce(aspect);
            }
        }
    }

    IEnumerator IESetAspect(float aspectHeiDevWid)
    {
        //PS：UI隐藏时，RectTransform.rect.width为0，因此要等UI初始化完成后，长度对了才能设置尺寸
        yield return new WaitForEndOfFrame();
        if (!Comp)//避免组件被销毁
            yield break;
        SetAspectAtOnce(aspectHeiDevWid);
    }

    Coroutine cacheEnum;
    void TryStopCoroutine()
    {
        if (cacheEnum != null)
            CoroutineManager.StopCoroutineEx(cacheEnum);
    }

    protected abstract void SetAspectAtOnce(float aspectHeiDevWid);
}
