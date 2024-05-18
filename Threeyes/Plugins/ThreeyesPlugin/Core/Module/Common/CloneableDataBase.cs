using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace Threeyes.Core
{
    public interface ICloneableData : ICloneable
    {
        TInst Clone<TInst>()
            where TInst : class;
    }


    /// <summary>
    /// Make this data cloneable
    /// </summary>
    public abstract class CloneableDataBase : ICloneable, ICloneableData
    {
        #region Editor Utility

        /// <summary>
        /// Warning:
        /// -（仅执行浅层次克隆，针对引用类型（如AnimationCurve）需要子类自行克隆）
        /// </summary>
        /// <returns></returns>
        public virtual object Clone()
        {
            //return ReflectionTool.DoCopy(this);//Warning/ToDelete：部分字段（如 AnimationCurve）可能会报错或闪退，而且部分平台不支持反射，暂缓使用，改为由子类自行实现
            return this.MemberwiseClone();//The MemberwiseClone method creates a shallow copy by creating a new object, and then copying the nonstatic fields of the current object to the new object. If a field is a value type, a bit-by-bit copy of the field is performed. If a field is a reference type, the reference is copied but the referred object is not; therefore, the original object and its clone refer to the same object.（仅执行浅层次克隆，针对引用类型（如AnimationCurve）需要自行克隆）
        }

        /// <summary>
        /// Copy data to other instance
        /// (PS: only clone struct and reference. For class type instance, you should create new instance using their construct function(eg: AnimationCurve(params Keyframe[] keys))
        /// </summary>
        /// <typeparam name="TInst"></typeparam>
        /// <returns></returns>
        public virtual TInst Clone<TInst>()
            where TInst : class
        {
            return (TInst)Clone();
        }

        #endregion
    }
}