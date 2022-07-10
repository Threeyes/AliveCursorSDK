#if USE_PostProcessing

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

public class ChromaticAberrationHelper : ComponentHelperBase<PostProcessingProfileHelper>
{
    public bool IsAvaliable
    { get { return Comp.profile; } }

    public bool IsActive
    {
        get
        {
            return Comp.profile.chromaticAberration.enabled;
        }
        set { Comp.profile.chromaticAberration.enabled = value; }
    }

    public float Intensity
    {
        get
        {
            return settings.intensity;
        }

        set
        {
            var newSettings = settings;
            newSettings.intensity = value;
            settings = newSettings;
        }
    }

    public ChromaticAberrationModel.Settings settings
    {
        get { return IsAvaliable ? Comp.profile.chromaticAberration.settings : default(ChromaticAberrationModel.Settings); }
        set
        {
            if (IsAvaliable)
                Comp.profile.chromaticAberration.settings = value;
        }
    }


}

#endif