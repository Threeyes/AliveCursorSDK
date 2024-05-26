#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using UnityEngine.Events;

namespace Threeyes.Core
{
    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class InspectorView_ReflectionMemberHolder<T> : UnityEditor.Editor
        where T : ReflectionMemberHolderBase
    {
        //#Runtime
        protected T _target;
        Object lastTargetObj, curTargetObject;
        protected virtual void OnEnable()
        {
            _target = (T)target;
        }

        protected const string warningText_PleaseSetMemberInfo = "Please set all necessary member information first!";
        public override void OnInspectorGUI()
        {
            lastTargetObj = _target.Target;//缓存目标

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("target"));
            if (!lastTargetObj)//如果目标为空：提示警告
            {
                EditorGUILayout.HelpBox("Please specify Target!", MessageType.Warning);
            }

            OnInspectorGUIFunc();

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();//应用修改

                curTargetObject = _target.Target;
                if (curTargetObject != lastTargetObj)//用户更改了目标：清空其他字段并保存
                {
                    OnFieldTargetChanged();
                }
            }
        }

        protected abstract void OnInspectorGUIFunc();

        /// <summary>
        /// 当Target字段改变
        /// </summary>
        protected abstract void OnFieldTargetChanged();


        #region Utility
        static string missingMemberInfoFormat = "<Missing {0}>";

        /// <summary>
        /// 绘制下拉框，以便提供可用的元素
        /// 
        /// ToUpdate:
        /// -应该是提供类似UnityEvent的菜单，能够显示该物体所有组件可用的方法
        /// 
        /// PS:
        /// -如果上次选中值不在可选项中，则在Popup上提示
        /// </summary>
        /// <param name="gUIContent"></param>
        /// <param name="listOption">首位一定是 emptyMemberName</param>
        /// <param name="lastSelect"></param>
        /// <param name="getter"></param>
        /// <param name="setter">Fire when value changed</param>
        /// <returns></returns>
        protected static int DrawPopUp(Object target, GUIContent gUIContent, List<string> listOption, CustomFunc<string> getter, UnityAction<string> setter, ref bool isCurValueValid)
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
        #endregion
    }
}
#endif