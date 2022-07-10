using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
/// <summary>
/// 程序运行时更新图片等资源，适用于为OEM公司加载不同的图片
/// 适用于：
/// ——UI.RawImage
/// ——3D.Renderer
/// </summary>
public class TextureRunTimeLoader : RunTimeLoaderBase<Texture>
{
    //以下组件，二选一
    public RawImage rawImage;
    public Renderer _renderer;
    protected override void Awake()
    {
        if (!rawImage)
            rawImage = GetComponent<RawImage>();

        if (!_renderer)
            _renderer = GetComponent<Renderer>();
        base.Awake();
    }

    protected override Texture GetAssetFunc(WWW www)
    {
        return www.texture;
    }
    protected override void SetAssetFunc(Texture asset, UnityAction<Texture> actOnLoadSucExter = null)
    {
        base.SetAssetFunc(asset, actOnLoadSucExter);

        if (rawImage)
        {
            rawImage.texture = asset;
        }
        else if (_renderer)
        {
            _renderer.material.mainTexture = asset;
        }
    }
}
