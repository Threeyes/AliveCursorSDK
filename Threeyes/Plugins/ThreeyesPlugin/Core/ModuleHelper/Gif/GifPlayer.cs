using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Threeyes.Decoder;
using UnityEngine;
/// <summary>
/// A Simple gif player
/// </summary>
public class GifPlayer : MonoBehaviour
{
    public GifDecoder.DecodeOption decodeOption;
    public List<GifFrameData> listData = new List<GifFrameData>();
    public TextureEvent onUpdateTexture;

    bool isSelfToken = false;
    CancellationTokenSource selfCancellationTokenSource;
    CancellationTokenRegistration ctr;
    int curIndex = 0;
    float lastUpdateTime = 0;
    public void Init(byte[] arrByteGif)
    {
        var listData = arrByteGif.ToListGifFrameData(decodeOption);
        Init(listData);
    }

    /// <summary>
    /// PS：返回值为void才能让UnityEvent调用
    /// </summary>
    /// <param name="arrByteGif"></param>
    public void BeginInitAsync(byte[] arrByteGif)
    {
        InitAsync(arrByteGif);
    }
    public async void InitAsync(byte[] arrByteGif)
    {
        await InitAsync(arrByteGif, CancellationToken.None);
    }

    public async Task InitAsync(byte[] arrByteGif, CancellationToken cancellationToken)
    {
        Reset();
        if (cancellationToken != CancellationToken.None)
        {
            using CancellationTokenRegistration ctr = cancellationToken.Register(Reset);//监听外部取消事件，然后调用selfCancellationTokenSource.Cancel (好处是cancellationTokenSource被销毁也不会影响本地访问）
        }
        selfCancellationTokenSource = new CancellationTokenSource();
        var listDecodedData = await arrByteGif.ToListGifFrameDataAsync(selfCancellationTokenSource.Token, decodeOption);
        if (selfCancellationTokenSource.IsCancellationRequested)//被取消
        {
            listDecodedData.ForEach(gfd => gfd?.Dispose());//删掉加载的图片
            return;
        }
        this.listData = listDecodedData;
    }
    public void Init(List<GifFrameData> listData)
    {
        Reset();
        this.listData = listData;
    }

    public void Reset()
    {
        ResetData();
        ResetUI();
    }
    public void ResetData()
    {
        if (selfCancellationTokenSource != null && !selfCancellationTokenSource.IsCancellationRequested)
            selfCancellationTokenSource.Cancel();//停止加载图片
        if (listData.Count > 0)
        {
            foreach (var d in listData)
            {
                //PS：立即销毁临时生成的Texture，避免大量内存占用
                //ToAdd:增加isDispose字段，如果用户使用了Assets内部的Texture，那就不需要销毁图像
                d?.Dispose();
            }
            listData.Clear();
        }
        curIndex = 0;
        selfCancellationTokenSource?.Dispose();
    }
    public void ResetUI()
    {
        onUpdateTexture.Invoke(null);//清空显示
    }
    void OnDestroy()
    {
        Reset();
    }

    void Update()
    {
        if (listData.Count == 0)
            return;
        float curDelaySeconds = listData[curIndex].frameDelaysSeconds;
        if (Time.time - lastUpdateTime > curDelaySeconds)
        {
            curIndex = (curIndex + 1) % listData.Count;
            onUpdateTexture.Invoke(listData[curIndex].texture);
            lastUpdateTime = Time.time;
        }
    }
}
