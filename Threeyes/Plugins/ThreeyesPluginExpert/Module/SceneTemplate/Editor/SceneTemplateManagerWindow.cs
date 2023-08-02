#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Threeyes.Editor;
using UnityEditor;
using UnityEditor.SceneTemplate;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
namespace Threeyes.SceneTemplate
{
    /// <summary>
    ///
    /// Ref: UnityEditor.SceneTemplate.SceneTemplateDialog
    ///
    /// ToUpdate：
    /// 1.搜索功能：UIBuilder有默认的SerachFieldUI，可以直接使用
    /// </summary>
    public class SceneTemplateManagerWindow : EditorWindow
    {
        private VisualTreeAsset uxmlAsset = default;

        VisualElement visualElementSceneTemplateManagerGroup;
        //——Left Group——
        VisualElement visualElementLeftGroup;
        ListView listViewSceneTemplate;
        RadioButtonGroup radioButtonGroupSceneTemplate;

        //——Right Group——
        VisualElement visualElementRightGroup;
        VisualElement visualElementPreviewArea;
        Button buttonCreate;
        Button buttonCancel;


        public List<SceneTemplateAsset> listValidAsset = new List<SceneTemplateAsset>();//扫描到的信息
        SceneTemplateAsset curSceneTemplateAsset;
        static string curTargetSceneFilePath;//待存储的场景文件路径

        private static readonly GUIContent k_WindowTitle = new GUIContent("Select Template...");
        private static readonly Vector2 k_MinWindowSize = new Vector2(600, 600);
        public static SceneTemplateManagerWindow ShowWindow(string targetSceneFilePath)
        {
            curTargetSceneFilePath = targetSceneFilePath;
            // Get existing open window or if none, make a new one:
            SceneTemplateManagerWindow window = GetWindow<SceneTemplateManagerWindow>(true);
            window.titleContent = k_WindowTitle;
            window.minSize = k_MinWindowSize;
            window.Show();
            return window;
        }

        private void OnEnable()
        {
            uxmlAsset = Resources.Load<VisualTreeAsset>("Layouts/SceneTemplateManagerWindow");
            uxmlAsset.CloneTree(rootVisualElement);

            //##Setup UI
            //——Left Group——
            visualElementSceneTemplateManagerGroup = rootVisualElement.Q<VisualElement>("SceneTemplateManagerGroup");
            visualElementLeftGroup = visualElementSceneTemplateManagerGroup.Q<VisualElement>("LeftGroup");
            listViewSceneTemplate = visualElementLeftGroup.Q<ListView>("SceneTemplateListView");
            listViewSceneTemplate.selectionChanged += OnListVIewChanged;
            //——Right Group——
            visualElementRightGroup = visualElementSceneTemplateManagerGroup.Q<VisualElement>("RightGroup");
            visualElementPreviewArea = visualElementRightGroup.Q<VisualElement>("PreviewArea");
            buttonCreate = visualElementRightGroup.Q<Button>("CreateButton");
            buttonCreate.RegisterCallback<ClickEvent>(OnCreateButtonClick);
            buttonCancel = visualElementRightGroup.Q<Button>("CancelButton");
            buttonCancel.RegisterCallback<ClickEvent>(OnCancelButtonClick);
            //ToAdd:搜索指定目录的所有SceneTemplate并显示，左边为按钮（带名称），右边预览

            InitUIWithCurInfo();
        }

        void OnListVIewChanged(IEnumerable<object> selection)
        {
            SceneTemplateAsset curSelect = selection.FirstOrDefault() as SceneTemplateAsset;
            if (!curSelect)
                return;

            InitCurInfo(curSelect);
        }

        #region UpdateUI
        void InitUIWithCurInfo()
        {
            InitUI(curSceneTemplateAsset);
        }
        void InitUI(SceneTemplateAsset target)
        {
            InitListView();
        }
        void InitListView()
        {
            listValidAsset = GetValidListSceneTemplateAsset();

            //#2 创建对应的ListView
            listViewSceneTemplate.Clear();
            List<string> listItemName = listValidAsset.Select(a => a.templateName).ToList();
            Func<VisualElement> makeItem = () =>
            {
                Label label = new Label();
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                return label;
            };
            Action<VisualElement, int> bindItem = (e, i) => (e as Label).text = listItemName[i];

            listViewSceneTemplate.makeItem = makeItem;
            listViewSceneTemplate.bindItem = bindItem;
            listViewSceneTemplate.itemsSource = listValidAsset;
            listViewSceneTemplate.selectedIndex = 0;//ToUpdate:使用上次已选的Template
        }

        /// <summary>
        /// 有需要可以重载搜索的范围
        /// </summary>
        /// <returns></returns>
        protected List<SceneTemplateAsset> GetValidListSceneTemplateAsset()
        {
            List<SceneTemplateAsset> listCurValidAsset = new List<SceneTemplateAsset>();
            //#1 扫描有效的Item
            foreach (SceneTemplateAsset sceneTemplateAsset in GetSceneTemplates())
            {
                string path = AssetDatabase.GetAssetPath(sceneTemplateAsset);
                if (Directory.GetParent(path).Parent.Name == "SceneTemplates")//PS:搜索(Assets或Package目录)存放在SceneTemplates/【自定义文件夹】下的模板场景，便于用户自行创建对应的SceneTemplate。（注意，URP的模板场景直接放在SceneTemplates文件夹下，因此可以忽略）
                {
                    listCurValidAsset.Add(sceneTemplateAsset);
                }
            }
            return listCurValidAsset;
        }
        void InitCurInfo(SceneTemplateAsset target)
        {
            curSceneTemplateAsset = target;
            if (target)
            {
                //通过rootVisualElement.Bind(so) 可将字段及Element进行动态绑定，从而显示预览信息
                SerializedObject so = new SerializedObject(target);
                visualElementRightGroup.Bind(so);
                visualElementPreviewArea.style.backgroundImage = target.preview;
                //ToUpdate：Preview的Aspect
            }
            else
            {
                visualElementRightGroup.Unbind();// Unbind the object from the actual visual element
                curSceneTemplateAsset = null;
            }
        }
        void OnCreateButtonClick(ClickEvent evt)
        {
            if (curTargetSceneFilePath.IsNullOrEmpty())
                return;

            if (!PathTool.GetParentDirectory(curTargetSceneFilePath).Exists)//如果文件夹不存在，则尝试创建
            {
                PathTool.GetOrCreateFileParentDir(curTargetSceneFilePath);
                AssetDatabase.Refresh();
            }
            string relateFilePath = EditorPathTool.AbsToUnityRelatePath(curTargetSceneFilePath);
            SceneTemplateService.Instantiate(curSceneTemplateAsset, true, relateFilePath);
            Close();
        }

        void OnCancelButtonClick(ClickEvent evt)
        {
            Close();
        }
        #endregion

        #region Utility
        static IEnumerable<SceneTemplateAsset> GetSceneTemplates()
        {
            foreach (var sceneTemplatePath in GetSceneTemplatePaths())
            {
                yield return AssetDatabase.LoadAssetAtPath<SceneTemplateAsset>(sceneTemplatePath);
            }
        }

        static IEnumerable<string> GetSceneTemplatePaths()
        {
            return AssetDatabase.FindAssets("t:SceneTemplateAsset").Select(AssetDatabase.GUIDToAssetPath);
        }
        #endregion
    }
}
#endif