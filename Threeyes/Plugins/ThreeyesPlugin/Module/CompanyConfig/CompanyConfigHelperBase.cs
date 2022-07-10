using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 加载厂商的特定配置文件
/// </summary>
public abstract class CompanyConfigHelperBase<TEvent, T> : MonoBehaviour
    where TEvent : UnityEvent<T>

{
    public TEvent onLoadSuccess;
    public UnityEvent onLoadFailed;

    public string targetName;//需要的参数

    private void Awake()
    {
        Init();
    }

    protected virtual void Init()
    {
        try
        {
            if (targetName.IsNullOrEmpty())
            {
                Debug.LogError("targetName is Null!");
                return;
            }

            T result = TryGetAsset();
            if (result != null)
                onLoadSuccess.Invoke(result);
            else
            {
                Debug.Log("无法加载 " + targetName);
                onLoadFailed.Invoke();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("CompanyConfigHelper错误:\r\n" + e);
        }
    }

    protected abstract T TryGetAsset();
}
