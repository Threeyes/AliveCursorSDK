using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using System.IO;
using System.Linq;
using Threeyes.Core;
using Threeyes.Data;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Threeyes.Steamworks
{
    /// <summary>
    /// 通过SO存储WorkshopItem的信息，便于Unity可视化及转化成Json格式的WorkshopItemInfo
    /// </summary>
    public abstract class SOWorkshopItemInfo : ScriptableObject
    {
        #region Property & Field
        public abstract WorkshopItemInfo BaseItemInfo { get; }

        #region Workshop上传信息
        public ulong ItemID { get { return itemId; } set { itemId = value; } }
        public Texture2D TexturePreview { get => texturePreview; set => texturePreview = value; }
        public string Title { get => title; set => title = value; }//#0 名称（PS：用户可通过在名字里增加标识词来匹配搜索关键字）
        public string Description { get => description; }//#1 描述
        public abstract string ContentDirPath { get; }//#2 待上传文件夹路径
        /// <summary>
        /// #3 预览图路径(PS：为了便于用户预览图片，只能使用Unity内的文件（如果用户选择外部文件，则会被复制到本项目中）)
        /// </summary>
        public string PreviewFilePath
        {
            get
            {
#if UNITY_EDITOR
                if (TexturePreview != null)
                {
                    //PS:因为选中的图片可能与Item不同目录，而且后缀不确定，因此要用这种方式获取（不拷贝到Item的原因是避免资源在项目中被多次克隆）
                    return PathTool.ProjectDirPath + "/" + AssetDatabase.GetAssetPath(TexturePreview);
                }
#endif
                return "";
            }
        }
        public WSItemVisibility ItemVisibility { get { return itemVisibility; } set { itemVisibility = value; } }//#4 可视性
        /// <summary>
        /// #5 Tags，用逗号分隔
        /// 
        /// PS:
        //1.每个Tag只能是255字节内，且为可打印字符，具体规范：  https://partner.steamgames.com/doc/api/ISteamUGC#SetItemTags
        // 2.管理平台配置：https://partner.steamgames.com/doc/features/inventory/itemtags
        /// </summary>
        public abstract string[] Tags { get; }
        #endregion

        //#用户定义的信息(必要）
        //https://partner.steamgames.com/doc/features/workshop/implementation
        //https://partner.steamgames.com/doc/api/ISteamUGC#SetItemTag
        [Header("Item Info")]
        [SerializeField] string title;
        [SerializeField][Multiline] string description;
        [SerializeField] Texture2D texturePreview;
        [SerializeField] WSItemVisibility itemVisibility = WSItemVisibility.Public;

        //#唯一标识
        [Header("Item Identity")]
        [ReadOnly] public string itemName;//Item文件夹的名称（不可与本地项目中的其他Item重名）（通过EditorWindow创建时设置，不可二次更改。）
        [ReadOnly] public Identity ID;//唯一ID（用于避免AssetBundle加载重名的问题）（通过EditorWindow创建时设置，不可二次更改。）
        [ReadOnly] public ulong itemId = 0;//Steam赋予的唯一 ItemID(首次Upload成功后赋值，不可更改。)

        //#用户定义的信息(可选）
        //ToAdd:后期可在打包文件中另外附一张多语言文档
        //public string updateLanguage="english";//对用户来说有点复杂，暂时不实现。Todo:改为枚举或者让用户直接在网页编辑(参考链接：https://partner.steamgames.com/doc/api/ISteamUGC#SetItemUpdateLanguage）（https://partner.steamgames.com/doc/store/localization#supported_languages）
        //public string metaData;//元数据(可在Query时获取，而不需要将下载。最大值为5000bytes。（或者可以直接存储json文件内容，便于查询） ：https://partner.steamgames.com/doc/api/ISteamUGC#SetItemMetadata)（解惑：https://dataedo.com/kb/data-glossary/what-is-metadata）

        #endregion

        #region Check State
        public bool IsPreviewExtensionVaild// 检查当前预览图后缀是否可用
        {
            get
            {
                return arrStrValidPreviewFileExtension.Any((ex) => PreviewFilePath.Contains(ex));
            }
        }
        public bool IsPreviewFileSizeVaild
        {
            get
            {
                if (!File.Exists(PreviewFilePath))
                    return false;
                FileInfo fileInfo = new FileInfo(PreviewFilePath);
                return fileInfo.Length < MaxPreviewFileSize;
            }
        }

        public bool IsItemUploaded { get { return itemId != 0; } }//Item是否已经上传

        public bool IsBuildValid { get { string cacheValidResult; return CheckIfBuildValid(out cacheValidResult); } }
        public abstract bool IsExported { get; }//检查是否有导出目录
        public abstract bool IsUploadValid { get; }//PS:仅简单检查导出目录是否存在即可

        public virtual bool CheckIfBuildValid(out string errorLog)
        {
            errorLog = "";

            //#Title
            if (string.IsNullOrEmpty(Title))
            {
                errorLog = "The title is empty!";
            }

            //#Preview
            else if (!TexturePreview)//预览图不能为空
            {
                errorLog = "Preview file can't be null!";
            }
            else if (!File.Exists(PreviewFilePath))//预览图不在Unity项目内
            {
                errorLog = "The preview file does not exist!";
            }
            else if (!IsPreviewExtensionVaild)//预览图后缀不对
            {
                errorLog = "The extension of the preview file is not valid!";
            }
            else if (!IsPreviewFileSizeVaild)//预览文件大小不对
            {
                errorLog = $"The size of the preview file can't be larger than {MaxPreviewFileSize / 1024}KB!";
            }

            //#资源
            else if (IsSceneFile)//Scene文件
            {
                if (!File.Exists(SceneFilePath))//Scene不能为空
                {
                    errorLog = $"The scene file not exist in {SceneFilePath}!";
                }
            }
            return errorLog.IsNullOrEmpty();
        }
        public abstract bool CheckIfUploadValid(out string errorLog);
        #endregion

        #region Internal Path
        ///##项目内部
        ///文件夹路径：
        ///XXX（Item名）
        ///——Data
        ///————WorkshopItemInfo.asset
        ///————Preview.jpg/png/gif/...
        ///——Scene
        ///————Entry.unity
        public string ItemDirPath => GetItemDirPath(itemName);// Item文件夹的绝对路径
        public string DataDirPath => GetDataDirPath(itemName);// Item/Data的绝对路径
        public string SceneFilePath => GetSceneFilePath(itemName);// Item/Scene/Entry.unity的绝对路径
        protected virtual bool IsSceneFile { get { return true; } }//子类可以重载，方便打包只包含模型的Mod

        //——Utility——
        public static readonly string DataDirName = "Datas";
        public static readonly string SOAssetPackAssetName = "AssetPack.asset";
        public static readonly string WorkshopItemInfoAssetName = "WorkshopItemInfo.asset";
        public static readonly string DefaultPreviewName = "Preview";// 预览图名称
        public static readonly string[] arrStrValidPreviewFileExtension = new string[] { "jpg", "jpeg", "png", "gif" };// 预览图支持的格式（Warning：Workshop.Upload不支持大写后缀！）
        public static readonly long MaxPreviewFileSize = 1048576;// 预览图文件上限，单位B (PS:预览图文件大小上限为1024KB【如果超过，则Upload时会报错：Result.LimitExceeded】)
        public static readonly string SceneDirName = "Scenes";
        public static readonly string SceneName = "Entry";// 场景名称
        public static string GetItemDirPath(string itemName) { return Steamworks_PathDefinition.ItemParentDirPath + "/" + itemName; }
        public static string GetRelatedItemDirPath(string itemName) { return $"Assets/{Steamworks_PathDefinition.ItemRootDirName}/{itemName}"; }
        public static string GetSOAssetPackFilePath(string itemName)
        {
            return $"{GetDataDirPath(itemName)}/{SOAssetPackAssetName}";
        }
        public static string GetRelatedSOAssetPackFilePath(string itemName)
        {
            return $"{GetRelatedItemDirPath(itemName)}/{DataDirName}/{SOAssetPackAssetName}";
        }

        public static string GetSceneDirPath(string itemName) { return GetItemDirPath(itemName) + "/" + SceneDirName; }
        public static string GetSceneFilePath(string itemName) { return GetSceneDirPath(itemName) + "/" + SceneName + ".unity"; }
        public static string GetRelateSceneFilePath(string itemName) { return $"{GetRelatedItemDirPath(itemName)}/{SceneDirName}/{SceneName}.unity"; }//Unity的场景路径，确保打包后能正常加载，如：Assets/Items/Default/Scenes/Entry.unity
        public static string GetDataDirPath(string itemName) { return GetItemDirPath(itemName) + "/" + DataDirName; }
        public static string GetItemInfoFilePath(string itemName) { return GetDataDirPath(itemName) + "/" + WorkshopItemInfoAssetName; }
        public string GetDefaultPreviewFilePath(string fileExtension) => GetDefaultPreviewFilePath(itemName, fileExtension);
        public static string GetDefaultPreviewFilePath(string itemName, string fileExtension)
        {
            if (!fileExtension.StartsWith('.'))//PS:通过FileInfo.Extension获取的后缀带有'.'，因此需要统一补全
                fileExtension = "." + fileExtension;
            return GetItemDirPath(itemName) + "/" + DataDirName + "/" + DefaultPreviewName + fileExtension;
        }
        #endregion

        #region Export Path
        protected readonly string ItemModName_Scene = "Scene";//Scene文件的名称

        public virtual string ItemModName
        {
            get
            {
                return itemName + "_" + ID.Guid;//为了支持同时加载AssetBundle，需要为每一个打包的文件提供唯一的ID
                //return ItemModName_Scene;
            }
        }//打包后的Mod名称，子类可自定义
        public virtual string ItemModFileName { get { return ItemModName + "." + WorkshopItemInfo.UModFileExtension; } }//Mod文件名
        public abstract string ExportItemDirPath { get; }
        #endregion

        #region Utility
        [ContextMenu("Generate New ID if null")]
        public void GenerateNewIDIfNull()
        {
            if (ID.Guid.IsNullOrEmpty())
            {
                ID.Guid = Identity.NewGuid();
                SetAsDirty();
            }
        }

        //由WorkshopItemUploader调用
        public void SetAsDirty()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
        #endregion
    }

    public abstract class SOWorkshopItemInfo<TItemInfo> : SOWorkshopItemInfo
        where TItemInfo : WorkshopItemInfo, new()
    {
        public override WorkshopItemInfo BaseItemInfo { get { return ItemInfo; } }
        public abstract TItemInfo ItemInfo { get; }
        public override string ContentDirPath { get => ExportItemDirPath; }

        #region Export Path
        //public static readonly TItemInfo DefaultInfo = new TItemInfo().Default;//功能：用于获取实例中的override字段    
        ///#导出目录路径：
        ///XXX（Item名）
        ///——Content.umod
        ///——ItemInfo.json
        ///——Preview.xxx
        public override string ExportItemDirPath { get { return Steamworks_PathDefinition.ExportItemRootDirPath + "/" + itemName; } }
        public string ExportItemModFilePath => ExportItemDirPath + "/" + ItemModFileName;
        public string ExportItemInfoFilePath => ExportItemDirPath + "/" + WorkshopItemInfo.ItemInfoFileName;
        public string ExportItemPreviewFilePath { get { return ExportItemDirPath + "/" + new FileInfo(PreviewFilePath).Name; } }            //PS：如果 PreviewFilePath 返回"",那最终值也是无效路径

        public override bool IsExported { get { return Directory.Exists(ExportItemDirPath); } }//检查是否有导出目录
        public override bool IsUploadValid { get { return Directory.Exists(ExportItemDirPath); } }//PS:仅简单检查导出目录是否存在即可

        public override bool CheckIfUploadValid(out string errorLog)
        {
            errorLog = "";
            if (!Directory.Exists(ExportItemDirPath))
            {
                errorLog = $"Export Item dir not exist: {ItemDirPath}";
            }
            else if (!File.Exists(ExportItemModFilePath))
            {
                errorLog = $"Export Item Content file not exist in: {ExportItemModFilePath}";
            }
            else if (!File.Exists(ExportItemInfoFilePath))
            {
                errorLog = $"Export Item Info file not exist in: {ExportItemInfoFilePath}";
            }
            else if (!File.Exists(ExportItemPreviewFilePath))
            {
                errorLog = $"Export Item Preview file not exist in: {ExportItemPreviewFilePath}";
            }
            //ToAdd: 打开Scene并检查是否已经包含所有必要的组件（如AliveCursor）
            return errorLog.IsNullOrEmpty();
        }
        #endregion
    }
}