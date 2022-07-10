using System.Collections.Generic;
using UnityEngine;

#if USE_PostProcessing
using UnityEngine.PostProcessing;
#elif USE_PostProcessingV2
using UnityEngine.Rendering.PostProcessing;
#endif


/// <summary>
/// 设置PostProcessing中各模块特效
/// 
/// 
/// PS：VIU需要在场景中增加PostProcessingProfileManager脚本，用于初始化
/// </summary>
public class PostProcessingProfileHelper : CameraFXHelperBase
{
#if USE_PostProcessing
    public PostProcessingProfile profile;//当前引用的Profile
#endif

    public List<PPMode> listPPMode = new List<PPMode>();


#if USE_PostProcessing

    /// <summary>
    /// 激活选中的Mode
    /// </summary>
    /// <param name="isOn"></param>
    public void EnableModel(bool isOn)
    {
        foreach (PPMode ppMode in listPPMode)
        {
            EnableMode(ppMode, isOn);
        }
    }


    /// <summary>
    /// 启用/禁用当前相机的Porfile（调用这个方法的好处是不会影响资源库中的ppp）
    /// </summary>
    /// <param name="tempProfile"></param>
    public void ActiveProfile(bool isActive)
    {
        SetUpForEachCam
(
    (cam) =>
    {
        var ppb = cam.GetComponent<PostProcessingBehaviour>();
        if (ppb)
            ppb.enabled = isActive;

    }
);

    }
    /// <summary>
    /// 设置当前相机的Porfile
    /// </summary>
    /// <param name="tempProfile"></param>
    public void SetProfile(PostProcessingProfile tempProfile)
    {
        if (!tempProfile)
        {
            Debug.LogError("Null Profile!");
            return;
        }

        SetUpForEachCam
        (
            (cam) =>
            {
                var ppb = cam.AddComponentOnce<PostProcessingBehaviour>();
                ppb.profile = tempProfile;

            }
       );

        profile = tempProfile;
    }

    public void EnableModel(string modeName)
    {
        EnableMode(modeName, true);
    }
    public void DisableAllModel()
    {
        foreach (string enumName in System.Enum.GetNames(typeof(PPMode)))
        {
            if (enumName == PPMode.Null.ToString())
                continue;

            DisableModel(enumName);
        }
    }
    public void DisableModel(string modeName)
    {
        EnableMode(modeName, false);
    }

    void EnableMode(string modeName, bool isActive)
    {
        if (!profile)
        {
            Debug.LogError(name + ": PostProcessingProfile 文件未初始化！");
            return;
        }
        PPMode mode = modeName.Parse<PPMode>();
        EnableMode(mode, isActive);
    }

    public void EnableMode(PPMode mode, bool isActive)
    {
        EnableMode(profile, mode, isActive);
    }

    public static void EnableMode(PostProcessingProfile tempProfile, PPMode mode, bool isActive)
    {
        if (tempProfile.IsNull())
        {
            Debug.LogError("PostProcessingProfile is Null !");
            return;
        }

        if (mode == PPMode.Null)
        {
            Debug.LogError("Enum Not Set!");
            return;
        }
        switch (mode)
        {
            case PPMode.AmbientOcclusion:
                tempProfile.ambientOcclusion.enabled = isActive; return;
            case PPMode.ScreenSpaceReflection:
                tempProfile.screenSpaceReflection.enabled = isActive; return;
            case PPMode.DepthOfField:
                tempProfile.depthOfField.enabled = isActive; return;
            case PPMode.MotionBlur:
                tempProfile.motionBlur.enabled = isActive; return;
            case PPMode.EyeAdaptation:
                tempProfile.eyeAdaptation.enabled = isActive; return;
            case PPMode.Bloom:
                tempProfile.bloom.enabled = isActive; return;
            case PPMode.ColorGrading:
                tempProfile.colorGrading.enabled = isActive; return;
            case PPMode.UserLut:
                tempProfile.userLut.enabled = isActive; return;
            case PPMode.ChromaticAberration:
                tempProfile.chromaticAberration.enabled = isActive; return;
            case PPMode.Grain:
                tempProfile.grain.enabled = isActive; return;
            case PPMode.Vignette:
                tempProfile.vignette.enabled = isActive; return;
            case PPMode.Dithering:
                tempProfile.dithering.enabled = isActive; return;
            default:
                Debug.LogWarning("未定义！");
                return;
        }
    }
#endif


    [System.Serializable]
    public enum PPMode
    {
        [Name("无")]
        Null = 0,
        [Name("雾")]
        Fog = 1,
        [Name("抗锯齿")]
        Antialiasing = 2,
        [Name("环境光遮挡")]
        AmbientOcclusion = 3,
        [Name("屏幕空间反射")]
        ScreenSpaceReflection = 4,
        [Name("景深")]
        DepthOfField = 5,
        [Name("运动模糊")]
        MotionBlur = 6,
        [Name("人眼适应")]
        EyeAdaptation = 7,
        [Name("泛光")]
        Bloom = 8,
        [Name("颜色分级")]
        ColorGrading = 9,
        [Name("用户查找纹理")]
        UserLut = 10,
        [Name("色差")]
        ChromaticAberration = 11,
        [Name("颗粒")]
        Grain = 12,
        [Name("渐晕")]
        Vignette = 13,
        [Name("抖动显示")]
        Dithering = 14
    }
}

