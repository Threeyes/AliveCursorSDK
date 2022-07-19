#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;
namespace Threeyes.ValueHolder
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ReflectionValueHolderBase), true)]//editorForChildClasses
    public class InspectorView_ReflectionValueHolder : UnityEditor.Editor
    {
        private ReflectionValueHolderBase _target;
        private void OnEnable()
        {
            _target = (ReflectionValueHolderBase)target;
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            Object targetObject = _target.target;
            if (targetObject)
            {
                System.Type targetType = targetObject.GetType();
                //Ĭ��Ϊ��
                List<string> listFieldOption = new List<string>() { ReflectionValueHolderBase.emptyMemberName };
                List<string> listPropertyOption = new List<string>() { ReflectionValueHolderBase.emptyMemberName };
                List<string> listGetMethodOption = new List<string>() { ReflectionValueHolderBase.emptyMemberName };
                List<string> listSetMethodOption = new List<string>() { ReflectionValueHolderBase.emptyMemberName };

                //�г����ж�Ӧ��ֵ
                foreach (FieldInfo fieldInfo in targetType.GetAllFields(ReflectionValueHolderBase.defaultBindingFlags))
                {
                    if (_target.IsTypeMatch(fieldInfo.FieldType))
                        listFieldOption.Add(fieldInfo.Name);
                }
                DrawPopUp(new GUIContent("Field"), listFieldOption, ref _target.targetSerializeFieldName);

                foreach (PropertyInfo propertyInfo in targetType.GetAllPropertys(ReflectionValueHolderBase.defaultBindingFlags))
                {
                    if (_target.IsTypeMatch(propertyInfo.PropertyType))
                        listPropertyOption.Add(propertyInfo.Name);
                }
                DrawPopUp(new GUIContent("Property"), listPropertyOption, ref _target.targetSerializePropertyName);
                //��ʾProperty�Ƿ���� get/set 
                if (_target.targetSerializePropertyName.NotNullOrEmpty())
                {
                    PropertyInfo propertyInfoCurSelect = targetType.GetProperty(_target.targetSerializePropertyName);
                    if (propertyInfoCurSelect != null)
                    {
                        string warningTips = null;
                        if (!propertyInfoCurSelect.CanRead)
                            warningTips += "Write Only";
                        if (!propertyInfoCurSelect.CanWrite)
                            warningTips += "Read Only";
                        if (warningTips.NotNullOrEmpty())
                            EditorGUILayout.HelpBox(warningTips, MessageType.Warning);
                    }
                }

                foreach (MethodInfo methodInfo in targetType.GetAllMethods(ReflectionValueHolderBase.defaultBindingFlags))
                {
                    if (_target.IsDesireGetMethod(methodInfo))
                        listGetMethodOption.Add(methodInfo.Name);

                    if (_target.IsDesireSetMethod(methodInfo))
                        listSetMethodOption.Add(methodInfo.Name);
                }


                DrawPopUp(new GUIContent("GetMethod"), listGetMethodOption, ref _target.targetSerializeGetMethodName);
                DrawPopUp(new GUIContent("SetMethod"), listSetMethodOption, ref _target.targetSerializeSetMethodName);
            }
            else
            {
                EditorGUILayout.HelpBox("Please specify Target!", MessageType.Warning);

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gUIContent"></param>
        /// <param name="listOption"></param>
        /// <param name="lastSelect">������ڿ�ѡ���У������ֵ</param>
        /// <returns></returns>
        private int DrawPopUp(GUIContent gUIContent, List<string> listOption, ref string lastSelect)
        {
            bool isNameExist = listOption.Contains(lastSelect);
            if (!isNameExist)
                lastSelect = null;//��������ڣ�����գ��������������ö�ʧ������Member������
            int lastIndex = isNameExist ? listOption.IndexOf(lastSelect) : 0;
            int curIndex = EditorGUILayout.Popup(gUIContent, lastIndex, listOption.ToArray());
            if (lastIndex != curIndex)
            {
                Undo.RecordObject(target, "DrawPopUp" + lastSelect);
                lastSelect = listOption[curIndex];
            }
            return curIndex;
        }
    }
}
#endif