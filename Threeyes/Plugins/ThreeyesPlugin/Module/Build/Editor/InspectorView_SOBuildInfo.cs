#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;
namespace Threeyes.Editor
{
    /// <summary>
    /// 参考：引用显示优化 https://forum.unity.com/threads/editor-tool-better-scriptableobject-inspector-editing.484393/
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SOBuildInfo), true)]
    public class InspectorView_SOBuildInfo : UnityEditor.Editor
    {
        GUIStyle HLTextStyle = new GUIStyle();
        public override void OnInspectorGUI()
        {
            SOBuildInfo _target = (SOBuildInfo)target;

            DrawBuildButton(_target);

            DrawDefaultInspector();


            EditorGUILayout.BeginVertical();

            #region UpdateDescription

            if (GUILayout.Button("增加打包注释"))
            {
                _target.AddUpdateDescription();
            }

            #endregion

            #region  Build

            EditorGUILayout.Space();

            DrawBuildButton(_target);

            #endregion

            #region SetUp

            EditorGUILayout.Space();

            Color cacheColor = GUI.color;
            GUI.color = SOBuildInfoManager.Instance.CurBuildInfo == _target ? EditorGUIDefinition.HLColor : cacheColor;
            if (GUILayout.Button("SetAsCur"))
            {
                AssetMenuEditor_BuildPlayer.SetAsCur(_target);
            }
            GUI.color = cacheColor;

            if (GUILayout.Button("SetBuildingSettingScene"))
            {
                AssetMenuEditor_BuildPlayer.SetBuildingSettingScene(_target);
            }
            if (GUILayout.Button("SetConsoleSceneConfig"))
            {
                if (_target.sOConsoleSceneConfig)
                    AssetMenuEditor_BuildPlayer.SetConsoleSceneConfig(_target.sOConsoleSceneConfig);
            }

            if (GUILayout.Button("ModifyScene"))
            {
                AssetMenuEditor_BuildPlayer.SetAsCur(_target);
                AssetMenuEditor_BuildPlayer.ModifyScene(_target);
            }

            #region StreamingAsset
            string streamingAssetFolderPath = AssetMenuEditor_BuildPlayer.GetOEMStreamingAssetsDataPath(_target);
            GUI.enabled = Directory.Exists(streamingAssetFolderPath);
            if (GUILayout.Button("Open StreamingAsset Folder"))
            {
                PathTool.OpenFolder(streamingAssetFolderPath);
            }
            GUI.enabled = true;


            if (GUILayout.Button("CopyStreamingAssets"))
            {
                AssetMenuEditor_BuildPlayer.CopyStreamingAssets(_target);
            }
            if (GUILayout.Button("BackUpStreamingAssets"))
            {
                AssetMenuEditor_BuildPlayer.BackUpStreamAssets(_target);
            }

            GUILayout.Label("StreamingAssets Path: " + AssetMenuEditor_BuildPlayer.GetOEMStreamingAssetsDataPath(_target), EditorStyles.wordWrappedLabel);

            #endregion

            #endregion

            EditorGUILayout.EndVertical();
        }

        private static void DrawBuildButton(SOBuildInfo _target)
        {
            GUILayout.BeginHorizontal();
            if (Selection.objects.Length > 1)//大于一个
            {
                if (GUILayout.Button("Build Select"))
                {
                    AssetMenuEditor_BuildPlayer.BuildSelect();
                    GUIUtility.ExitGUI();//https://forum.unity.com/threads/endlayoutgroup-beginlayoutgroup-must-be-called-first.523209/
                }
                if (GUILayout.Button("Build Select&Run"))
                {
                    AssetMenuEditor_BuildPlayer.BuildSelectAndRun();
                    GUIUtility.ExitGUI();//https://forum.unity.com/threads/endlayoutgroup-beginlayoutgroup-must-be-called-first.523209/
                }
            }
            else
            {
                if (GUILayout.Button("Build"))
                {
                    AssetMenuEditor_BuildPlayer.BulidTarget(_target, false);
                    GUIUtility.ExitGUI();
                }

                if (GUILayout.Button("Build&Run"))
                {
                    AssetMenuEditor_BuildPlayer.BulidTarget(_target);
                    GUIUtility.ExitGUI();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            string buildFilePath = AssetMenuEditor_BuildPlayer.GetBuildFilePath(_target);
            string buildFolderPath = AssetMenuEditor_BuildPlayer.GetBuildFolderPath(_target);
            GUI.enabled = File.Exists(buildFilePath);
            if (GUILayout.Button("Open File"))
            {
                OpenFile(buildFilePath);
            }
            GUI.enabled = true;


            GUI.enabled = Directory.Exists(buildFolderPath);
            if (GUILayout.Button("Open Folder"))
            {
                PathTool.OpenFolder(buildFolderPath);
            }
            GUI.enabled = true;

            GUILayout.EndHorizontal();
        }

        private static void OpenFile(string buildFilePath)
        {
            ProcessStartInfo psi = new ProcessStartInfo(buildFilePath);//“”内为文件路径
                                                                       //创建进程对象
            Process pro = new Process();
            //告诉进程要打开的文件信息
            pro.StartInfo = psi;
            //调用方法打开
            pro.Start();
        }
    }
}
#endif