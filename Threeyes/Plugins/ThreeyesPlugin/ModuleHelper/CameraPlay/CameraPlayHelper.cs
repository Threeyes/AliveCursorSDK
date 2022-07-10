#if USE_CameraPlay

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CameraPlayHelper :
#if USE_CameraPlay
    CameraFXHelperBase
#else
    MonoBehaviour
#endif
{
    /// <summary>
    /// 是否在调用方法前重置特效，因为有些特效不会自动重置，会保留原有状态导致失效。（如果要实现特效叠加， 不推荐勾选）
    /// </summary>
    public bool isResetEffectBeforeInvoke = false;

    //受伤（边缘一环变色）
    [Header("受伤时的颜色（边缘变色）")]
    public Color colorHit = Color.red;


    [Header("闪光颜色")]
    public Color colorFlashLight = Color.white;

    [Header("全屏幕变色")]
    public Color colorOn = Color.red;

    [Header("醉酒特效")]
    public CameraPlay.Drunk_Preset DrunkPreset;

    #region 特效 （可叠加，可适用于多个相机）

    /// <summary>
    /// 受伤（带屏幕血迹）
    /// </summary>
    /// <param name="time"></param>
    public void BloodHit(float time)
    {
        SetUpForEachCam(() =>
     {
         if (time <= 0)
             CameraPlay.BloodHit();
         else
             CameraPlay.BloodHit(time);
     });
    }

    public void Hit(float time)
    {
        SetUpForEachCam(() =>
    CameraPlay.Hit(colorHit, time));
    }

    //径向模糊
    public void Radial(float time)
    {
        SetUpForEachCam(() =>
    CameraPlay.Radial(time));
    }

    //模糊
    public void Blur(float time)
    {
        SetUpForEachCam(() =>
        CameraPlay.Blur(time));
    }

    //屏幕故障
    public void Glitch(float time)
    {
        SetUpForEachCam(() =>
    CameraPlay.Glitch(time));
    }

    public void Glitch2(float time)
    {
        SetUpForEachCam(() =>
    CameraPlay.Glitch2(time));
    }

    //局部戳入变形
    public void Pitch(float time)
    {
        SetUpForEachCam(() =>
    CameraPlay.Pitch(time));
    }

    //色差
    public void Chromatical(float time)
    {
        SetUpForEachCam(() =>
    CameraPlay.Chromatical(time));
    }

    public void Fade_OnOff(bool isFade, float time)
    {
        if(isFade)
        {
            Fade_On(time);
        }
        else
        {
            Fade_Off(time);
        }
    }

    //Fade FX (淡出）
    /// <summary>
    /// 变暗
    /// </summary>
    /// <param name="time"></param>
    public void Fade_On(float time)
    {
        if (isResetEffectBeforeInvoke)
        {
            CameraPlay.Fade_Switch = false;
        }

        SetUpForEachCam(
            () => CameraPlay.Fade_ON(time),
            () =>
            {
                CameraPlay.CamFade = null;
                CameraPlay.Fade_Switch = false;
            });
    }

    public void Fade_Off(float time)
    {
        //Todo:应取消FadeOn的效果
        SetUpForEachCam(
            (cam) =>
            {
                CameraPlay.CamFade = cam.GetComponent<CameraPlay_Fade>();
                CameraPlay.Fade_OFF(time);
            },
            (cam) =>
            {
                CameraPlay.Fade_Switch = true;
            });
    }

    public void BlackWhite_ON()
    {
        BlackWhite_ON(1);
    }

    /// <summary>
    /// 黑白
    /// </summary>
    /// <param name="time"></param>
    public void BlackWhite_ON(float time)
    {
        SetUpForEachCam(
        () => CameraPlay.BlackWhite_ON(time),
        () =>
        {
            CameraPlay.CamBlackWhite = null;
            CameraPlay.BlackWhite_Switch = false;
        });
    }

    public void BlackWhite_OFF(float time)
    {
        SetUpForEachCam(
        (cam) =>
        {
            CameraPlay.CamBlackWhite = cam.GetComponent<CameraPlay_BlackWhite>();
            CameraPlay.BlackWhite_OFF(time);
        },
        (cam) =>
        {
            CameraPlay.BlackWhite_Switch = true;
        });
    }

    //Drunk 醉酒
    public void Drunk_ON()
    {
        if (isResetEffectBeforeInvoke)
        {
            CameraPlay.Drunk_Switch = false;//避免第二次不能进入
        }

        SetUpForEachCam(
            () => CameraPlay.Drunk_ON(DrunkPreset),
            () => CameraPlay.Drunk_Switch = false
            );
    }

    public void Drunk_OFF(float time)
    {
        SetUpForEachCam(
            () => CameraPlay.Drunk_OFF(time));
    }

    //闪光
    public void FlashLight(float time)
    {
        SetUpForEachCam(
        () => CameraPlay.FlashLight(colorFlashLight, time));
    }

    public void Pixel_ON()
    {
        Pixel_ON(1);
    }

    //像素化
    public void Pixel_ON(float time)
    {
        SetUpForEachCam(
            () => CameraPlay.Pixel_ON(time),
            () =>
            {
                CameraPlay.CamPixel = null;
                CameraPlay.Pixel_Switch = false;
            });
    }

    public void Pixel_OFF()
    {
        Pixel_OFF(1);
    }

    public void Pixel_OFF(float time)
    {
        SetUpForEachCam(
        (cam) =>
        {
            CameraPlay.CamPixel = cam.GetComponent<CameraPlay_Pixel>();
            CameraPlay.Pixel_OFF(time);
        },
        (cam) =>
        {
            CameraPlay.Pixel_Switch = true;
        });
    }


    //全屏幕变色
    public void Colored_ON(float time)
    {
        SetUpForEachCam(
        () => CameraPlay.Colored_ON(time),
        () =>
        {
            CameraPlay.CamColored = null;
            CameraPlay.Colored_Switch = false;
        });
    }

    public void Colored_OFF(float time)
    {
        SetUpForEachCam(
        (cam) =>
        {
            CameraPlay.CamColored = cam.GetComponent<CameraPlay_Colored>();
            CameraPlay.Colored_OFF(time);
        },
        (cam) =>
        {
            CameraPlay.Colored_Switch = true;
        });
    }

    #endregion

    void InitEffect()
    {
        if (isResetEffectBeforeInvoke)
            ResetAllEffect();
    }
    /// <summary>
    /// 重置效果，防止更改场景后发生冲突（适用场景：加载场景、重用特效）
    /// </summary>
    public void ResetAllEffect()
    {
        //以下方法都会调用并生成一个类（如CameraPlay_Fade），并且默认时间为1，因此会与实际的方法冲突
        //CameraPlay.NightVision_OFF();
        //CameraPlay.Drunk_OFF();
        //CameraPlay.SniperScope_OFF();
        //CameraPlay.BlackWhite_OFF();
        //CameraPlay.FlyVision_OFF();
        //CameraPlay.Fade_OFF(0);
        //CameraPlay.Pixel_OFF();
        //CameraPlay.Colored_OFF();
        //CameraPlay.Thermavision_OFF();
        //CameraPlay.Infrared_OFF();
        //CameraPlay.WidescreenH_OFF();
        //CameraPlay.RainDrop_OFF();
        //CameraPlay.Inverse_OFF();
    }

    public override void SetUpForEachCam(UnityAction<Camera> actionMain, UnityAction<Camera> actionForMultiCam = null)
    {
        InitEffect();

        //Bug：暂时不能针对复杂特效使用
        base.SetUpForEachCam(actionMain, actionForMultiCam);
    }

    public override void InitForEachCam(ref Camera cam)
    {
        CameraPlay.CurrentCamera = cam;
    }

}

#endif