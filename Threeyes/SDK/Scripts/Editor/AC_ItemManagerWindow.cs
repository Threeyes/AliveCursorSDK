#if UNITY_EDITOR
using UMod.BuildEngine;
using UnityEditor;
using UMod.Shared;
using Threeyes.Steamworks;

namespace Threeyes.AliveCursor.SDK.Editor
{
    /// <summary>
    ///
    /// UIBuilder注意：
    /// 1.（Bug）TextFiled/Label通过bingdingPath绑定ulong后会显示错误，因此暂时不显示ItemID（应该是官方的bug：https://forum.unity.com/threads/binding-ulong-serializedproperty-to-inotifyvaluechanged-long.1005417/）
    /// 2.ViewDataKey只对特定UI有效（PS：This key only really applies to ScrollView, ListView, and Foldout. If you give any of these a unique key (not enforced, but recommended （https://forum.unity.com/threads/can-someone-explain-the-view-data-key-and-its-uses.855145/）)）
    ///
    /// ToUpdate:
    /// 1.ChangeLog输入框只有上传成功后才清空
    /// </summary>
    public sealed class AC_ItemManagerWindow : ItemManagerWindow<AC_ItemManagerWindow, AC_ItemManagerWindowInfo, AC_SOEditorSettingManager, AC_SOWorkshopItemInfo, AC_WorkshopItemInfo>
    {
        #region MenuItem
        [MenuItem("Alive Cursor/Item Manager", priority = 0)]
        public static void AC_OpenWindow()
        {
            OpenWindow();
        }
        [MenuItem("Alive Cursor/Build And Run %m", priority = 1)]
        public static void AC_BuildAndRunCurItem()
        {
            BuildAndRunCurItem();
        }
        [MenuItem("Alive Cursor/Build All", priority = 2)]
        public static void AC_BuildAll()
        {
            BuildAll();
        }
        [MenuItem("Alive Cursor/Add Simulator Scene", priority = 3)]
        public static void AC_RunCurSceneWithSimulator()
        {
            RunCurSceneWithSimulator();
        }
        [MenuItem("Alive Cursor/SDK Wiki", priority = 1000)]
        public static void AC_OpenSDKWiki()
        {
            OpenSDKWiki("https://github.com/Threeyes/AliveCursorSDK/wiki");
        }
        #endregion
    }
    public class AC_ItemManagerWindowInfo : ItemManagerWindowInfo<AC_SOEditorSettingManager, AC_SOWorkshopItemInfo>
    {
        public override AC_SOEditorSettingManager SOEditorSettingManagerInst { get { return AC_SOEditorSettingManager.Instance; } }
        public override string WindowAssetPath { get { return "Layouts/AC_ItemManagerWindow"; } }
        public override void AfterBuild(ModBuildResult result, ref AC_SOWorkshopItemInfo sOWorkshopItemInfo)
        {
            ModContent modContent = result.BuiltMod.GetModContentMask();
            {
                sOWorkshopItemInfo.itemSafety = modContent.Has(ModContent.Scripts) ? AC_WSItemAdvance.IncludeScripts : AC_WSItemAdvance.None;
                EditorUtility.SetDirty(sOWorkshopItemInfo);
            }
        }
    }
#endif
}