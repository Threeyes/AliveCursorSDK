using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
/// <summary>
/// 根据是否包含某个参数，从而决定激活某些组件或功能
/// </summary>
public class CompanyConfigParamHelper : MonoBehaviour
{
    public string paramName;//需要的参数

    public Toggle.ToggleEvent onIsHaveParam;
    public UnityEvent onHaveParam;
    public UnityEvent onDontHaveParam;
    private void Awake()
    {
        Init();
    }

    void Init()
    {
        try
        {
            if (paramName.IsNullOrEmpty())
            {
                Debug.LogError("paramName is Null!");
                return;
            }
            SOCompanyConfig sOCompanyConfig = SOBuildInfoManager.Instance.curCompanyConfig;
            if (sOCompanyConfig)
            {
                bool ishasParam = sOCompanyConfig.HasParam(paramName);
                onIsHaveParam.Invoke(ishasParam);
                if (ishasParam)
                    onHaveParam.Invoke();
                else
                    onDontHaveParam.Invoke();
            }
            else
            {
                Debug.LogError("curCompanyConfig is Null!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("CompanyConfigParamHelper错误:\r\n" + e);
        }
    }
}
