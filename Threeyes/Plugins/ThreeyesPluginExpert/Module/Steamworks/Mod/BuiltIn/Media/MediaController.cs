using Newtonsoft.Json;
using System;
using System.Collections;
using Threeyes.Persistent;
using UnityEngine;
using Threeyes.UI;
using Threeyes.Decoder;
using UnityEngine.Video;
using UnityEngine.Networking;
using NaughtyAttributes;
using static Threeyes.Steamworks.MediaController.ConfigInfo;
using System.Collections.Generic;
using Threeyes.Data;
using Threeyes.Core;
using Threeyes.ModuleHelper;

namespace Threeyes.Steamworks
{
    /// <summary>
    /// - 作为各种媒体的封装管理类
    /// 
    /// Todo：
    /// -首次初始化时，需要延后一帧，等待RendererHelper初始化材质，否则可能导致内容被冲掉
    /// 
    /// PS:
    /// -如果videoPlayer的RenderMode设置为APIOnly，则可以通过videoPlayer.texture获取对应的图片
    /// -标记为sealed避免滥用
    /// -以下的部分输出方式为可选，如果不需要就可以不赋值相关组件字段
    /// 
    /// Bug：
    /// -在运行时且Video正在播放，如果使用MaterialController修改材质会导致其克隆而锁定为当前画面。
    ///         -解决办法：MaterialController新增选项：不克隆，仅修改
    /// </summary>
    public sealed class MediaController : ConfigurableComponentBase<MediaController, SOMediaControllerConfig, MediaController.ConfigInfo, MediaController.PropertyBag>
        , IContextMenuProvider
    {
        #region Property & Field
        [Required] public RendererHelper rendererHelper;//控制图片的输出

        [Header("Gif")]
        public GifPlayer gifPlayer;//[Optional] 控制GIF的输入源
        public bool useRendererHelperToDisplayGif = true;//通过RendererHelper更新Gif图像帧。因为输出方式多样，用户可以禁用该字段并自行实现

        /// <summary>
        /// PS:
        /// -暂时只支持RenderMode：RenderTexture（建议，直接使用RendererHelper替换当前贴图）和MaterialOverride（暂不替换原材质以减少浮渣顶，后续有需要再针对每个输出方式设置特定材质）
        /// -输出音源：如果不需要声音，则把VideoPlayer的AudioOutputMode设置为None；如果需要3D音源，则设置为AudioSource（如果是PrefabMode，则需要在Debug模式才能设置AudioSource）
        /// </summary>
        [Header("Video")]
        public VideoPlayerHelper videoPlayerHelper;//[Optional] 控制视频的输入源/播放/暂停。（支持RenderMode为RenderTexture/MaterialOverride/APIOnly）
        public bool useRendererHelperToDisplayVideo = true;//通过RendererHelper更新Video图像帧，仅适用于VideoRenderMode为RenderTexture

        public StringEvent onErrorInfoChanged = new StringEvent();//【ToUse】可选的提示错误信息（如文件加载失败、url读取失败等）（如果没错需要清空）

        public MediaType curMediaType = MediaType.None;
        #endregion

        private void OnEnable()
        {
            if (useRendererHelperToDisplayGif && gifPlayer)
                gifPlayer.onUpdateTexture.AddListener(SetTextureFunc);

            if (useRendererHelperToDisplayVideo && videoPlayerHelper)
                videoPlayerHelper.Comp.prepareCompleted += OnVIdeoPlayerPrepareCompleted;
        }
        private void OnDisable()
        {
            if (useRendererHelperToDisplayGif && gifPlayer)
                gifPlayer.onUpdateTexture.RemoveListener(SetTextureFunc);

            if (useRendererHelperToDisplayVideo && videoPlayerHelper)
                videoPlayerHelper.Comp.prepareCompleted -= OnVIdeoPlayerPrepareCompleted;
        }
        private void OnVIdeoPlayerPrepareCompleted(VideoPlayer source)
        {
            if (useRendererHelperToDisplayVideo && videoPlayerHelper.Comp.renderMode == VideoRenderMode.APIOnly)//当视频准备完成时，当其RenderMode为APIOnly，则主动获取其贴图
            {
                SetTextureFunc(source.texture);
            }
        }



        #region IModHandler

        bool hasCacheInit = false;//cacheConfigInfo是否已经初始化
        ConfigInfo cacheConfigInfo = new ConfigInfo();//缓存上次的Source数据，只有当某个值更改才进行重新加载，避免频繁读取。
        public override void UpdateSetting()
        {
            if (hasCacheInit)
            {
                if (Config.isVideoMute != cacheConfigInfo.isVideoMute)
                {
                    SetVideoMuteFunc(Config.isVideoMute);
                }
            }

            if (Config.HasSourceChanged(cacheConfigInfo))//媒体源有更新：重新读取
            {
                //#1 Reset
                videoPlayerHelper?.Comp.Stop();
                gifPlayer?.Reset();
                curMediaType = MediaType.None;
                onErrorInfoChanged.Invoke("");

                ///Todo：
                ///-根据externalMediaFilePath及Media的bytes信息进行读取文件
                ///
                /// -[SDK]
                /// -每个模块都有一个可选单独的父物体，如果无效则会隐藏该模块(可以通过BoolEvent暴露，在Init时调用)，避免互相影响
                /// 
                ///-需要链接GifPlayer、VideoPlayer（或EventPlayer_Video/VideoPlayerHelper）等组件
                ///     -针对图像格式统一以Texture的形式输出，可以通过RendererHelper来更新材质或UI的图像信息
                ///     -针对视频格式，直接使用VideoPlayer相关组件播放

                MediaSource mediaSource = Config.mediaSource;
                if (mediaSource == MediaSource.Local && Config.LocalMedia)//#1 优先读取本地文件
                {
                    string absFilePath = FilePathModifier.GetAbsPath(Config.externalMediaFilePath);//将externalMediaFilePath转为全局路径，避免用户选择了relate导致返回相对路径（FileModifier的ParentDir对应了Config的PersistentDirPath）

                    byte[] bytes = Config.LocalMedia.bytes;
                    //Debug.LogError("bytes count: " + bytes.Length);

                    if (IsValidImageFIle(absFilePath))
                    {
                        SetImage(bytes);
                    }
                    else if (IsValidGifFile(absFilePath))
                    {
                        if (gifPlayer)
                            SetGif(bytes);
                    }
                    else if (IsValidVideoFile(absFilePath))
                    {
                        if (videoPlayerHelper)
                            SetVideoPlayer(absFilePath, true);
                    }
                    else
                    {
                        LogFunc("Not support format for file:\r\n" + absFilePath);
                    }
                }
                else if (mediaSource == MediaSource.Remote && Config.urlMediaPath.NotNullOrEmpty())//#2 读取Url在线媒体
                {
                    ///Todo:
                    ///-新增一个通用的下载Manager，先保存为本地路径的临时文件，然后再读取
                    ///-判断urlMediaPath的格式是否正确（如前缀为file://、http等）

                    if (IsValidImageFIle(Config.urlMediaPath))
                    {
                        DownloadTextureData(Config.urlMediaPath, SetImage);
                    }
                    else if (IsValidGifFile(Config.urlMediaPath))
                    {
                        if (gifPlayer)
                            DownloadTextureData(Config.urlMediaPath, actionOnDataProcessingError: SetGif);//PS:Gif对应actionOnDataProcessingError
                    }
                    else if (IsValidVideoFile(Config.urlMediaPath))
                    {
                        SetVideoPlayer(Config.urlMediaPath, false);
                    }
                    else
                    {
                        ///ToUpdate:
                        ///目前只能检测url包含指定文件后缀的路径，但其实很多网络图片/视频都没有特定后缀，需要下载才知道。可以通过在此尝试下载后使用:
                        ///1.尝试转为Image
                        ///2.失败后转为Gif(actionOnDataProcessingError的回调)
                        ///3.再次失败后转为Video
                        ///
                        ///增加Warning：随意下载文件可能有安全隐患！
                        Debug.LogWarning("Url format not supported! Try convert to any valid format one by one:\r\n" + Config.urlMediaPath);//提示后缀无法识别

                        DownloadTextureData(Config.urlMediaPath, SetImage, actionOnDataProcessingError:
                            (bytes) =>
                            {
                                try
                                {
                                    if (gifPlayer)
                                        SetGif(bytes);
                                }
                                catch (Exception e)
                                {
                                    Debug.LogError($"The file in the URL({Config.urlMediaPath}) is not a supported GIF file, Checking if it is in video format... \r\nError info:" + e);

                                    if (videoPlayerHelper)
                                        SetVideoPlayer(Config.urlMediaPath, false);
                                }
                            });
                    }
                }
                else//设置无效：忽略
                {
                }
            }

            //保存修改，方便后续比较
            cacheConfigInfo = UnityObjectTool.DeepCopy(Config);//保存所有数据
            //cacheConfigInfo_Source.SaveSourceChanged(Config);

            hasCacheInit = true;//标记cacheConfigInfo
        }

        void SetImage(byte[] bytes)
        {
            TextureDecoder.DecodeResult decodeResult = TextureDecoder.DecodeEx(bytes);
            if (decodeResult.value != null)
            {
                SetTextureFunc(decodeResult.value);
                curMediaType = MediaType.StaticImage;
            }
        }
        void SetGif(byte[] bytes)
        {
            gifPlayer.Init(bytes);
            curMediaType = MediaType.Gif;
        }
        void SetVideoPlayer(string absPath, bool isFileOrUrl)
        {
            //#1 设置路径
            if (isFileOrUrl)
                videoPlayerHelper.SetFileUrl(absPath);
            else
                videoPlayerHelper.SetRemoteUrl(absPath);

            //#2 设置输出源（使用VideoPlayer的目标RenderTexture）(ToUpdate:直接创建VideoClip对应的RenderTexture（或克隆VideoPlayer当前引用的），参考RenderTextureHelper)
            if (useRendererHelperToDisplayVideo && videoPlayerHelper.Comp.renderMode == VideoRenderMode.RenderTexture && videoPlayerHelper.Comp.targetTexture)
            {
                SetTextureFunc(videoPlayerHelper.Comp.targetTexture);
            }

            //#3 初始化设置
            videoPlayerHelper.SetMute(Config.isVideoMute);
            videoPlayerHelper.Play();
            curMediaType = MediaType.Video;
        }

        ///ToUpdate:
        ///-后期新增一个MediaTool，专门处理这些文件后缀判断等功能

        /// <summary>
        /// 通过RendererHelper来显示Texture
        /// </summary>
        /// <param name="texture"></param>
        void SetTextureFunc(Texture texture)
        {
            rendererHelper.SetTexture(texture);
        }
        #endregion

        #region IContextMenuProvider

        static readonly int contextMenuPriority_Video = 20;
        public /*override*/ List<ToolStripItemInfo> GetContextMenuInfos()
        {
            //List<ToolStripItemInfo> listInfo = base.GetContextMenuInfo();
            List<ToolStripItemInfo> listInfo = new List<ToolStripItemInfo>();
            ///提供动态的右键菜单：
            /// -当视频正在播放时，可出现静音切换菜单（Video/ToggleMute或）,增加播放/暂停选项。Config中需要保存isMute的选项，可以不暴露在外
            /// 【ToUpdate】：
            /// -升级为二级菜单，能够通过识别‘/’来自动生成多级菜单
            /// -提供多语言
            if (curMediaType == MediaType.Video && videoPlayerHelper)
            {
                listInfo.Add(new ToolStripItemInfo("Video/Mute", (o, arg) => SetVideoMuteFunc(true), contextMenuPriority_Video));
                listInfo.Add(new ToolStripItemInfo("Video/Unmute", (o, arg) => SetVideoMuteFunc(false), contextMenuPriority_Video + 1));

                //——以下选项暂不持久化，因为涉及到视频进度的问题，仅让用户在运行时调用——
                listInfo.Add(new ToolStripItemInfo("Video/TogglePlay", (o, arg) => videoPlayerHelper.TogglePlayPause(), contextMenuPriority_Video + 2));
            }
            return listInfo;
        }

        void SetVideoMuteFunc(bool isMute)
        {
            if (!videoPlayerHelper)
                return;
            videoPlayerHelper.SetMute(isMute);
            Config.isVideoMute = isMute;
        }

        [ContextMenu("ToggleVideoMute")]
        public void ToggleVideoMute()//Editor Debug Method
        {
            SetVideoMuteFunc(!Config.isVideoMute);
        }
        #endregion

        #region Utility
        /// <summary>
        /// 下载图像，支持静态图及Gif
        /// </summary>
        /// <param name="url"></param>
        /// <param name="actionOnSuccess"></param>
        /// <param name="actionOnDataProcessingError"></param>
        void DownloadTextureData(string url, Action<byte[]> actionOnSuccess = null, Action<byte[]> actionOnDataProcessingError = null)
        {
            TryStopCoroutine_Download();
            cacheEnum_Download = CoroutineManager.StartCoroutineEx(IEDownloadTextureData(url, actionOnSuccess, actionOnDataProcessingError));
        }

        UnityEngine.Coroutine cacheEnum_Download;
        void TryStopCoroutine_Download()
        {
            if (cacheEnum_Download != null)
                CoroutineManager.StopCoroutineEx(cacheEnum_Download);
        }

        /// <summary>
        /// 支持下载图片和gif，参考UIBrowserItem
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        IEnumerator IEDownloadTextureData(string url, Action<byte[]> actionOnSuccess = null, Action<byte[]> actionOnDataProcessingError = null)
        {
            using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
            {
                var request = uwr.SendWebRequest();
                yield return request;

                try
                {
                    if (uwr.result == UnityWebRequest.Result.Success)//Success代表为有效的图片数据
                    {
                        actionOnSuccess.Execute(uwr.downloadHandler.data);
                    }
                    else if (uwr.result == UnityWebRequest.Result.DataProcessingError)//【数据转换错误】尝试转换为Gif（PS:UnityWebRequestTexture.GetTexture会因为无法将Gif转换为图片而报此错误，由此可初步判断是否为gif格式）
                    {
                        actionOnDataProcessingError.Execute(uwr.downloadHandler.data);
                    }
                    else
                    {
                        Debug.LogError($"Download file from ({url}) failed:\r\n" + uwr.result + "\r\n" + uwr.error);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Download failed with error:\r\n" + e);
                }
            }
        }

        void LogFunc(string content)
        {
            Debug.LogError(content);
            onErrorInfoChanged.Invoke(content);
        }
        static bool IsValidVideoFile(string filePath)
        {
            ///各平台支持视频格式：https://docs.unity3d.com/Manual/VideoSources-FileCompatibility.html
            ///-暂时先使用Windows/macOS都支持且最广泛使用的mp4格式
            return filePath.ToLower().EndsWith(".mp4");
        }
        static bool IsValidGifFile(string filePath)
        {
            return filePath.ToLower().EndsWith(".gif");
        }

        /// <summary>
        /// Static Image
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        static bool IsValidImageFIle(string filePath)
        {
            return filePath.ToLower().EndsWith(".png") || filePath.ToLower().EndsWith(".jpg");//TextureDecorder暂时支持这几种格式，后面再扩充
        }
        #endregion

        #region Define

        public enum MediaType
        {
            None,
            StaticImage,
            Gif,
            Video
        }

        [Serializable]
        public class ConfigInfo : SerializableComponentConfigInfoBase
        {
            //——Media Source——
            ///ToAdd:
            ///-externalMediaFilePath增加Option，限制可选择的文件

            /// <summary>
            /// Warning：
            /// -初始化时有可能两个字段都是为空，需要判断一下。
            /// -可以在运行前设置defaultMedia的值
            /// -externalMedia的值会在运行后由RuntimeEditor实时创建
            /// </summary>
            public SOBytesAsset LocalMedia { get { return externalMedia ? externalMedia : defaultMedia; } }//本地媒体文件

            public MediaSource mediaSource = MediaSource.Local;
            //ToAdd：通过string提供外部路径，当其更新时根据文件的后缀调用不同的组件进行初始化。（研究如何标记为使用preUICommonFileField好让用户编辑;或者）
            [JsonIgnore] public SOBytesAsset defaultMedia;
            [JsonIgnore] public SOBytesAsset externalMedia;//
            [Tooltip("The local media file, support image/gif/video")] [PersistentAssetFilePath(nameof(externalMedia), true, dataOption_FilePropertyName: nameof(ExternalMediaDataOption), defaultAssetFieldName: nameof(defaultMedia))] [ShowIf(nameof(mediaSource), MediaSource.Local)] public string externalMediaFilePath;

            [Tooltip("The URL source of the media file, support image/gif/video")] [ShowIf(nameof(mediaSource), MediaSource.Remote)] [AllowNesting] public string urlMediaPath;//（优先使用Media，如果没有再Fallback为该字段）外部的流媒体Url路径（支持视频、图片等。【V2】如果是gif可以先下载到临时路径再解压）
            DataOption_File ExternalMediaDataOption { get { return new DataOption_BytesFile() { OverrideFileFilterExtensions = new string[] { "jpg", "png", "gif", "mp4" } }; } }

            //——Setting——
            public bool isVideoMute = false;

            //——Persistent——
            [HideInInspector] [JsonIgnore] [PersistentDirPath] public string PersistentDirPath;

            public ConfigInfo()
            {
            }


            /// <summary>
            /// 检查视频源设置是否有更新
            /// </summary>
            /// <param name="other"></param>
            /// <returns></returns>
            public bool HasSourceChanged(ConfigInfo other)
            {
                return mediaSource != other.mediaSource || externalMediaFilePath != other.externalMediaFilePath || urlMediaPath != other.urlMediaPath;
            }

            /// <summary>
            /// 仅保存Source相关字段
            /// </summary>
            /// <param name="other"></param>
            public void SaveSourceChanged(ConfigInfo other)
            {
                mediaSource = other.mediaSource;
                externalMediaFilePath = other.externalMediaFilePath;
                urlMediaPath = other.urlMediaPath;
            }
            public enum MediaSource
            {
                Local,
                Remote
            }
        }

        public class PropertyBag : ConfigurableComponentPropertyBagBase<MediaController, ConfigInfo> { }
        #endregion
    }
}
