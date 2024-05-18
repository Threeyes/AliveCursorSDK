#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
/// <summary>
/// 将Inspector的更改同步到Hierarchy上（适用于自定义Inspector界面）
/// </summary>
namespace Threeyes.Core.Editor
{
    public class InspectorViewSyncWithHierarchyBase : UnityEditor.Editor
    {
        protected bool IsMultiSelected
        {
            get { return serializedObject.isEditingMultipleObjects; }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();//The Update method actually reads and copies the data into an internal structure (SerializedProperty). That means once “updated” a SerializedObject and it’s SerializedProperties represents a copy of the serialized data in memory. 
            EditorGUI.BeginChangeCheck();

            OnInspectorGUIFunc();

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();//Write the copied data back to disk.
                EditorApplication.RepaintHierarchyWindow();
            }
        }

        public virtual void OnInspectorGUIFunc()
        {
            base.OnInspectorGUI();
        }

        #region GUI Method

        protected void DrawPropertyField(string propertyPath, GUIContent gUIContent = null)
        {
            EditorDrawerTool.DrawPropertyField(serializedObject, propertyPath, gUIContent);//显示事件
        }

        protected bool DrawFoldOut(GUIPropertyGroup gUIPropertyGroup, bool isFoldout)
        {
            return EditorDrawerTool.DrawFoldOut(serializedObject, gUIPropertyGroup, isFoldout);
        }

        #endregion
    }
}
#endif