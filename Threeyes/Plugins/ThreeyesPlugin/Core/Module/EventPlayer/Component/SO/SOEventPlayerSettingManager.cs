#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Threeyes.Editor;

namespace Threeyes.EventPlayer
{
    //ToUpdate:应该是管理所有Core的Module的宏定义
    [CreateAssetMenu(menuName = "SO/Manager/EventPlayerSettingManager")]
    public class SOEventPlayerSettingManager : SOInstacneBase<SOEventPlayerSettingManager>
    {
        #region Property & Field

        //——Static——
        public static bool ShowPropertyInHierarchy { get { return Instance ? Instance.showPropertyInHierarchy : true; } set { if (Instance) Instance.showPropertyInHierarchy = value; } }
        public static SOEventPlayerSettingManager Instance { get { return GetOrCreateInstance(ref _instance, defaultName, pathInResources); } }
        private static SOEventPlayerSettingManager _instance;
        static string defaultName = "EventPlayerSetting";
        static string pathInResources = "Threeyes";//该Manager在Resources下的路径，默认是Resources根目录


        public string version = "0"; //Last cache version

        //Display Setting
        public bool showPropertyInHierarchy = true;//Show info of subclass

        //Other Plugin Support Setting
        //[Header("Other Plugin Support")]
        public bool useTimeline = false;
        public bool useVideoPlayer = false;
        public bool useBezierSolution = false;
        public bool useDoTweenPro = false;
        public bool activeDoTweenProPreview = false;

        //RelatePath
        static readonly string baseExtendDir = "Base/Timeline";
        static readonly string epExtendDir = "EventPlayer/Extend";

        //[宏定义,是否激活]
        public Dictionary<DefineSymbol, bool> dictDSActiveState
        {
            get
            {
                return new Dictionary<DefineSymbol, bool>
                {
                    { new DefineSymbol("Threeyes_Timeline", "Timeline Event", "支持Timeline", baseExtendDir+"|"+ epExtendDir) ,useTimeline},//PS:Timeline可能包括多个子插件（因此要检查所有Extend文件夹）
                    {  new DefineSymbol("Threeyes_VideoPlayer", "VideoPlayer Event", "支持VideoPlayer", epExtendDir+"/"+"Video"),useVideoPlayer },
                    {  new DefineSymbol("Threeyes_BezierSolution", "BezierSolution Support", "支持BezierSolution", epExtendDir+"/"+"BezierSolution"),useBezierSolution },
                    {  new DefineSymbol("Threeyes_DoTweenPro", "DoTweenPro Support", "支持DoTweenPro", epExtendDir+"/"+"DoTweenPro"),useDoTweenPro }
                };
            }
        }

        #endregion

        public void UpdateVersion(string currentVersion)
        {
            if (version != currentVersion)
            {
                //Update Setting
                version = currentVersion;
                EditorUtility.SetDirty(this);//先保存一次这个字段，否则后期的RefreshDefine会导致保存失败
                RefreshDefine();
                Debug.Log("Update EventPlayerSetting version from " + version + " to " + currentVersion);
                EditorUtility.SetDirty(this);//！需要调用该方法保存更改
            }
        }

        [ContextMenu("RefreshDefine")]
        public void RefreshDefine()
        {
            //ToUpdate:改用EditorDefineSymbolTool.ModifyDefines
            List<string> listDefineToAdd = new List<string>();
            List<string> listDefineToRemove = new List<string>();
            foreach (var element in dictDSActiveState)
            {
                if (element.Value == true)//激活状态
                    listDefineToAdd.Add(element.Key.name);
                else
                    listDefineToRemove.Add(element.Key.name);
            }
            EditorDefineSymbolTool.ModifyDefines(listDefineToAdd, listDefineToRemove);
        }

        void OnValidate()
        {
#if UNITY_EDITOR
            EditorApplication.RepaintHierarchyWindow();
#endif
        }
    }

    [InitializeOnLoad]
    public static class EventPlayerVersionManager
    {
        public static readonly string EventPlayer_Version = "3.0"; //Plugin Version

        static EventPlayerVersionManager()
        {
            if (SOEventPlayerSettingManager.Instance)
            {
                SOEventPlayerSettingManager.Instance.UpdateVersion(EventPlayer_Version);
            }
        }
    }
}
#endif