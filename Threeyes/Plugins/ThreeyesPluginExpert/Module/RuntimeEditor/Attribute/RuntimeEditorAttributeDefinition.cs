using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
namespace Threeyes.RuntimeEditor
{
    ///——标记类/类成员是否可以编辑（参考Newtonsoft.Json的命名规则）——
    public sealed class RuntimeEditorObjectAttribute : Attribute
    {
        public MemberConsideration MemberConsideration { get => _memberConsideration; set => _memberConsideration = value; }
        private MemberConsideration _memberConsideration = MemberConsideration.OptOut;

        public RuntimeEditorObjectAttribute() { }

        public RuntimeEditorObjectAttribute(MemberConsideration memberConsideration)
        {
            _memberConsideration = memberConsideration;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false)]
    public class RuntimeEditorPropertyAttribute : Attribute
    {
        public string PropertyName { get; set; }

        public RuntimeEditorPropertyAttribute()
        {
        }

        public RuntimeEditorPropertyAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }

    /// <summary>
    /// Mark the field/property is not editable
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class RuntimeEditorIgnoreAttribute : Attribute
    {
    }


    //
    // 摘要:
    //     Specifies the member include options for the RuntimeEditorSerializer.
    public enum MemberConsideration
    {
        /// <summary>
        /// All  members are included by default. Members can be excluded using [RuntimeEditorIgnoreAttribute]. This is the default member serialization mode.
        /// 
        ///ToUpdate:UIRuntimeEditorFactory暂未扫描Property，后期完善。整体按照Json的实现，避免分支
        /// </summary>
        OptOut,
        /// <summary>
        /// Only members marked with [RuntimeEditorPropertyAttribute] are included.
        /// </summary>
        OptIn,

        /// <summary>
        /// All public and private fields are included. Members can be excluded using [RuntimeEditorIgnoreAttribute].
        /// </summary>
        Fields
    }


    /////——增加按钮（参考NaughtyAttributes）——


    /// <summary>
    /// 将方法暴露成按钮。
    /// 
    /// PS：
    /// -当前仅支持无参数。
    /// 
    /// Todo：
    /// -【V2】f后期可支持多参数方法，可通过nameof指定类中的成员作为参数，参考PersistentAssetFilePathAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RuntimeEditorButtonAttribute : Attribute
    {
        public string Text { get; private set; }
        public RuntimeEditorButtonAttribute(string text = null)
        {
            this.Text = text;
        }

        /// <summary>
        /// Check if target method valid
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        public bool IsValid(MethodInfo methodInfo)
        {
            if (methodInfo == null)
                return false;

            return methodInfo.GetParameters().Length == 0;
        }

        public void InvokeMethod(MethodInfo methodInfo, object obj)
        {
            if (methodInfo == null)
                return;

            methodInfo.Invoke(methodInfo.IsStatic ? null : obj, new object[] { });
        }
    }
}