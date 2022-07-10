using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

public class CompanyConfigTextureHelper : CompanyConfigHelperBase<TextureEvent, Texture>
{
    protected override Texture TryGetAsset()
    {
        SOCompanyConfig sOCompanyConfig = SOBuildInfoManager.Instance.curCompanyConfig;
        if (sOCompanyConfig)
            return sOCompanyConfig.TryGetTextureAsset(targetName);
        return null;
    }
}