#if UNITY_EDITOR
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Threeyes.Steamworks
{
    /// <summary>
    /// 【Editor】存储Modder的自定义配置，支持重载
    /// PS：
    /// -会自动在Modder的目录下创建
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TSOSDKManagerInfo"></typeparam>
    /// <typeparam name="TSOItemInfo"></typeparam>
    public abstract class SOEditorSettingManager<T, TSOItemInfo> : SOInstacneBase<T, SOEditorSettingManagerInfo>
        where T : SOEditorSettingManager<T, TSOItemInfo>
        where TSOItemInfo : SOWorkshopItemInfo
    {
        #region Property & Field
        public TSOItemInfo CurWorkshopItemInfo
        {
            get
            { return curWorkshopItemInfo; }
            set
            {
                curWorkshopItemInfo = value;
                EditorUtility.SetDirty(Instance);
            }
        }

        //——AC_ItemManagerWindow——
        public string ItemWindow_ExePath
        {
            get
            {
                return itemWindow_ExePath;
            }
            set
            {
                itemWindow_ExePath = value;
                EditorUtility.SetDirty(Instance);
            }
        }
        public bool ItemWindow_IsPreviewGif
        {
            get
            {
                return itemWindow_IsPreviewGif;
            }
            set
            {
                itemWindow_IsPreviewGif = value;
                EditorUtility.SetDirty(Instance);
            }
        }

        [Expandable] [SerializeField] protected TSOItemInfo curWorkshopItemInfo;

        [Header("ItemManagerWindow")]
        [SerializeField] protected string itemWindow_ExePath = "";
        [SerializeField] protected bool itemWindow_IsPreviewGif = false;

        #endregion
    }
    public class SOEditorSettingManagerInfo : SOInstacneInfo
    {
        public override string pathInResources { get { return "Threeyes"; } }
        public override string defaultName { get { return "SteamworksEditorSetting"; } }//建议不要改名，保证统一
    }
}
#endif