using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using Threeyes.Coroutine;
/// <summary>
/// 通过协程读取外部图像
/// </summary>
public class ExternalTextureLoaderManager : SingletonBase<ExternalTextureLoaderManager>
{
    /// <summary>
    /// 最多同时读取数量
    /// </summary>
    public int maxThread = 1;
    static List<IEnumerator> listWaitingEnumerator = new List<IEnumerator>();

    /// <summary>
    /// 使用协程加载图片
    /// </summary>
    /// <param name="path"></param>
    /// <param name="onLoadSuc"></param>
    /// <param name="onLoadFailed"></param>
    /// <returns></returns>
    public static IEnumerator LoadTextureAsync(string path, UnityAction<Texture> onLoadSuc, UnityAction<string> onLoadFailed = null)
    {
        IEnumerator enumerator = IELoadTexture(path, onLoadSuc, onLoadFailed);
        listWaitingEnumerator.Add(enumerator);

        TryStartLoadMainThread();
        return enumerator;
    }

    public static void StopLoad(IEnumerator enumerator)
    {
        if (listWaitingEnumerator.Contains(enumerator))
            listWaitingEnumerator.Remove(enumerator);
        CoroutineManager.Instance.StopCoroutine(enumerator);
    }

    static void TryStartLoadMainThread()
    {
        if (corMainThread == null)
            CoroutineManager.Instance.StartCoroutine(IELoadMainThread());
    }
    static Coroutine corMainThread;
    static IEnumerator IELoadMainThread()
    {
        //Todo:增加多线程的处理
        while (listWaitingEnumerator.Count > 0)
        {
            IEnumerator enumerator = listWaitingEnumerator[0];
            listWaitingEnumerator.RemoveAt(0);
            yield return CoroutineManager.Instance.StartCoroutine(enumerator);
        }
    }


    //
    /// <summary>
    /// PS:开发者使用上面的LoadTextureAsync，也可以手动调用这个方法
    /// </summary>
    /// <param name="path">文件路径，不需要file:///前缀</param>
    /// <param name="onLoadSuc"></param>
    /// <param name="onLoadFailed"></param>
    /// <returns></returns>
    public static IEnumerator IELoadTexture(string path, UnityAction<Texture> onLoadSuc, UnityAction<string> onLoadFailed)
    {
        string errMsg = "";
#if !UNITY_EDITOR && UNITY_ANDROID

        WWW www = new WWW(Application.streamingAssetsPath + "/" + path);
        yield return www;

        if (www.error.IsNullOrEmpty())
        {
            onLoadSuc.Execute(www.texture);
        }
        else
        {
            errMsg = "读取图片失败： " + www.error + "\r\n" + "读取位置：" + path;
            Debug.LogError(errMsg);
            onLoadFailed.Execute(errMsg);
        }
#else
        if (!File.Exists(path))
        {
            errMsg = "Texture File not Exists:\r\n" + path;
            Debug.LogWarning(errMsg);
            onLoadFailed.Execute(errMsg);
            yield break;
        }

        //Bug：以下代码可能会导致报错：（portXXX）
        //path = "file:///" + path;

        WWW www = new WWW(path);
        yield return www;

        if (www.error.IsNullOrEmpty())
        {
            onLoadSuc.Execute(www.texture);
        }
        else
        {
            errMsg = "读取图片失败： " + www.error + "\r\n" + "读取位置：" + path;
            Debug.LogError(errMsg);
            onLoadFailed.Execute(errMsg);
        }
#endif
    }


    public static Texture2D LoadTextureAtOnce(string path, UnityAction<string> onLoadFailed = null)
    {
        UnityAction<string> actionInteralFailed =
              (msg) =>
              {
                  Debug.LogWarning(msg);
                  onLoadFailed.Execute(msg);
              };

        if (!File.Exists(path))
        {
            actionInteralFailed.Execute("Texture File not Exists:\r\n" + path);
            return null;
        }

        try
        {
            byte[] byteArray = File.ReadAllBytes(path);
            Texture2D sampleTexture = new Texture2D(2, 2);
            // the size of the texture will be replaced by image size
            bool isLoaded = sampleTexture.LoadImage(byteArray);
            if (isLoaded)
            {
                return sampleTexture;
            }
            else
            {
                actionInteralFailed.Execute("Texture File Load Failed:\r\n" + path);
                return null;
            }
        }
        catch (System.Exception e)
        {
            actionInteralFailed.Execute("Texture File Load Error:\r\n" + e);
            return null;
        }
    }

    void LogError(string err)
    {

    }
}
