using NaughtyAttributes;
using Newtonsoft.Json;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace Threeyes.Steamworks
{
    public class PostProcessingController : ConfigurableComponentBase<PostProcessingController, SOPostProcessingControllerConfig, PostProcessingController.ConfigInfo, PostProcessingController.PropertyBag>
    {
        #region Property & Field
        [Header("PostProcessing")]
        [Tooltip("The PostProcessing volume")][Required][SerializeField] protected Volume volume;
        #endregion

        #region IModHandler
        Bloom bloom = null;
        ChannelMixer channelMixer = null;
        ChromaticAberration chromaticAberration = null;
        ColorAdjustments colorAdjustments = null;
        //ColorCurves colorCurves = null;
        ColorLookup colorLookup = null;
        DepthOfField depthOfField = null;
        FilmGrain filmGrain = null;
        LensDistortion lensDistortion = null;
        LiftGammaGain liftGammaGain = null;
        MotionBlur motionBlur = null;
        PaniniProjection paniniProjection = null;
        ShadowsMidtonesHighlights shadowsMidtonesHighlights = null;
        SplitToning splitToning = null;
        Tonemapping tonemapping = null;
        Vignette vignette = null;
        WhiteBalance whiteBalance = null;
        public override void UpdateSetting()
        {
            if (!volume)
                return;
            if (!volume.profile)
                return;

            volume.gameObject.SetActive(Config.isUsePostProcessing);//控制模型的激活状态（包括其Trigger）

            ///ToUpdate:
            ///-【V2】如果相关Effect没激活，那就直接移除（尝试 volume.profile.Remove）。需要比较一下性能是否比保留Effect更好，因为需要实现移除、比对等额外操作，如没必要就不实现


            //——Bloom——
            if (volume.profile.TryGet(out Bloom bloom))
            {
                bloom.active = Config.bloom_IsActive;
                bloom.threshold.value = Config.bloom_Threshold;
                bloom.intensity.value = Config.bloom_Intensity;
                bloom.scatter.value = Config.bloom_Scatter;
                bloom.clamp.value = Config.bloom_Clamp;
                bloom.tint.value = Config.bloom_Tint;

                bloom.dirtIntensity.value = Config.bloom_DirtIntensity;
            }
            if (volume.profile.TryGet(out ChannelMixer channelMixer))
            {
                channelMixer.active = Config.channelMixer_IsActive;
                channelMixer.redOutRedIn.value = Config.channelMixer_RedOutRedIn;
                channelMixer.redOutGreenIn.value = Config.channelMixer_RedOutGreenIn;
                channelMixer.redOutBlueIn.value = Config.channelMixer_RedOutBlueIn;
                channelMixer.greenOutRedIn.value = Config.channelMixer_GreenOutRedIn;
                channelMixer.greenOutGreenIn.value = Config.channelMixer_GreenOutGreenIn;
                channelMixer.greenOutBlueIn.value = Config.channelMixer_GreenOutBlueIn;
                channelMixer.blueOutRedIn.value = Config.channelMixer_BlueOutRedIn;
                channelMixer.blueOutGreenIn.value = Config.channelMixer_BlueOutGreenIn;
                channelMixer.blueOutBlueIn.value = Config.channelMixer_BlueOutBlueIn;
            }
            if (volume.profile.TryGet(out ChromaticAberration chromaticAberration))
            {
                chromaticAberration.active = Config.chromaticAberration_IsActive;
                chromaticAberration.intensity.value = Config.chromaticAberration_Intensity;
            }
            if (volume.profile.TryGet(out ColorAdjustments colorAdjustments))
            {
                colorAdjustments.active = Config.colorAdjustments_IsActive;
                colorAdjustments.postExposure.value = Config.colorAdjustments_PostExposure;
                colorAdjustments.contrast.value = Config.colorAdjustments_Contrast;
                colorAdjustments.colorFilter.value = Config.colorAdjustments_ColorFilter;
                colorAdjustments.hueShift.value = Config.colorAdjustments_HueShift;
                colorAdjustments.saturation.value = Config.colorAdjustments_Saturation;
            }
            if (volume.profile.TryGet(out ColorLookup colorLookup))
            {
                colorLookup.active = Config.colorLookup_IsActive;
                colorLookup.contribution.value = Config.colorAdjustments_Contribution;
            }
            if (volume.profile.TryGet(out DepthOfField depthOfField))
            {
                depthOfField.active = Config.depthOfField_IsActive;
                depthOfField.mode.value = Config.depthOfField_Mode;
                depthOfField.gaussianStart.value = Config.depthOfField_GaussianStart;
                depthOfField.gaussianEnd.value = Config.depthOfField_GaussianEnd;
                depthOfField.gaussianMaxRadius.value = Config.depthOfField_GaussianMaxRadius;
                depthOfField.highQualitySampling.value = Config.depthOfField_HighQualitySampling;
                depthOfField.focusDistance.value = Config.depthOfField_FocusDistance;
                depthOfField.aperture.value = Config.depthOfField_Aperture;
                depthOfField.focalLength.value = Config.depthOfField_FocalLength;
                depthOfField.bladeCount.value = Config.depthOfField_BladeCount;
                depthOfField.bladeCurvature.value = Config.depthOfField_BladeCurvature;
                depthOfField.bladeRotation.value = Config.depthOfField_BladeRotation;
            }
            if (volume.profile.TryGet(out FilmGrain filmGrain))
            {
                filmGrain.active = Config.filmGrain_IsActive;
                filmGrain.type.value = Config.filmGrain_Type;
                filmGrain.intensity.value = Config.filmGrain_Intensity;
                filmGrain.response.value = Config.filmGrain_Response;
            }
            if (volume.profile.TryGet(out LensDistortion lensDistortion))
            {
                lensDistortion.active = Config.lensDistortion_IsActive;
                lensDistortion.intensity.value = Config.lensDistortion_Intensity;
                lensDistortion.xMultiplier.value = Config.lensDistortion_XMultiplier;
                lensDistortion.yMultiplier.value = Config.lensDistortion_YMultiplier;
                lensDistortion.center.value = Config.lensDistortion_Center;
                lensDistortion.scale.value = Config.lensDistortion_Scale;
            }
            if (volume.profile.TryGet(out LiftGammaGain liftGammaGain))
            {
                liftGammaGain.active = Config.liftGammaGain_IsActive;
                liftGammaGain.lift.value = Config.liftGammaGain_Lift;
                liftGammaGain.gamma.value = Config.liftGammaGain_Gamma;
                liftGammaGain.gain.value = Config.liftGammaGain_Gain;
            }
            if (volume.profile.TryGet(out MotionBlur motionBlur))
            {
                motionBlur.active = Config.motionBlur_IsActive;
                motionBlur.mode.value = Config.motionBlur_Mode;
                motionBlur.quality.value = Config.motionBlur_Quality;
                motionBlur.intensity.value = Config.motionBlur_Intensity;
                motionBlur.clamp.value = Config.motionBlur_Clamp;
            }
            if (volume.profile.TryGet(out PaniniProjection paniniProjection))
            {
                paniniProjection.active = Config.paniniProjection_IsActive;
                paniniProjection.distance.value = Config.paniniProjection_Distance;
                paniniProjection.cropToFit.value = Config.paniniProjection_CropToFit;
            }
            if (volume.profile.TryGet(out ShadowsMidtonesHighlights shadowsMidtonesHighlights))
            {
                shadowsMidtonesHighlights.active = Config.shadowsMidtonesHighlights_IsActive;
                shadowsMidtonesHighlights.shadows.value = Config.shadowsMidtonesHighlights_Shadows;
                shadowsMidtonesHighlights.midtones.value = Config.shadowsMidtonesHighlights_Midtones;
                shadowsMidtonesHighlights.highlights.value = Config.shadowsMidtonesHighlights_Highlights;
                shadowsMidtonesHighlights.shadowsStart.value = Config.shadowsMidtonesHighlights_ShadowsStart;
                shadowsMidtonesHighlights.shadowsEnd.value = Config.shadowsMidtonesHighlights_ShadowsEnd;
                shadowsMidtonesHighlights.highlightsStart.value = Config.shadowsMidtonesHighlights_ShadowsStart;
                shadowsMidtonesHighlights.highlightsEnd.value = Config.shadowsMidtonesHighlights_HighlightsEnd;
            }
            if (volume.profile.TryGet(out SplitToning splitToning))
            {
                splitToning.active = Config.splitToning_IsActive;
                splitToning.shadows.value = Config.splitToning_Shadows;
                splitToning.highlights.value = Config.splitToning_Highlights;
                splitToning.balance.value = Config.splitToning_Balance;
            }
            if (volume.profile.TryGet(out Tonemapping tonemapping))
            {
                tonemapping.active = Config.tonemapping_IsActive;
                tonemapping.mode.value = Config.tonemapping_Mode;
            }
            if (volume.profile.TryGet(out Vignette vignette))
            {
                vignette.active = Config.vignette_IsActive;
                vignette.color.value = Config.vignette_Color;
                vignette.center.value = Config.vignette_Center;
                vignette.intensity.value = Config.vignette_Intensity;
                vignette.smoothness.value = Config.vignette_Smoothness;
                vignette.rounded.value = Config.vignette_Rounded;
            }
            if (volume.profile.TryGet(out WhiteBalance whiteBalance))
            {
                whiteBalance.active = Config.whiteBalance_IsActive;
                whiteBalance.temperature.value = Config.whiteBalance_Temperature;
                whiteBalance.tint.value = Config.whiteBalance_Tint;
            }
        }
        #endregion

        #region Editor
#if UNITY_EDITOR
        [ContextMenu("UpdateConfigUsingComponentData")]
        void EditorUpdateConfigUsingComponentData()
        {

            //——Bloom——
            if (volume.profile.TryGet(out Bloom bloom))
            {
                Config.bloom_IsActive = bloom.active;
                Config.bloom_Threshold = bloom.threshold.value;
                Config.bloom_Intensity = bloom.intensity.value;
                Config.bloom_Scatter = bloom.scatter.value;
                Config.bloom_Clamp = bloom.clamp.value;
                Config.bloom_Tint = bloom.tint.value;

                Config.bloom_DirtIntensity = bloom.dirtIntensity.value;
            }
            if (volume.profile.TryGet(out ChannelMixer channelMixer))
            {
                Config.channelMixer_IsActive = channelMixer.active;
                Config.channelMixer_RedOutRedIn = channelMixer.redOutRedIn.value;
                Config.channelMixer_RedOutGreenIn = channelMixer.redOutGreenIn.value;
                Config.channelMixer_RedOutBlueIn = channelMixer.redOutBlueIn.value;
                Config.channelMixer_GreenOutRedIn = channelMixer.greenOutRedIn.value;
                Config.channelMixer_GreenOutGreenIn = channelMixer.greenOutGreenIn.value;
                Config.channelMixer_GreenOutBlueIn = channelMixer.greenOutBlueIn.value;
                Config.channelMixer_BlueOutRedIn = channelMixer.blueOutRedIn.value;
                Config.channelMixer_BlueOutGreenIn = channelMixer.blueOutGreenIn.value;
                Config.channelMixer_BlueOutBlueIn = channelMixer.blueOutBlueIn.value;
            }
            if (volume.profile.TryGet(out ChromaticAberration chromaticAberration))
            {
                Config.chromaticAberration_IsActive = chromaticAberration.active;
                Config.chromaticAberration_Intensity = chromaticAberration.intensity.value;
            }
            if (volume.profile.TryGet(out ColorAdjustments colorAdjustments))
            {
                Config.colorAdjustments_IsActive = colorAdjustments.active;
                Config.colorAdjustments_PostExposure = colorAdjustments.postExposure.value;
                Config.colorAdjustments_Contrast = colorAdjustments.contrast.value;
                Config.colorAdjustments_ColorFilter = colorAdjustments.colorFilter.value;
                Config.colorAdjustments_HueShift = colorAdjustments.hueShift.value;
                Config.colorAdjustments_Saturation = colorAdjustments.saturation.value;
            }
            if (volume.profile.TryGet(out ColorLookup colorLookup))
            {
                Config.colorLookup_IsActive = colorLookup.active;
                Config.colorAdjustments_Contribution = colorLookup.contribution.value;
            }
            if (volume.profile.TryGet(out DepthOfField depthOfField))
            {
                Config.depthOfField_IsActive = depthOfField.active;
                Config.depthOfField_Mode = depthOfField.mode.value;
                Config.depthOfField_GaussianStart = depthOfField.gaussianStart.value;
                Config.depthOfField_GaussianEnd = depthOfField.gaussianEnd.value;
                Config.depthOfField_GaussianMaxRadius = depthOfField.gaussianMaxRadius.value;
                Config.depthOfField_HighQualitySampling = depthOfField.highQualitySampling.value;
                Config.depthOfField_FocusDistance = depthOfField.focusDistance.value;
                Config.depthOfField_Aperture = depthOfField.aperture.value;
                Config.depthOfField_FocalLength = depthOfField.focalLength.value;
                Config.depthOfField_BladeCount = depthOfField.bladeCount.value;
                Config.depthOfField_BladeCurvature = depthOfField.bladeCurvature.value;
                Config.depthOfField_BladeRotation = depthOfField.bladeRotation.value;
            }
            if (volume.profile.TryGet(out FilmGrain filmGrain))
            {
                Config.filmGrain_IsActive = filmGrain.active;
                Config.filmGrain_Type = filmGrain.type.value;
                Config.filmGrain_Intensity = filmGrain.intensity.value;
                Config.filmGrain_Response = filmGrain.response.value;
            }
            if (volume.profile.TryGet(out LensDistortion lensDistortion))
            {
                Config.lensDistortion_IsActive = lensDistortion.active;
                Config.lensDistortion_Intensity = lensDistortion.intensity.value;
                Config.lensDistortion_XMultiplier = lensDistortion.xMultiplier.value;
                Config.lensDistortion_YMultiplier = lensDistortion.yMultiplier.value;
                Config.lensDistortion_Center = lensDistortion.center.value;
                Config.lensDistortion_Scale = lensDistortion.scale.value;
            }
            if (volume.profile.TryGet(out LiftGammaGain liftGammaGain))
            {
                Config.liftGammaGain_IsActive = liftGammaGain.active;
                Config.liftGammaGain_Lift = liftGammaGain.lift.value;
                Config.liftGammaGain_Gamma = liftGammaGain.gamma.value;
                Config.liftGammaGain_Gain = liftGammaGain.gain.value;
            }
            if (volume.profile.TryGet(out MotionBlur motionBlur))
            {
                Config.motionBlur_IsActive = motionBlur.active;
                Config.motionBlur_Mode = motionBlur.mode.value;
                Config.motionBlur_Quality = motionBlur.quality.value;
                Config.motionBlur_Intensity = motionBlur.intensity.value;
                Config.motionBlur_Clamp = motionBlur.clamp.value;
            }
            if (volume.profile.TryGet(out PaniniProjection paniniProjection))
            {
                Config.paniniProjection_IsActive = paniniProjection.active;
                Config.paniniProjection_Distance = paniniProjection.distance.value;
                Config.paniniProjection_CropToFit = paniniProjection.cropToFit.value;
            }
            if (volume.profile.TryGet(out ShadowsMidtonesHighlights shadowsMidtonesHighlights))
            {
                Config.shadowsMidtonesHighlights_IsActive = shadowsMidtonesHighlights.active;
                Config.shadowsMidtonesHighlights_Shadows = shadowsMidtonesHighlights.shadows.value;
                Config.shadowsMidtonesHighlights_Midtones = shadowsMidtonesHighlights.midtones.value;
                Config.shadowsMidtonesHighlights_Highlights = shadowsMidtonesHighlights.highlights.value;
                Config.shadowsMidtonesHighlights_ShadowsStart = shadowsMidtonesHighlights.shadowsStart.value;
                Config.shadowsMidtonesHighlights_ShadowsEnd = shadowsMidtonesHighlights.shadowsEnd.value;
                Config.shadowsMidtonesHighlights_ShadowsStart = shadowsMidtonesHighlights.highlightsStart.value;
                Config.shadowsMidtonesHighlights_HighlightsEnd = shadowsMidtonesHighlights.highlightsEnd.value;
            }
            if (volume.profile.TryGet(out SplitToning splitToning))
            {
                Config.splitToning_IsActive = splitToning.active;
                Config.splitToning_Shadows = splitToning.shadows.value;
                Config.splitToning_Highlights = splitToning.highlights.value;
                Config.splitToning_Balance = splitToning.balance.value;
            }
            if (volume.profile.TryGet(out Tonemapping tonemapping))
            {
                Config.tonemapping_IsActive = tonemapping.active;
                Config.tonemapping_Mode = tonemapping.mode.value;
            }
            if (volume.profile.TryGet(out Vignette vignette))
            {
                Config.vignette_IsActive = vignette.active;
                Config.vignette_Color = vignette.color.value;
                Config.vignette_Center = vignette.center.value;
                Config.vignette_Intensity = vignette.intensity.value;
                Config.vignette_Smoothness = vignette.smoothness.value;
                Config.vignette_Rounded = vignette.rounded.value;
            }
            if (volume.profile.TryGet(out WhiteBalance whiteBalance))
            {
                Config.whiteBalance_IsActive = whiteBalance.active;
                Config.whiteBalance_Temperature = whiteBalance.temperature.value;
                Config.whiteBalance_Tint = whiteBalance.tint.value;
            }

        }
#endif
        #endregion

        #region Define
        [Serializable]
        public class ConfigInfo : SerializableComponentConfigInfoBase
        {
            public bool isUsePostProcessing = true;

            #region Constructor
            [JsonConstructor]
            public ConfigInfo() { }
            #endregion

            #region Bloom
            [Header("Bloom")]
            [EnableIf(nameof(isUsePostProcessing))][AllowNesting] public bool bloom_IsActive = false;
            [Tooltip("Filters out pixels under this level of brightness. Value is in gamma-space.")]
            [ShowIf(nameof(isBloomValid))][AllowNesting] public float bloom_Threshold = 0.9f;
            [Tooltip("Strength of the bloom filter.")]
            [ShowIf(nameof(isBloomValid))][AllowNesting] public float bloom_Intensity = 0f;
            [Tooltip("Set the radius of the bloom effect")]
            [ShowIf(nameof(isBloomValid))][AllowNesting][Range(0, 1)] public float bloom_Scatter = 0.1f;
            [Tooltip("Use the color picker to select a color for the Bloom effect to tint to.")]
            [ShowIf(nameof(isBloomValid))][AllowNesting] public Color bloom_Tint = Color.white;
            [Tooltip("Set the maximum intensity that Unity uses to calculate Bloom. If pixels in your Scene are more intense than this, URP renders them at their current intensity, but uses this intensity value for the purposes of Bloom calculations.")]
            [ShowIf(nameof(isBloomValid))][AllowNesting] public float bloom_Clamp = 65472f;
            //——Lens Dirt——
            //Texture bloom_DirtTexture//图片格式没有限制，目前暂时没应用到，等后期加上
            [Tooltip("Amount of dirtiness.")]
            [ShowIf(nameof(isBloomValid))][AllowNesting] public float bloom_DirtIntensity = 0f;
            #endregion

            #region ChannelMixer
            //——PS：（Inspector中，每个Tab代表原颜色，下方三个颜色分量代表要映射的颜色，默认是映射到自身的颜色（值为100））——
            [Header("ChannelMixer")]
            [EnableIf(nameof(isUsePostProcessing))][AllowNesting] public bool channelMixer_IsActive = false;
            [Tooltip("Modify influence of the red channel in the overall mix.")]
            [ShowIf(nameof(isChannelMixerValid))][AllowNesting][Range(-200, 200)] public float channelMixer_RedOutRedIn = 100f;
            [Tooltip("Modify influence of the green channel in the overall mix.")]
            [ShowIf(nameof(isChannelMixerValid))][AllowNesting][Range(-200, 200)] public float channelMixer_RedOutGreenIn = 0f;
            [Tooltip("Modify influence of the blue channel in the overall mix.")]
            [ShowIf(nameof(isChannelMixerValid))][AllowNesting][Range(-200, 200)] public float channelMixer_RedOutBlueIn = 0f;

            [Tooltip("Modify influence of the red channel in the overall mix.")]
            [ShowIf(nameof(isChannelMixerValid))][AllowNesting][Range(-200, 200)] public float channelMixer_GreenOutRedIn = 0f;
            [Tooltip("Modify influence of the green channel in the overall mix.")]
            [ShowIf(nameof(isChannelMixerValid))][AllowNesting][Range(-200, 200)] public float channelMixer_GreenOutGreenIn = 100f;
            [Tooltip("Modify influence of the blue channel in the overall mix.")]
            [ShowIf(nameof(isChannelMixerValid))][AllowNesting][Range(-200, 200)] public float channelMixer_GreenOutBlueIn = 0f;

            [Tooltip("Modify influence of the red channel in the overall mix.")]
            [ShowIf(nameof(isChannelMixerValid))][AllowNesting][Range(-200, 200)] public float channelMixer_BlueOutRedIn = 0f;
            [Tooltip("Modify influence of the green channel in the overall mix.")]
            [ShowIf(nameof(isChannelMixerValid))][AllowNesting][Range(-200, 200)] public float channelMixer_BlueOutGreenIn = 0f;
            [Tooltip("Modify influence of the blue channel in the overall mix.")]
            [ShowIf(nameof(isChannelMixerValid))][AllowNesting][Range(-200, 200)] public float channelMixer_BlueOutBlueIn = 100f;
            #endregion

            #region ChromaticAberration
            [Header("ChromaticAberration")]
            [EnableIf(nameof(isUsePostProcessing))][AllowNesting] public bool chromaticAberration_IsActive = false;
            [Tooltip("Set the strength of the Chromatic Aberration effect.")]
            [ShowIf(nameof(isChromaticAberrationValid))][AllowNesting][Range(0, 1)] public float chromaticAberration_Intensity = 0f;
            #endregion

            #region ColorAdjustments
            [Header("ColorAdjustments")]
            [EnableIf(nameof(isUsePostProcessing))][AllowNesting] public bool colorAdjustments_IsActive = false;
            [Tooltip("Adjusts the overall exposure of the scene in EV100. This is applied after HDR effect and right before tonemapping so it won't affect previous effects in the chain.")]
            [ShowIf(nameof(isColorAdjustmentsValid))][AllowNesting] public float colorAdjustments_PostExposure = 0f;
            [Tooltip("Expands or shrinks the overall range of tonal values.")]
            [ShowIf(nameof(isColorAdjustmentsValid))][AllowNesting][Range(-100, 100)] public float colorAdjustments_Contrast = 0f;
            [Tooltip("Tint the render by multiplying a color.")]
            [ShowIf(nameof(isColorAdjustmentsValid))][AllowNesting][ColorUsage(true)] public Color colorAdjustments_ColorFilter = Color.white;
            [Tooltip("Shift the hue of all colors.")]
            [ShowIf(nameof(isColorAdjustmentsValid))][AllowNesting][Range(-180, 180)] public float colorAdjustments_HueShift = 0f;
            [Tooltip("Pushes the intensity of all colors.")]
            [ShowIf(nameof(isColorAdjustmentsValid))][AllowNesting][Range(-100, 100)] public float colorAdjustments_Saturation = 0f;
            #endregion

            #region ColorCurves
            //ToAdd:Curve相关
            #endregion

            #region ColorLookup
            [Header("ColorLookup")]
            [EnableIf(nameof(isUsePostProcessing))][AllowNesting] public bool colorLookup_IsActive = false;
            //Texture colorLookup_Texture//Warning:【需要特定格式的贴图】，如果设置为其他图片会报警告：Invalid lookup texture. It must be a non-sRGB 2D texture or render texture with the same size as set in the Universal Render Pipeline settings.
            [Tooltip("How much of the lookup texture will contribute to the color grading effect.")]
            [ShowIf(nameof(isColorLookupValid))][AllowNesting][Range(0, 1)] public float colorAdjustments_Contribution = 0f;
            #endregion

            #region DepthOfField
            [Header("DepthOfField")]
            [EnableIf(nameof(isUsePostProcessing))][AllowNesting] public bool depthOfField_IsActive = false;
            [Tooltip("Use \"Gaussian\" for a faster but non physical depth of field; \"Bokeh\" for a more realistic but slower depth of field.")]
            [ShowIf(nameof(isDepthOfFieldValid))][AllowNesting] public DepthOfFieldMode depthOfField_Mode = DepthOfFieldMode.Off;
            //——Gaussian——
            [Tooltip("The distance at which the blurring will start.")]
            [ShowIf(nameof(isDepthOfFieldValidAndGaussianMode))][AllowNesting] public float depthOfField_GaussianStart = 10;
            [Tooltip("The distance at which the blurring will reach its maximum radius.")]
            [ShowIf(nameof(isDepthOfFieldValidAndGaussianMode))][AllowNesting] public float depthOfField_GaussianEnd = 30;
            [Tooltip("The maximum radius of the gaussian blur. Values above 1 may show under-sampling artifacts.")]
            [ShowIf(nameof(isDepthOfFieldValidAndGaussianMode))][AllowNesting][Range(0.5f, 1.5f)] public float depthOfField_GaussianMaxRadius = 1;
            [Tooltip("Use higher quality sampling to reduce flickering and improve the overall blur smoothness.")]
            [ShowIf(nameof(isDepthOfFieldValidAndGaussianMode))][AllowNesting] public bool depthOfField_HighQualitySampling = false;
            //——Bokeh——
            [Tooltip("The distance to the point of focus.")]
            [ShowIf(nameof(isDepthOfFieldValidAndBokehMode))][AllowNesting] public float depthOfField_FocusDistance = 10;
            [Tooltip("The ratio of aperture (known as f-stop or f-number). The smaller the value is, the shallower the depth of field is.")]
            [ShowIf(nameof(isDepthOfFieldValidAndBokehMode))][AllowNesting][Range(1f, 32f)] public float depthOfField_Aperture = 5.6f;
            [Tooltip("The distance between the lens and the film. The larger the value is, the shallower the depth of field is.")]
            [ShowIf(nameof(isDepthOfFieldValidAndBokehMode))][AllowNesting][Range(1f, 300f)] public float depthOfField_FocalLength = 50f;
            [Tooltip("The number of aperture blades.")]
            [ShowIf(nameof(isDepthOfFieldValidAndBokehMode))][AllowNesting][Range(3, 9)] public int depthOfField_BladeCount = 5;
            [Tooltip("The curvature of aperture blades. The smaller the value is, the more visible aperture blades are. A value of 1 will make the bokeh perfectly circular.")]
            [ShowIf(nameof(isDepthOfFieldValidAndBokehMode))][AllowNesting][Range(0f, 1f)] public float depthOfField_BladeCurvature = 1f;
            [Tooltip("The rotation of aperture blades in degrees.")]
            [ShowIf(nameof(isDepthOfFieldValidAndBokehMode))][AllowNesting][Range(-180f, 180f)] public float depthOfField_BladeRotation = 0f;
            #endregion

            #region FilmGrain
            [Header("FilmGrain")]
            [EnableIf(nameof(isUsePostProcessing))][AllowNesting] public bool filmGrain_IsActive = false;
            [Tooltip("The type of grain to use. You can select a preset or provide your own texture by selecting Custom.")]
            [ShowIf(nameof(isFilmGrainValid))][AllowNesting] public FilmGrainLookup filmGrain_Type = FilmGrainLookup.Thin1;
            [Tooltip("Use the slider to set the strength of the Film Grain effect.")]
            [ShowIf(nameof(isFilmGrainValid))][AllowNesting][Range(0f, 1f)] public float filmGrain_Intensity = 0;
            [Tooltip("Controls the noisiness response curve based on scene luminance. Higher values mean less noise in light areas.")]
            [ShowIf(nameof(isFilmGrainValid))][AllowNesting][Range(0f, 1f)] public float filmGrain_Response = 0.8f;
            //public Texture filmGrain_Texture//自定义贴图。Warning:【需要特定格式的贴图】
            #endregion

            #region LensDistortion
            [Header("LensDistortion")]
            [EnableIf(nameof(isUsePostProcessing))][AllowNesting] public bool lensDistortion_IsActive = false;
            [Tooltip("Total distortion amount.")]
            [ShowIf(nameof(isLensDistortionValid))][AllowNesting][Range(-1f, 1f)] public float lensDistortion_Intensity = 0f;
            [Tooltip("Intensity multiplier on X axis. Set it to 0 to disable distortion on this axis.")]
            [ShowIf(nameof(isLensDistortionValid))][AllowNesting][Range(0f, 1f)] public float lensDistortion_XMultiplier = 1f;
            [Tooltip("Intensity multiplier on Y axis. Set it to 0 to disable distortion on this axis.")]
            [ShowIf(nameof(isLensDistortionValid))][AllowNesting][Range(0f, 1f)] public float lensDistortion_YMultiplier = 1f;
            [Tooltip("Distortion center point. 0.5,0.5 is center of the screen")]
            [ShowIf(nameof(isLensDistortionValid))][AllowNesting] public Vector2 lensDistortion_Center = new Vector2(0.5f, 0.5f);
            [Tooltip("Controls global screen scaling for the distortion effect. Use this to hide screen borders when using high \"Intensity.\"")]
            [ShowIf(nameof(isLensDistortionValid))][AllowNesting][Range(0.01f, 5f)] public float lensDistortion_Scale = 1f;
            #endregion

            #region LiftGammaGain
            [Header("LiftGammaGain")]
            [EnableIf(nameof(isUsePostProcessing))][AllowNesting] public bool liftGammaGain_IsActive = false;
            [Tooltip("Use this to control and apply a hue to the dark tones. This has a more exaggerated effect on shadows.")]
            [ShowIf(nameof(isLiftGammaGainValid))][AllowNesting] public Vector4 liftGammaGain_Lift = new Vector4(1f, 1f, 1f, 0f);
            [Tooltip("Use this to control and apply a hue to the mid-range tones with a power function.")]
            [ShowIf(nameof(isLiftGammaGainValid))][AllowNesting] public Vector4 liftGammaGain_Gamma = new Vector4(1f, 1f, 1f, 0f);
            [Tooltip("Use this to increase and apply a hue to the signal and make highlights brighter.")]
            [ShowIf(nameof(isLiftGammaGainValid))][AllowNesting] public Vector4 liftGammaGain_Gain = new Vector4(1f, 1f, 1f, 0f);
            #endregion

            #region MotionBlur
            [Header("MotionBlur")]
            [EnableIf(nameof(isUsePostProcessing))][AllowNesting] public bool motionBlur_IsActive = false;
            [Tooltip("The motion blur technique to use. If you don't need object motion blur, CameraOnly will result in better performance.")]
            [ShowIf(nameof(isMotionBlurValid))][AllowNesting] public MotionBlurMode motionBlur_Mode = MotionBlurMode.CameraOnly;
            [Tooltip("The quality of the effect. Lower presets will result in better performance at the expense of visual quality.")]
            [ShowIf(nameof(isMotionBlurValid))][AllowNesting] public MotionBlurQuality motionBlur_Quality = MotionBlurQuality.Low;
            [Tooltip("The strength of the motion blur filter. Acts as a multiplier for velocities.")]
            [ShowIf(nameof(isMotionBlurValid))][AllowNesting][Range(0f, 1f)] public float motionBlur_Intensity = 0f;
            [Tooltip("Sets the maximum length, as a fraction of the screen's full resolution, that the velocity resulting from Camera rotation can have. Lower values will improve performance.")]
            [ShowIf(nameof(isMotionBlurValid))][AllowNesting][Range(0f, 0.2f)] public float motionBlur_Clamp = 0.05f;
            #endregion

            #region PaniniProjection
            [Header("PaniniProjection")]
            [EnableIf(nameof(isUsePostProcessing))][AllowNesting] public bool paniniProjection_IsActive = false;
            [Tooltip("Panini projection distance.")]
            [ShowIf(nameof(isPaniniProjectionValid))][AllowNesting][Range(0f, 1f)] public float paniniProjection_Distance = 0f;
            [Tooltip("Panini projection crop to fit.")]
            [ShowIf(nameof(isPaniniProjectionValid))][AllowNesting][Range(0f, 1f)] public float paniniProjection_CropToFit = 1f;
            #endregion

            #region ShadowsMidtonesHighlights
            [Header("ShadowsMidtonesHighlights")]
            [EnableIf(nameof(isUsePostProcessing))][AllowNesting] public bool shadowsMidtonesHighlights_IsActive = false;
            [Tooltip("Use this to control and apply a hue to the shadows.")]
            [ShowIf(nameof(isShadowsMidtonesHighlightsValid))][AllowNesting] public Vector4 shadowsMidtonesHighlights_Shadows = new Vector4(1f, 1f, 1f, 0f);
            [Tooltip("Use this to control and apply a hue to the midtones.")]
            [ShowIf(nameof(isShadowsMidtonesHighlightsValid))][AllowNesting] public Vector4 shadowsMidtonesHighlights_Midtones = new Vector4(1f, 1f, 1f, 0f);
            [Tooltip("Use this to control and apply a hue to the highlights.")]
            [ShowIf(nameof(isShadowsMidtonesHighlightsValid))][AllowNesting] public Vector4 shadowsMidtonesHighlights_Highlights = new Vector4(1f, 1f, 1f, 0f);
            //——Shadow Limits——
            [Tooltip("Start point of the transition between shadows and midtones.")]
            [ShowIf(nameof(isShadowsMidtonesHighlightsValid))][AllowNesting] public float shadowsMidtonesHighlights_ShadowsStart = 0f;
            [Tooltip("End point of the transition between shadows and midtones.")]
            [ShowIf(nameof(isShadowsMidtonesHighlightsValid))][AllowNesting] public float shadowsMidtonesHighlights_ShadowsEnd = 0.3f;
            //——Highlight Limits——
            [Tooltip("Start point of the transition between midtones and highlights.")]
            [ShowIf(nameof(isShadowsMidtonesHighlightsValid))][AllowNesting] public float shadowsMidtonesHighlights_HighlightsStart = 0.55f;
            [Tooltip("End point of the transition between midtones and highlights.")]
            [ShowIf(nameof(isShadowsMidtonesHighlightsValid))][AllowNesting] public float shadowsMidtonesHighlights_HighlightsEnd = 1f;
            #endregion

            #region SplitToning
            [Header("SplitToning")]
            [EnableIf(nameof(isUsePostProcessing))][AllowNesting] public bool splitToning_IsActive = false;
            [Tooltip("The color to use for shadows.")]
            [ShowIf(nameof(isSplitToningValid))][AllowNesting] public Color splitToning_Shadows = Color.grey;
            [Tooltip("The color to use for highlights.")]
            [ShowIf(nameof(isSplitToningValid))][AllowNesting] public Color splitToning_Highlights = Color.grey;
            [Tooltip("Balance between the colors in the highlights and shadows.")]
            [ShowIf(nameof(isSplitToningValid))][AllowNesting][Range(-100f, 100f)] public float splitToning_Balance = 0f;
            #endregion

            #region Tonemapping
            [Header("Tonemapping")]
            [EnableIf(nameof(isUsePostProcessing))][AllowNesting] public bool tonemapping_IsActive = false;
            [Tooltip("Select a tonemapping algorithm to use for the color grading process.")]
            [ShowIf(nameof(isTonemappingValid))][AllowNesting] public TonemappingMode tonemapping_Mode = TonemappingMode.None;
            #endregion

            #region Vignette
            [Header("Vignette")]
            [EnableIf(nameof(isUsePostProcessing))][AllowNesting] public bool vignette_IsActive = false;
            [Tooltip("Vignette color.")]
            [ShowIf(nameof(isVignetteValid))][AllowNesting] public Color vignette_Color = Color.black;
            [Tooltip("Sets the vignette center point (screen center is [0.5,0.5]).")]
            [ShowIf(nameof(isVignetteValid))][AllowNesting] public Vector2 vignette_Center = new Vector2(0.5f, 0.5f);
            [Tooltip("Amount of vignetting on screen.")]
            [ShowIf(nameof(isVignetteValid))][AllowNesting][Range(0, 1)] public float vignette_Intensity = 0f;
            [Tooltip("Smoothness of the vignette borders.")]
            [ShowIf(nameof(isVignetteValid))][AllowNesting][Range(0.01f, 1f)] public float vignette_Smoothness = 0.2f;
            [Tooltip("Should the vignette be perfectly round or be dependent on the current aspect ratio?")]
            [ShowIf(nameof(isVignetteValid))][AllowNesting] public bool vignette_Rounded = false;
            #endregion

            #region WhiteBalance
            [Header("WhiteBalance")]
            [EnableIf(nameof(isUsePostProcessing))][AllowNesting] public bool whiteBalance_IsActive = false;
            [Tooltip("Sets the white balance to a custom color temperature.")]
            [ShowIf(nameof(isWhiteBalanceValid))][AllowNesting][Range(-100, 100f)] public float whiteBalance_Temperature = 0f;
            [Tooltip("Sets the white balance to compensate for a green or magenta tint.")]
            [ShowIf(nameof(isWhiteBalanceValid))][AllowNesting][Range(-100, 100f)] public float whiteBalance_Tint = 0f;
            #endregion

            #region NaughtAttribute
            bool isBloomValid { get { return isUsePostProcessing && bloom_IsActive; } }
            bool isChannelMixerValid { get { return isUsePostProcessing && channelMixer_IsActive; } }
            bool isChromaticAberrationValid { get { return isUsePostProcessing && chromaticAberration_IsActive; } }
            bool isColorAdjustmentsValid { get { return isUsePostProcessing && colorAdjustments_IsActive; } }
            bool isColorLookupValid { get { return isUsePostProcessing && colorLookup_IsActive; } }
            bool isDepthOfFieldValid { get { return isUsePostProcessing && depthOfField_IsActive; } }
            bool isDepthOfFieldValidAndGaussianMode { get { return isDepthOfFieldValid && depthOfField_Mode == DepthOfFieldMode.Gaussian; } }
            bool isDepthOfFieldValidAndBokehMode { get { return isDepthOfFieldValid && depthOfField_Mode == DepthOfFieldMode.Bokeh; } }
            bool isFilmGrainValid { get { return isUsePostProcessing && filmGrain_IsActive; } }
            bool isLensDistortionValid { get { return isUsePostProcessing && lensDistortion_IsActive; } }
            bool isLiftGammaGainValid { get { return isUsePostProcessing && liftGammaGain_IsActive; } }
            bool isMotionBlurValid { get { return isUsePostProcessing && motionBlur_IsActive; } }
            bool isPaniniProjectionValid { get { return isUsePostProcessing && paniniProjection_IsActive; } }
            bool isShadowsMidtonesHighlightsValid { get { return isUsePostProcessing && shadowsMidtonesHighlights_IsActive; } }
            bool isSplitToningValid { get { return isUsePostProcessing && splitToning_IsActive; } }
            bool isTonemappingValid { get { return isUsePostProcessing && tonemapping_IsActive; } }
            bool isVignetteValid { get { return isUsePostProcessing && vignette_IsActive; } }
            bool isWhiteBalanceValid { get { return isUsePostProcessing && whiteBalance_IsActive; } }
            #endregion
        }

        public class PropertyBag : ConfigurableComponentPropertyBagBase<PostProcessingController, ConfigInfo> { }
        #endregion
    }
}