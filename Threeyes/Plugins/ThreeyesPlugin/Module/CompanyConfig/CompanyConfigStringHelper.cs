using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CompanyConfigStringHelper : CompanyConfigHelperBase<StringEvent, string>
{
    protected override string TryGetAsset()
    {
        SOCompanyConfig sOCompanyConfig = SOBuildInfoManager.Instance.curCompanyConfig;
        if (sOCompanyConfig)
            return sOCompanyConfig.TryGetStringAsset(targetName);
        return null;
    }
}
