#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using Threeyes.Persistent;
using UnityEditor;
using UnityEngine;

namespace Threeyes.Editor
{
    [InitializeOnLoad]
    public static class HierarchyView_PersistentData
    {
        static HierarchyView_PersistentData()
        {
            //Delegate for OnGUI events for every visible list item in the HierarchyWindow.
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
        }

        private static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

            if (!go)
                return;

            PersistentDataBase comp = go.GetComponent<PersistentDataBase>();
            if (comp)
            {
                EditorDrawerTool.RecordGUIColors();

                EditorDrawerTool.DrawHierarchyViewInfo(selectionRect, comp);

                EditorDrawerTool.RestoreGUIColors();
            }
        }
    }
}
#endif