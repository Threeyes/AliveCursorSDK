#if USE_PostProcessing

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
/// <summary>
/// 暴露后期特效的指定属性，便于动画使用
/// </summary>
public class DepthOfFieldHelper : ComponentHelperBase<PostProcessingProfileHelper>
{
    public bool IsAvaliable
    { get { return Comp.profile; } }

    public bool IsActive
    {
        get
        {
            return Comp.profile.depthOfField.enabled;
        }
        set { Comp.profile.depthOfField.enabled = value; }
    }

    public float Aperture
    {
        get { return settings.aperture; }
        set
        {
            var newSettings = settings;
            newSettings.aperture = value;
            settings = newSettings;
        }
    }
    public float FocusDistance
    {
        get { return settings.focusDistance; }
        set
        {
            var newSettings = settings;
            newSettings.focusDistance = value;
            settings = newSettings;
        }
    }

    public DepthOfFieldModel.Settings settings
    {
        get { return IsAvaliable ? Comp.profile.depthOfField.settings : default(DepthOfFieldModel.Settings); }
        set
        {
            if (IsAvaliable)
                Comp.profile.depthOfField.settings = value;
        }
    }

}

#endif