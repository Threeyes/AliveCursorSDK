using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;
using UnityEngine.Events;
using System.IO;
using Threeyes.Persistent;
#if USE_JsonDotNet
using Newtonsoft.Json;
#endif
#if UNITY_EDITOR
using System.Text.RegularExpressions;
using UnityEditor;
#endif

/// <summary>
/// 存放提示信息
/// </summary>
[CreateAssetMenu(menuName = "SO/TipsInfo/TipsInfo")]
[JsonObject(MemberSerialization.OptIn)]
public class SOTipsInfo : ScriptableObject
{
    public bool HasMessage { get { return sprite != null || !string.IsNullOrEmpty(tips); } }
    public bool HasAudio { get { return audioClip != null; } }
    public float AudioClipLength { get { return audioClip ? audioClip.length : 0; } }//音频长度

    //ToUpdate：删掉Name标记,SOOptionTipsInfo将其对应依赖改为JsonProperty
    [JsonProperty("标题")] public string title;//标题
    [JsonProperty("内容")] [Multiline] public string tips;//主要提示
    [JsonProperty("图片路径")] [PersistentAssetFilePath(nameof(texture))] public string textureFilePath;//主要图片的路径（绝对路径或相对于PersistentDataDirPath）

    [JsonIgnore] [HideInInspector] [PersistentDirPath] public string PersistentDirPath;//【运行时】存储PD所在路径
    public Texture texture;//主要图片
    public Sprite sprite;//图标
    public AudioClip audioClip;//音频

    [JsonProperty("额外信息")] public List<string> listExtraTips = new List<string>();//额外的信息，用于副标题、提示等位置
    public List<Texture> listExtraTexture = new List<Texture>();//额外的Texture
    public List<Sprite> listExtraSprite = new List<Sprite>();//额外的Sprite

    [Header("Config")]
    [JsonProperty("内容对齐方式")] public TextAnchor textAnchor = TextAnchor.MiddleCenter;//字体对齐方式
    public int blinkCount = 4;//动画闪烁显示次数。0为只显示，-1为无限循环（可用于警示）(Todo:非必要，后期根据需求再通过Json暴露)
    public TipsDisplayContent displayContent = new TipsDisplayContent();//决定显示的内容

    [Header("Editor Config")]
    [Multiline] public string speechTips;//语音提示（用于转为音频）
    public float audioLength;//音频长度，编辑器显示用
    public bool isAutoGenerateAudio = true;//是否自动生成音频（false适用于移到其他目录，且不需要重新生成的Tips）
    public bool isAutoUpdateSceneTips = true;//是否自动更新场景的提示
    public Color tipsRichTextColor;//Tips文本的RichText颜色，调用UpdateTipsColor时会用到

    #region Persistent

    //获取指定图片的绝对路径（适用于传入对应的path字段）
    public string GetAbsPersistentAssetFilePath(string filePath)
    {
        return PathTool.GetAbsPath(PersistentDirPath, filePath);
    }

    #endregion

    #region Editor
#if UNITY_EDITOR

    private void OnValidate()
    {
        UpdateSceneTips();
        CalAudioLength();
    }

    [ContextMenu("NameAddSpace")]
    public void CalAudioLength()
    {
        Undo.RegisterCompleteObjectUndo(this, "CalAudioLength " + this.name);
        audioLength = AudioClipLength;
    }

    [ContextMenu("CopyTipsToSpeech")]
    public void CopyTipsToSpeech()
    {
        speechTips = tips;
    }

    [ContextMenu("ClearSpeechTips")]
    public void ClearSpeechTips()
    {
        speechTips = "";
    }

    [ContextMenu("UseNameAsTips")]
    public void UseNameAsTips()
    {
        tips = name;
    }

    [ContextMenu("UseNameAsTitle Trim")]
    public void UseNameAsTitleTrim()
    {
        title = RemoveIndexFromName();
    }
    [ContextMenu("UseNameAsTips Trim")]
    public void UseNameAsTipsTrim()
    {
        tips = RemoveIndexFromName();
    }

    string RemoveIndexFromName()
    {
        string content = name;
        int emptyIndex = name.IndexOf(" ");//找到空格
        if (emptyIndex > 0)
        {
            content = content.Substring(emptyIndex + 1, content.Length - (emptyIndex + 1));
        }
        return content;
    }

    /// <summary>
    /// 序号加一
    /// </summary>
    [ContextMenu("NameAddOne")]
    public void NameAddOne()
    {
        string index = name.Substring(0, 2);
        int indexInt = index.TryParse<int>() + 1;
        index = string.Format("{0:D2}", indexInt);
        string newName = index + " " + name.Substring(3);
        string path = UnityEditor.AssetDatabase.GetAssetPath(this);
        string result = UnityEditor.AssetDatabase.RenameAsset(path, newName);
        if (result.NotNullOrEmpty())
            Debug.LogError(result);
    }

    /// <summary>
    /// 序号加一
    /// </summary>
    [ContextMenu("NameAddSpace")]
    public void NameAddSpace()
    {
        string path = UnityEditor.AssetDatabase.GetAssetPath(this);
        string result = UnityEditor.AssetDatabase.RenameAsset(path, name + " ");
        if (result.NotNullOrEmpty())
            Debug.LogError(result);
    }

    /// <summary>
    /// 更新当前场景中的Tips
    /// </summary>
    public void UpdateSceneTips()
    {
        if (!isAutoUpdateSceneTips)
            return;

#if UNITY_EDITOR
        //通知相关的Tips更新
        if (!Application.isPlaying)
        {
            List<UITips> listTips = Component.FindObjectsOfType<UITips>().ToList();
            foreach (var tips in listTips)
            {
                if (tips.TipsInfo == this)
                {
                    tips.UpdateTips();
                }
            }
        }
#endif
    }

    public void DeleteTipsColor()
    {
        if (tips.Contains("</color>"))//使用了正则表达式
        {
            //参考：https://c.runoob.com/front-end/854
            tips = Regex.Replace(tips, @"<color=(\S*?)[^>]*>.*?|</color>", "");
        }
    }

    /// <summary>
    /// （Bug：设置后还需要再次点击颜色条，强制Editor刷新）
    /// </summary>
    [ContextMenu("UpdateTipsColor")]
    public void UpdateTipsColor()
    {
        DeleteTipsColor();
        //tips = string.Format("<color=#{0}>{1}</color>", ByteToHex(tipsRichTextColor.r) + ByteToHex(tipsRichTextColor.g) + ByteToHex(tipsRichTextColor.b) + ByteToHex(tipsRichTextColor.a), tips);
        tips = tips.ToRichText_Color(tipsRichTextColor);
    }


    string ByteToHex(byte b)
    {
        return b.ToString("x2");
    }

#endif
    #endregion
}

/// <summary>
/// 提示显示的内容
/// </summary>
[System.Serializable]
public class TipsDisplayContent
{
    public bool isShowText = true;//文本
    public bool isShowSprite = true;//UI图片
    public bool isShowTexture = true;//图片
}

[System.Serializable]
public class SOTipsInfoEvent : UnityEvent<SOTipsInfo>
{

}