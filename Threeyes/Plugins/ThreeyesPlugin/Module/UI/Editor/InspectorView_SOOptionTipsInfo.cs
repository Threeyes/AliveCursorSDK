#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Threeyes.Editor
{

    [CanEditMultipleObjects]
    [CustomEditor(typeof(SOOptionTipsInfo), true)]
    public class InspectorView_SOOptionTipsInfo : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            SOOptionTipsInfo _target = (SOOptionTipsInfo)target;
            DrawDefaultInspector();

#if USE_RTVoice
        if (GUILayout.Button("生成选中的提示语音"))
            GenerateAudio.AutoGenerateSpec();

        if (GUILayout.Button("生成所有提示语音"))
            GenerateAudio.AutoGenerate();

        if (GUILayout.Button("更新场景中的提示"))
            _target.UpdateSceneTips();

#endif
        }
    }
}
#endif