#if UNITY_2020_1_OR_NEWER
#define NewVersion
#else//旧版本直接激活
#define Active
#endif
#if NewVersion && USE_Timeline
#define Active
#endif
#if Active
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Threeyes.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UITipsClip))]
    public class InspectorView_UITipsClip : BaseClipEditor<UITipsClip>
    {
        private SerializedProperty mserialPropertySoTipsInfo = null;
        protected virtual void OnEnable()
        {
            if (serializedObject != null)
                mserialPropertySoTipsInfo = FindProperty(x => x.soTipsInfo);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            SOTipsInfo soTipsInfo = mserialPropertySoTipsInfo.exposedReferenceValue as SOTipsInfo;

            if (GUILayout.Button("刷新长度"))
            {
                var clip = target as UITipsClip;
                //Todo(可以尝试先取消赋值，再重新赋值）
            }


            //Preview
            if (mserialPropertySoTipsInfo != null)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.Space();
                soTipsInfo.tips = EditorGUILayout.TextArea(soTipsInfo.tips, GUILayout.Height(100));

                EditorGUILayout.LabelField("Extra Tips");
                if (soTipsInfo.listExtraTips.Count > 0)
                {
                    for (int i = 0; i != soTipsInfo.listExtraTips.Count; i++)
                    {
                        soTipsInfo.listExtraTips[i] = EditorGUILayout.TextField(soTipsInfo.listExtraTips[i]);
                    }
                }

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(soTipsInfo, "Set Field Value");
                    EditorUtility.SetDirty(soTipsInfo);
                }
            }
        }
    }
}
#endif
#endif