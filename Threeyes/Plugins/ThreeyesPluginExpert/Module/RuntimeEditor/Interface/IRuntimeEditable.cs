using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Threeyes.Data;
using UnityEngine;
namespace Threeyes.RuntimeEditor
{
    /// <summary>
    /// Mark as Editable on Runtime
    /// </summary>
    public interface IRuntimeEditable
    {
        /// <summary>
        /// 可选项
        /// </summary>
        FilePathModifier FilePathModifier { get; }


        /// <summary>
        /// 返回需要序列化指定实例的Member信息清单，用于后续运行时编辑数据
        /// 
        /// PS:
        /// -因为Modder无法使用Reflection，所以需要[nameof(XXX)]返回成员的名称
        /// -可以通过返回其他相关联组件（如- URP相机的UniversalAdditionalCameraData）的Member，实现在一个组件中统一管理其他的数据
        /// </summary>
        /// <returns></returns>
        List<RuntimeEditableMemberInfo> GetListRuntimeEditableMemberInfo();
    }

    /// <summary>
    /// Info for member
    /// </summary>
    public class RuntimeEditableMemberInfo
    {
        public object obj;//The object whose member value will be returned. For non-static fields, obj should be an instance of a class that inherits or declares the field.（静态类可空）
        public Type objType;//Type of obj
        public string memberName;//name of member

        public RuntimeEditableMemberInfo() { }

        public RuntimeEditableMemberInfo(object obj, Type objType, string memberName)
        {
            this.obj = obj;
            this.objType = objType;
            this.memberName = memberName;
        }
    }
}