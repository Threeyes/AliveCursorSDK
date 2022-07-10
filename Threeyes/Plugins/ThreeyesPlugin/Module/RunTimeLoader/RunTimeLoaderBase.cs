using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class RunTimeLoaderBase : MonoBehaviour
{
    public bool isLoadOnAwake = true;
    public bool isHideOnLoadFailed = false;//加载失败后隐藏（适用于可以隐藏的物体）
    public ExternalFileLocation externalFileLocation = ExternalFileLocation.StreamingAsset;

    [Tooltip("在StreamingAsset下的路径")]
    public string filePath;// eg: UI\Logo\XXX.png
    public string OriginAssetPath { get { return Application.streamingAssetsPath + "/" + filePath; } }//本地文件路径
    public string AssetPath//通过WWW加载的路径
    {
        get
        {
            string result = OriginAssetPath;
#if !UNITY_EDITOR && UNITY_ANDROID//PS:安卓端的streamingAssetsPath自带file://

#else
            result = "file://" + result;
#endif
            return result;
        }
    }//通过WWW加载的路径
}

public abstract class RunTimeLoaderBase<TAsset> : RunTimeLoaderBase
    where TAsset : class
{
    public UnityAction<TAsset> actLoadAssetSuccess;

    protected virtual void Awake()
    {
        if (isLoadOnAwake)
            LoadAsset();
    }

    public virtual void LoadAsset()
    {
        StartCoroutine(IELoadAsset(AssetPath));
    }
    public virtual void LoadAsset(UnityAction<TAsset> actOnLoadSuc, UnityAction<WWW> actOnLoadFailed = null)
    {
        StartCoroutine(IELoadAsset(AssetPath, actOnLoadSuc, actOnLoadFailed));
    }

    protected abstract TAsset GetAssetFunc(WWW www);

    protected virtual void SetAssetFunc(TAsset asset, UnityAction<TAsset> actOnLoadSucExter = null)
    {
        if (actLoadAssetSuccess != null)
            actLoadAssetSuccess.Invoke(asset);

        if (actOnLoadSucExter != null)
            actOnLoadSucExter.Invoke(asset);
    }

    public IEnumerator IELoadAsset(UnityAction<TAsset> actOnLoadSuc = null, UnityAction<WWW> actOnLoadFailed = null)
    {
        return IELoadAsset(AssetPath, actOnLoadSuc, actOnLoadFailed);
    }
    protected IEnumerator IELoadAsset(string path, UnityAction<TAsset> actOnLoadSucExter = null, UnityAction<WWW> actOnLoadFailed = null)
    {
        WWW www = new WWW(path);
        yield return www;
        if (www.error.IsNullOrEmpty())
        {
            OnLoadSuccess(www, actOnLoadSucExter);
        }
        else
        {
            OnLoadFailed(www, actOnLoadFailed);
            Debug.LogError("读取文件失败!\r\n" + www.error);
        }
    }

    protected virtual void OnLoadSuccess(WWW www, UnityAction<TAsset> actOnLoadSucExter = null)
    {
        TAsset asset = GetAssetFunc(www);
        if (asset != null)
            SetAssetFunc(asset, actOnLoadSucExter);
        else
        {
            Debug.LogError("无法获取资源！");
        }
    }

    protected virtual void OnLoadFailed(WWW www, UnityAction<WWW> actOnLoadFailed = null)
    {
        Debug.LogError("读取资源失败： " + www.error + "\r\n" + "读取位置：" + AssetPath);
        if (actOnLoadFailed != null)
            actOnLoadFailed(www);

        if (isHideOnLoadFailed)
            gameObject.SetActive(false);
    }

}
