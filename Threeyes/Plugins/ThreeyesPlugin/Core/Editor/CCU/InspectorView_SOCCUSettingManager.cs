#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static Threeyes.Core.Editor.ConditionalCompilationUtility;
using UnityEngine.Events;

//[assembly: Threeyes.Core.OptionalDependency("Newtonsoft.Json.JsonConvert123", "USE_JsonDotNet123", "Threeyes.ActionTest")]//ToDelete

namespace Threeyes.Core.Editor
{
    /// <summary>
    /// Todo:
    /// +列出所有自定义Define及激活状态
    /// +Tooltip中显示依赖该插件的所在库
    /// </summary>
    [CustomEditor(typeof(SOCCUSettingManager), true)]
    public class InspectorView_SOCCUSettingManager : UnityEditor.Editor
    {
        [MenuItem("Tools/Threeyes/" + "Conditional Compilation Setting")]
        public static void OpenOrCreateConfig()
        {
            var instnace = SOCCUSettingManager.Instance;//用户调用该菜单，才尝试创建其实例（PS：不放在[InitializeOnLoad]静态类中，因为了避免Unity未初始化导致报错）

            //聚焦该实例
            EditorUtility.FocusProjectWindow();
            EditorWindow inspectorWindow = EditorWindow.GetWindow(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow"));
            inspectorWindow.Focus();
            Selection.activeObject = instnace;
        }

        //SOCCUSettingManager _target;
        static List<OptionalDependencyInfo> listInfo_All = new List<OptionalDependencyInfo>();
        private void Awake()
        {
            //_target = (SOCCUSettingManager)target;
            InitData();
        }

        private void OnValidate()//Editor-only function that Unity calls when the script is loaded or a value changes in the Inspector.
        {
            InitData();
        }

        private void InitData()
        {
            List<string> projectDefines;
            listInfo_All = ConditionalCompilationUtility.GetODIs(out projectDefines);//因为该方法要遍历库，太耗时，所以不能频繁调用
        }

        static bool isSettingChange = false;
        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            //#1 Info
            GUILayout.Label("All Optional Dependencies in project", gUISytleGroupTitleText);
            GUI.enabled = false;
            foreach (OptionalDependencyInfo info in listInfo_All)
            {
                string requied = "Require: " + info.dependentClass;
                string usedByInfo = "Used by: " + info.listUsedBy.ConnectToString(", ");
                DrawToggle(new GUIContent(info.define, requied + "\r\n" + usedByInfo), () => info.isActive, null, ref isSettingChange);
            }
            GUI.enabled = true;

            //#2 Settings
            GUILayout.Label("Settings", gUISytleGroupTitleText);
            bool isAutoUpdateChange = false;
            bool autoUpdate = DrawToggle(new GUIContent("Auto Update"), () => ConditionalCompilationUtility.AutoUpdate, (value) => ConditionalCompilationUtility.AutoUpdate = value, ref isAutoUpdateChange);
            if (autoUpdate)
            {
                if (isAutoUpdateChange)//Use change to autoUpdate
                {
                    ConditionalCompilationUtility.UpdateDependencies();//主动调用
                }
            }
            else//Manual Update
            {
                if (GUILayout.Button("Manual Update"))//提供手动更新按钮
                {
                    ConditionalCompilationUtility.UpdateDependencies();
                }
            }
        }

        #region Utility
        public static GUIStyle gUISytleGroupTitleText
        {
            get
            {
                if (_gUISytleGroupTitleText == null)
                {
                    _gUISytleGroupTitleText = new GUIStyle()
                    {
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleLeft,
                    };
                    _gUISytleGroupTitleText.normal.textColor = EditorDrawerTool.labelHLColor * 0.8f; //根据Editor的Skin决定颜色
                }
                return _gUISytleGroupTitleText;
            }
        }
        static GUIStyle _gUISytleGroupTitleText;

        bool DrawToggle(GUIContent gUIContent, CustomFunc<bool> getter, UnityAction<bool> setter, ref bool isChanged)
        {
            bool curValue = getter();
            bool result = EditorGUILayout.ToggleLeft(gUIContent, getter());
            if (result != curValue)
            {
                if (setter != null)
                {
                    //Undo.RecordObject(_target, "Changed Property");
                    setter(result);//Change  setting
                    //EditorUtility.SetDirty(_target);
                }
                isChanged |= true;//Mark as changed
            }
            return result;
        }
        #endregion
    }
}
#endif