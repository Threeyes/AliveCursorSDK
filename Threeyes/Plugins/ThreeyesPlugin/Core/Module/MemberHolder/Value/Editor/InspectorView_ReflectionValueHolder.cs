#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using UnityEngine.Events;

namespace Threeyes.Core
{
    using MemberType = ReflectionValueHolderBase.MemberType;
    /// <summary>
    /// 提示用于设置target和memberType
    /// 
    /// PS：
    /// -仅弹出警告而不是错误，因为字段可能有其他用途，不一定要GetSet齐全
    /// 
    /// ToUpdate：
    /// -【非必须，因为可能部分方法或属性只能运行时调用。可以加一个bool值开关】调用Get，获取当前值并显示（参考InspectorView_ReflectionMethodHolder的实现）
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ReflectionValueHolderBase), true)]//editorForChildClasses
    public class InspectorView_ReflectionValueHolder : InspectorView_ReflectionMemberHolder<ReflectionValueHolderBase>
    {
        protected override void OnInspectorGUIFunc()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("targetMemberType"), new GUIContent("MemberType"));
            GUILayout.Space(5);

            Object targetObject = _target.Target;
            if (!targetObject)
                return;

            System.Type targetType = targetObject.GetType();
            var targetMemberType = _target.TargetMemberType;
            bool isMemberInfoValid = false;

            //——Field——
            if (targetMemberType == MemberType.Field)
            {
                List<string> listFieldOption = new List<string>() { ReflectionMemberHolderBase.emptyMemberName };
                foreach (FieldInfo fieldInfo in targetType.GetAllFields(ReflectionMemberHolderBase.defaultBindingFlags))
                {
                    if (_target.IsValueTypeMatch(fieldInfo.FieldType))
                        listFieldOption.Add(fieldInfo.Name);
                }
                DrawPopUp(target, new GUIContent("Field"), listFieldOption,
                    () => _target.TargetSerializeFieldName,
                    (val) => _target.TargetSerializeFieldName = val,
                    ref isMemberInfoValid);
            }

            //——Property——
            if (targetMemberType == MemberType.Property)
            {
                List<string> listPropertyOption = new List<string>() { ReflectionMemberHolderBase.emptyMemberName };
                foreach (PropertyInfo propertyInfo in targetType.GetAllPropertys(ReflectionMemberHolderBase.defaultBindingFlags))
                {
                    if (_target.IsValueTypeMatch(propertyInfo.PropertyType))
                        listPropertyOption.Add(propertyInfo.Name);
                }
                DrawPopUp(target, new GUIContent("Property"), listPropertyOption,
                    () => _target.TargetSerializePropertyName,
                    (val) => _target.TargetSerializePropertyName = val,
                    ref isMemberInfoValid);

                if (_target.TargetSerializePropertyName.NotNullOrEmpty())//确保Property是否同时包含 get/set，如果缺少某个模块则报错
                {
                    PropertyInfo propertyInfoCurSelect = targetType.GetProperty(_target.TargetSerializePropertyName);
                    if (propertyInfoCurSelect != null)
                    {
                        string warningTips = null;
                        if (!propertyInfoCurSelect.CanRead)
                            warningTips += "This property is Write Only!";
                        if (!propertyInfoCurSelect.CanWrite)
                            warningTips += "This property is Read Only!";
                        if (warningTips.NotNullOrEmpty())
                            EditorGUILayout.HelpBox(warningTips, MessageType.Warning);
                    }
                }
            }

            //——Method——
            if (targetMemberType == MemberType.Method)
            {
                List<string> listGetMethodOption = new List<string>() { ReflectionMemberHolderBase.emptyMemberName };
                List<string> listSetMethodOption = new List<string>() { ReflectionMemberHolderBase.emptyMemberName };
                foreach (MethodInfo methodInfo in targetType.GetAllMethods(ReflectionMemberHolderBase.defaultBindingFlags))
                {
                    if (_target.IsDesireGetMethod(methodInfo))
                        listGetMethodOption.Add(methodInfo.Name);

                    if (_target.IsDesireSetMethod(methodInfo))
                        listSetMethodOption.Add(methodInfo.Name);
                }

                //分别记录两种方法的有效性
                bool isMemberInfoValid_Get = false;
                bool isMemberInfoValid_Set = false;
                DrawPopUp(target, new GUIContent("GetMethod"), listGetMethodOption,
                    () => _target.TargetSerializeGetMethodName,
                    (val) => _target.TargetSerializeGetMethodName = val,
                    ref isMemberInfoValid_Get);
                DrawPopUp(target, new GUIContent("SetMethod"), listSetMethodOption,
                    () => _target.TargetSerializeSetMethodName,
                    (val) => _target.TargetSerializeSetMethodName = val,
                    ref isMemberInfoValid_Set);
                isMemberInfoValid = isMemberInfoValid_Get && isMemberInfoValid_Set;
            }
            if (!isMemberInfoValid)
            {
                EditorGUILayout.HelpBox(warningText_PleaseSetMemberInfo, MessageType.Warning);
            }
        }
        protected override void OnFieldTargetChanged()
        {
            _target.TargetSerializeFieldName =
            _target.TargetSerializePropertyName =
            _target.TargetSerializeGetMethodName =
            _target.TargetSerializeSetMethodName = "";
            Undo.RecordObject(target, "Clear Old Data");//保存到Undo
        }
    }
}
#endif