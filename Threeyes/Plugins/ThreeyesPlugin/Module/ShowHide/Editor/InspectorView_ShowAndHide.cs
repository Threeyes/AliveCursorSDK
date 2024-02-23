#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using Threeyes.Core.Editor;
using UnityEditor;
using UnityEngine;
namespace Threeyes.ShowHide.Editor
{

    [CanEditMultipleObjects]
    [CustomEditor(typeof(ShowAndHideBase), true)]//editorForChildClasses
    public class InspectorView_ShowAndHide : UnityEditor.Editor
    {
        private ShowAndHideBase _target;

        void OnEnable()
        {
            _target = (ShowAndHideBase)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            CommonDrawer(_target);
        }

        public static void CommonDrawer<T>(T sh) where T : ShowAndHideBase
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Show"))
            {
                sh.Show();
            }
            if (GUILayout.Button("Hide"))
            {
                sh.Hide();
            }
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Toggle"))
            {
                sh.ToggleShow();
            }

            EditorTool.RepaintHierarchyWindow();
        }
    }
}
#endif