#if UNITY_EDITOR
using System;
using System.Linq;
using System.Text;
using Threeyes.Core;
using Threeyes.Core.Editor;
using Threeyes.Persistent;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
namespace Threeyes.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(PersistentDataBase), true)]//editorForChildClasses
    public class InspectorView_PersistentData : InspectorViewSyncWithHierarchyBase
    {
        private PersistentDataBase _target;
        private void OnEnable()
        {
            if (target == null)
                return;
            if (target is PersistentDataBase)
                _target = (PersistentDataBase)target;
            else
                Debug.LogError(target.name + " (" + target.GetType() + ") is not type of " + nameof(PersistentDataBase));
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!_target)
                return;
            if (_target is PersistentData_Enum pd_Enum)//ToUpdate:后期直接隐藏string类型的值，可以通过NaughtAttribute或不同的继承实现
                OnInspectorGUI_Enum(pd_Enum);

            DrawHelpBox();
        }

        StringBuilder sbError = new StringBuilder();
        private void DrawHelpBox()
        {
            //当配置非法时，出现提示框
            sbError.Length = 0;
            _target.SetInspectorGUIHelpBox_Error(sbError);
            if (sbError.Length > 0)
                EditorGUILayout.HelpBox(sbError.ToString(), MessageType.Error);
        }

        public void OnInspectorGUI_Enum(PersistentData_Enum inst)
        {
            //Draw Enum
            var option = inst.DataOption;
            Type enumType = option.EnumType;
            EditorDrawerTool.DrawEnum(target, "DefaultValue", option.EnumType, () => inst.DefaultValue, (e) => inst.DefaultValue = e);
            EditorDrawerTool.DrawEnum(target, "PersistentValue", option.EnumType, () => inst.PersistentValue);//Readonly
        }
    }
}
#endif