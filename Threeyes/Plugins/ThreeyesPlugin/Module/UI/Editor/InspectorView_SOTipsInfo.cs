#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Threeyes.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SOTipsInfo), true)]
    public class InspectorView_SOTipsInfo : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            SOTipsInfo _target = (SOTipsInfo)target;
            DrawDefaultInspector();

#if USE_RTVoice

        var clip = _target.audioClip;
        if (clip)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("播放音频"))
            {
                EditorTool.PlayClip(clip);
            }
            if (GUILayout.Button("停止播放"))
            {
                EditorTool.StopAllClips();
            }
            GUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("生成选中的提示语音"))
        {
            GenerateAudio.AutoGenerateSpec();
            _target.CalAudioLength();
        }

        if (GUILayout.Button("生成所有提示语音"))
        {
            GenerateAudio.AutoGenerate();
            _target.CalAudioLength();
        }

        if (GUILayout.Button("更新场景中的提示"))
            _target.UpdateSceneTips();

        if (GUILayout.Button("设置文本的颜色"))
        {
            _target.UpdateTipsColor();
            _target.UpdateSceneTips();
        }
        if (GUILayout.Button("删除文本的颜色"))
        {
            _target.DeleteTipsColor();
            _target.UpdateSceneTips();
        }
#endif
        }
    }
}
#endif