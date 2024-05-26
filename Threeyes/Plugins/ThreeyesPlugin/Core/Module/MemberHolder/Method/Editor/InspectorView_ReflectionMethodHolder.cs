#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using Threeyes.Core.Editor;

namespace Threeyes.Core
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ReflectionMethodHolderBase), true)]//editorForChildClasses
    public class InspectorView_ReflectionMethodHolder : InspectorView_ReflectionMemberHolder<ReflectionMethodHolderBase>
    {
        protected override void OnInspectorGUIFunc()
        {
            GUILayout.Space(5);

            Object targetObject = _target.Target;
            if (!targetObject)
                return;

            System.Type targetType = targetObject.GetType();
            bool isMemberInfoValid = false;

            List<string> listMethodOption = new List<string>() { ReflectionMemberHolderBase.emptyMemberName };
            foreach (MethodInfo methodInfo in targetType.GetAllMethods(ReflectionMemberHolderBase.defaultBindingFlags))
            {
                if (_target.IsDesireMethod(methodInfo))
                    listMethodOption.Add(methodInfo.Name);
            }
            DrawPopUp(target, new GUIContent("Method"), listMethodOption,
                () => _target.TargetSerializeMethodName,
               OnMethodChanged,
                ref isMemberInfoValid);

            if (!isMemberInfoValid)
            {
                EditorGUILayout.HelpBox(warningText_PleaseSetMemberInfo, MessageType.Warning);
            }
            else
            {
                //针对带参方法，绘制Value
                if (_target is ReflectionMethodHolder_Enum rMH_Enum)//不知道Enum的具体类型无法直接绘制，因此要自行处理
                {
                    OnInspectorGUIFunc_EnumValue(rMH_Enum);
                }
                else
                {
                    SerializedProperty serializedProperty_Value = serializedObject.FindProperty("value");
                    if (serializedProperty_Value != null)
                    {
                        EditorGUILayout.PropertyField(serializedProperty_Value, new GUIContent("Value"));
                    }
                }
            }
        }

        private void OnInspectorGUIFunc_EnumValue(ReflectionMethodHolder_Enum rMH_Enum)
        {
            System.Type paramRealType = rMH_Enum.ParamRealType;
            if (paramRealType == null)//避免方法未设置
                return;

            EditorDrawerTool.DrawEnum(target, "Value", paramRealType, () => rMH_Enum.Value, (e) => rMH_Enum.Value = e);
        }


        void OnMethodChanged(string methodName)
        {
            _target.TargetSerializeMethodName = methodName;

        }
        protected override void OnFieldTargetChanged()
        {
            if (_target is ReflectionMethodHolder_Enum rMH_Enum)
            {
                rMH_Enum.SerializeValue = "";
            }
            _target.TargetSerializeMethodName = "";

            Undo.RecordObject(target, "Clear Old Data");//保存到Undo
        }


        #region Utility

        #endregion
    }
}
#endif