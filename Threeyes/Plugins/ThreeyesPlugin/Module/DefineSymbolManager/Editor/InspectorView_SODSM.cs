#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Threeyes.Editor
{
    [CustomEditor(typeof(SODefineSymbolManager), true)]
    public class InspectorView_SODSM : UnityEditor.Editor
    {
        string cacheScriptingDefineSymbols = "";
        SODefineSymbolManager _target;
        private void Awake()
        {
            _target = (SODefineSymbolManager)target;
            _target.Init();
            GetScriptingDefineSymbols();
        }

        void GetScriptingDefineSymbols()
        {
            //Cache
            cacheScriptingDefineSymbols = _target.ScriptingDefineSymbols;
        }

        public override void OnInspectorGUI()
        {

            SODefineSymbolManager _target = (SODefineSymbolManager)target;

            GUILayout.BeginVertical();

            GUILayout.Label("选择需要使用的第三方插件，然后按Refersh更新配置");
            //使用Toggle显示所有的可选项
            foreach (DefineSymbol ds in SODefineSymbolManager.ListAvaliableDS)
            {
                bool isUsing = _target.listCacheUsedDS.Contains(ds);
                bool isUse = GUILayout.Toggle(isUsing, new GUIContent(ds.description_en, ds.description_cn));
                if (isUse != isUsing)//点击了Toggle
                {
                    if (isUse)
                        _target.listCacheUsedDS.AddOnce(ds);
                    else
                        _target.listCacheUsedDS.Remove(ds);
                    EditorUtility.SetDirty(_target);//Save Change to Disk
                }
            }

            //Warning:如果不使用缓存直接修改值，会导致频繁更新而卡顿
            cacheScriptingDefineSymbols = EditorDrawerTool.DrawTextArea(cacheScriptingDefineSymbols);

            GUILayout.Space(4);

            if (GUILayout.Button("Search"))
            {
                SearchFunc(_target);
            }

            if (GUILayout.Button("Refresh"))
            {
                RefreshFunc(_target);
            }

            if (GUILayout.Button("SearchAndRefresh"))
            {
                SearchAndRefreshFunc(_target);
            }

            if (GUILayout.Button("Update KeyStore"))
            {
                AndroidBuildSettingsEditor.UpdateKeyStoreSetting();
            }

            GUILayout.EndVertical();

            GUILayout.Space(20);

            GUILayout.Label("源代码信息：");
            base.OnInspectorGUI();

        }

        private static void SearchFunc(SODefineSymbolManager _target)
        {
            _target.Search();
        }
        private void RefreshFunc(SODefineSymbolManager _target)
        {
            //if (cacheScriptingDefineSymbols != _target.ScriptingDefineSymbols)//优先更新输入框内的数据
            //{
            //    _target.ScriptingDefineSymbols = cacheScriptingDefineSymbols;
            //}
            //else
            _target.Refresh();

            GetScriptingDefineSymbols();
        }

        private void SearchAndRefreshFunc(SODefineSymbolManager _target)
        {
            _target.SearchAndRefresh(
                () => GetScriptingDefineSymbols()
                );
        }



    }
}
#endif