using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// 公司的私人配置
/// </summary>
[CreateAssetMenu(menuName = EditorDefinition.AssetMenuPrefix_SO_Build + "CompanyConfig")]
public class SOCompanyConfig : ScriptableObject
{
    public List<TextureAsset> listTexAsset = new List<TextureAsset>();//图片资源
    public List<StringAsset> listStringAsset = new List<StringAsset>();//文本资源
    public List<VideoClipAsset> listVideoClipAsset = new List<VideoClipAsset>();//视频资源

    public List<string> listParam = new List<string>();//指定的参数
    [Multiline]
    public string description;//描述


    /// <summary>
    /// 尝试获取图片资源
    /// </summary>
    /// <param name="targetName"></param>
    /// <returns></returns>
    public Texture TryGetTextureAsset(string targetName)
    {
        return TryGetAsset<TextureAsset, Texture>(listTexAsset, targetName);
    }

    /// <summary>
    /// 尝试获取文本资源
    /// </summary>
    /// <param name="targetName"></param>
    /// <returns></returns>
    public string TryGetStringAsset(string targetName)
    {
        return TryGetAsset<StringAsset, string>(listStringAsset, targetName);
    }

    /// <summary>
    /// 尝试获取视频资源
    /// </summary>
    /// <param name="texName"></param>
    /// <returns></returns>
    public VideoClip TryGetVideoClipAsset(string targetName)
    {
        return TryGetAsset<VideoClipAsset, VideoClip>(listVideoClipAsset, targetName);
    }

    protected T TryGetAsset<TAsset, T>(List<TAsset> listAsset, string targetName)
        where TAsset : AssetBase<T>
    {
        T result = default(T);
        TAsset asset = listAsset.Find((a) => a.Name == targetName);
        if (asset != null)
            result = asset.Asset;
        else
            Debug.LogError("Can't Find " + targetName);
        return result;
    }

    /// <summary>
    /// 是否包含指定参数
    /// </summary>
    /// <param name="requireParam"></param>
    /// <returns></returns>
    public bool HasParam(string requireParam)
    {
        return listParam.Contains(requireParam);
    }

    /// <summary>
    /// 自定义资源的基类
    /// </summary>
    public abstract class AssetBase<T>
    {
        public abstract T Asset { get; }

        public string Name
        {
            get
            {
                if (!_name.IsNullOrEmpty())
                    return _name;
                else
                    return GetNameOverride();
            }
        }

        [Tooltip("可选，如果为空则使用图片的名字")]
        public string _name;//（可选）资源的标识名，用于编辑器中设置

        protected virtual string GetNameOverride()
        {
            return "";
        }
    }

    [System.Serializable]
    public class StringAsset : AssetBase<string>
    {
        public override string Asset
        {
            get
            {
                return str;
            }
        }

        public string str;//文本
    }

    [System.Serializable]
    public class VideoClipAsset : AssetBase<VideoClip>
    {
        public override VideoClip Asset
        {
            get
            {
                return videoClip;
            }
        }

        public VideoClip videoClip;
    }

    [System.Serializable]
    public class ObjectAsset : AssetBase<Object>
    {
        public override Object Asset
        {
            get
            {
                return obj;
            }
        }

        public Object obj;//资源
    }

    [System.Serializable]
    public class TextureAsset : AssetBase<Texture>
    {

        public override Texture Asset
        {
            get
            {
                return texture;
            }
        }

        protected override string GetNameOverride()
        {
            if (texture)
                return texture.name;
            else
            {
                Debug.LogError("No Texture! Please Asign one!");
                return "";
            }
        }
        public Texture texture;//图片
    }
}
