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
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ReflectionValueHolderBase), true)]//editorForChildClasses
    public class InspectorView_ReflectionValueHolder : UnityEditor.Editor
    {
        private ReflectionValueHolderBase _target;
        private void OnEnable()
        {
            _target = (ReflectionValueHolderBase)target;
        }

        const string warningText_PleaseSetMemberInfo = "Please set all MemberInfos!";
        public override void OnInspectorGUI()
        {
            Object lastObject = _target.Target;//缓存

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("target"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("targetMemberType"), new GUIContent("MemberType"));
            GUILayout.Space(5);
            //base.OnInspectorGUI();

            Object targetObject = _target.Target;
            if (targetObject)
            {
                System.Type targetType = targetObject.GetType();
                var targetMemberType = _target.TargetMemberType;
                bool isMemberInfoValid = false;

                //——Field——
                if (targetMemberType == MemberType.Field)
                {
                    List<string> listFieldOption = new List<string>() { ReflectionValueHolderBase.emptyMemberName };
                    foreach (FieldInfo fieldInfo in targetType.GetAllFields(ReflectionValueHolderBase.defaultBindingFlags))
                    {
                        if (_target.IsTypeMatch(fieldInfo.FieldType))
                            listFieldOption.Add(fieldInfo.Name);
                    }
                    DrawPopUp(new GUIContent("Field"), listFieldOption,
                        () => _target.TargetSerializeFieldName,
                        (val) => _target.TargetSerializeFieldName = val,
                        ref isMemberInfoValid);
                }

                //——Property——
                if (targetMemberType == MemberType.Property)
                {
                    List<string> listPropertyOption = new List<string>() { ReflectionValueHolderBase.emptyMemberName };
                    foreach (PropertyInfo propertyInfo in targetType.GetAllPropertys(ReflectionValueHolderBase.defaultBindingFlags))
                    {
                        if (_target.IsTypeMatch(propertyInfo.PropertyType))
                            listPropertyOption.Add(propertyInfo.Name);
                    }
                    DrawPopUp(new GUIContent("Property"), listPropertyOption,
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
                    List<string> listGetMethodOption = new List<string>() { ReflectionValueHolderBase.emptyMemberName };
                    List<string> listSetMethodOption = new List<string>() { ReflectionValueHolderBase.emptyMemberName };
                    foreach (MethodInfo methodInfo in targetType.GetAllMethods(ReflectionValueHolderBase.defaultBindingFlags))
                    {
                        if (_target.IsDesireGetMethod(methodInfo))
                            listGetMethodOption.Add(methodInfo.Name);

                        if (_target.IsDesireSetMethod(methodInfo))
                            listSetMethodOption.Add(methodInfo.Name);
                    }

                    //分别记录两种方法的有效性
                    bool isMemberInfoValid_Get = false;
                    bool isMemberInfoValid_Set = false;
                    DrawPopUp(new GUIContent("GetMethod"), listGetMethodOption,
                        () => _target.TargetSerializeGetMethodName,
                        (val) => _target.TargetSerializeGetMethodName = val,
                        ref isMemberInfoValid_Get);
                    DrawPopUp(new GUIContent("SetMethod"), listSetMethodOption,
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
            else
            {
                EditorGUILayout.HelpBox("Please specify Target!", MessageType.Warning);
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();//应用修改

                if (_target.Target != lastObject)//如果用户更改了目标：清空其他字段并保存
                {
                    _target.TargetSerializeFieldName =
                    _target.TargetSerializePropertyName =
                    _target.TargetSerializeGetMethodName =
                    _target.TargetSerializeSetMethodName = "";
                    Undo.RecordObject(target, "Clear Old Data");//保存到Undo
                }
            }
        }

        public virtual void OnInspectorGUIFunc()
        {
            //base.OnInspectorGUI();
        }



        static string missingMemberInfoFormat = "<Missing {0}>";
        /// <summary>
        /// 
        /// PS:
        /// -如果上次选中值不在可选项中，则在Popup上提示
        /// </summary>
        /// <param name="gUIContent"></param>
        /// <param name="listOption">首位一定是 emptyMemberName</param>
        /// <param name="lastSelect"></param>
        /// <returns></returns>
        private int DrawPopUp(GUIContent gUIContent, List<string> listOption, CustomFunc<string> getter, UnityAction<string> setter, ref bool isCurValueValid)
        {
            string lastSelect = getter();

            bool isNameExist = true;
            int lastIndex = 0;
            if (lastSelect.IsNullOrEmpty())//空：代表尚未设置
            {
                isNameExist = true;//代表可能存在
                lastIndex = 0;//对应emptyMemberName，提示需要设置，不算报错
            }
            else
            {
                //检查上次选中的序号是否不存在（可能是物体引用丢失，或者Member改名）。如果不存在：把该名称添加到列表最后并提示丢失
                isNameExist = listOption.Contains(lastSelect);
                if (!isNameExist)
                {
                    listOption.Add(lastSelect);
                }
                lastIndex = listOption.IndexOf(lastSelect);//上次选中的序号（如果已经丢失，则重置为0）
            }

            //# 绘制Popup
            string[] arrayOption = listOption.ToArray();
            if (!isNameExist)
            {
                arrayOption[lastIndex] = string.Format(missingMemberInfoFormat, arrayOption[lastIndex]);//标记该MemberInfo为Missing（，参考UnityEvent的样式）
            }
            int curIndex = EditorGUILayout.Popup(gUIContent, lastIndex, arrayOption);
            if (lastIndex != curIndex)//检查变化并保存
            {
                Undo.RecordObject(target, "ChangePopUp" + lastSelect);
                setter(listOption[curIndex]);
            }

            isCurValueValid = isNameExist && (curIndex != 0);//检查当前选中值是否有效
            return curIndex;
        }
    }
}
#endif