#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TopMenuEditor_Common : MonoBehaviour
{

    #region Inspector

    /// <summary>
    /// 切换Inspector的锁定状态（场景仅有一个时可用）
    /// </summary>
    [MenuItem(EditorDefinition.TopMenuItemPrefix + "Editor" + "Toggle Inspector Lock #%w")] // ctrl+shift+w
    static void ToggleInspectorLock()
    {
        foreach (var ed in ActiveEditorTracker.sharedTracker.activeEditors)
        {
            Debug.Log(ed.name);
        }

        // Ref: http://answers.unity3d.com/questions/282959/set-inspector-lock-by-code.html
        ActiveEditorTracker.sharedTracker.isLocked = !ActiveEditorTracker.sharedTracker.isLocked;
        ActiveEditorTracker.sharedTracker.ForceRebuild();

        //foreach (var editor in ActiveEditorTracker.sharedTracker.activeEditors)
        //{
        //    Debug.LogWarning(editor.name);
        //}
    }

    #endregion


}
#endif