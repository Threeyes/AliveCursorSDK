#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
namespace Threeyes.Editor
{
    /// <summary>
    /// PS:如果UITips所链接的物体是Prefab，那么重新载入Scene后它的text可能会恢复成原Prefab模板的设置
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UITips))]
    public class InspectorView_UITips : UnityEditor.Editor
    {
        private UITips _target;

        void OnEnable()
        {
            _target = (UITips)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            //serializedObject.Update();

            if (GUILayout.Button("UpdateTips"))
            {
                _target.UpdateTips();
            }
            if (GUILayout.Button("Show"))
            {
                _target.Show();
            }
            if (GUILayout.Button("Hide"))
            {
                _target.Hide();
            }

            if (GUI.changed)
            {
                if (!Application.isPlaying)
                {
                    Scene curScene = EditorSceneManager.GetActiveScene();
                    EditorSceneManager.MarkSceneDirty(curScene);
                    //EditorUtility.SetDirty(_target);//可能就是这句,_target实际引用了Prefab的UITips
                    //EditorUtility.SetDirty(_target.text);
                }
            }

            //serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif