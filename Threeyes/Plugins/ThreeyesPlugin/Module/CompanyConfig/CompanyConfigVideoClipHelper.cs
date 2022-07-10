using UnityEngine.Events;
using UnityEngine.Video;

public class CompanyConfigVideoClipHelper : CompanyConfigHelperBase<CompanyConfigVideoClipHelper.VideoClipEvent, VideoClip>
{
    protected override VideoClip TryGetAsset()
    {
        SOCompanyConfig sOCompanyConfig = SOBuildInfoManager.Instance.curCompanyConfig;
        if (sOCompanyConfig)
            return sOCompanyConfig.TryGetVideoClipAsset(targetName);
        return null;
    }

    [System.Serializable]
    public class VideoClipEvent : UnityEvent<VideoClip>
    {

    }
}
