using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
/// <summary>
/// 因为事件名称不统一，已经弃用
/// </summary>
[Obsolete("Use CompanyConfigTextureHelper instead")]
public class CompanyConfigTextureHelper_Obsolete : MonoBehaviour
{
    public string textureName;
    public TextureEvent onLoadTexSuccess;
    public UnityEvent onLoadTexFailed;

    /// <summary>
    /// 在Awake之后执行，以免出现被隐藏的情况
    /// </summary>
    private void Start()
    {
        Init();
    }

    void Init()
    {
        try
        {
            if (textureName.IsNullOrEmpty())
            {
                Debug.LogError("paramName is Null!");
                return;
            }
            SOCompanyConfig sOCompanyConfig = SOBuildInfoManager.Instance.curCompanyConfig;
            if (sOCompanyConfig)
            {
                Texture texResult = sOCompanyConfig.TryGetTextureAsset(textureName);
                if (texResult)
                    onLoadTexSuccess.Invoke(texResult);
                else
                    onLoadTexFailed.Invoke();
            }
            else
            {
                Debug.LogError("curCompanyConfig is Null!");
            }

        }
        catch (System.Exception e)
        {
            Debug.LogError("CompanyConfigTextureHelper错误:\r\n" + e);
        }
    }
}
