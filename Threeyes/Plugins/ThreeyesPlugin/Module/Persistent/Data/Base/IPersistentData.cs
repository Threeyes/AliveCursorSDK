using System;
using Threeyes.Data;
using Threeyes.External;
using UnityEngine;
using UnityEngine.Events;

namespace Threeyes.Persistent
{

    public partial interface IPersistentData : IDisposable
    {
        /// <summary>
        /// 判断Key、Option等是否有效
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// The real type of Value
        /// PS: TValue may be the base type if the this class is for common usage (eg: Enum, SO))
        /// </summary>
        Type ValueType { get; }

        /// <summary>
        /// 唯一标识（针对每个Controller管理的区域内（如文件夹））
        /// </summary>
        string Key { get; set; }
        string Tooltip { get; set; }

        /// <summary>
        /// The parent dir path for this PersistentData file (Setup on [Runtime])
        /// </summary>
        string PersistentDirPath { get; set; }

        /// <summary>
        /// Controller初始化时主动调用
        /// </summary>
        void Init();
    }

    /// <summary>
    /// 存储持久化数据，适用于简单数据
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public interface IPersistentData<TValue> : IPersistentData
    {
        UnityEvent<TValue> EventOnValueChanged { get; }// Notify value changed
        UnityEvent<TValue> EventOnUIChanged { get; }// Notify relate UI change

        TValue DefaultValue { get; set; }
        TValue PersistentValue { get; set; }
        TValue ValueToSaved { get; }
        bool SaveAnyway { get; }
        bool HasChanged { get; set; }
        bool HasLoadedFromExtern { get; set; }
        void OnValueChanged(TValue value, PersistentChangeState persistentChangeState);
        void OnUIChanged(TValue value);
        void OnDefaultValueSaved();
    }

    /// <summary>
    /// 
    /// PS:接口传入的string代表路径
    /// </summary>
    public interface IPersistentData_File : IPersistentData<string>, IFilePathModifierHolder
    {
        event UnityAction<ExternalResources.LoadResult, object> AssetChanged;//(loadResult, defaultAsset)
    }

    /// <summary>
    /// 将外部资源转为Unity内部格式
    /// </summary>
    public interface IPersistentData_File<TAsset> : IPersistentData_File
    where TAsset : UnityEngine.Object
    {
        TAsset PersistentAsset { get; set; }
        TAsset DefaultAsset { get; set; }
        UnityEvent<TAsset> EventOnAssetChanged { get; }
    }

    public interface IPersistentData_ComplexData<TValue> : IPersistentData<TValue>, IFilePathModifierHolder
    {
        TValue TargetValue { get; set; }
    }
    public interface IPersistentData_Object : IPersistentData_ComplexData<object>
    {
    }
    /// <summary>
    /// 初始化IPersistentData_Object
    /// </summary>
    public interface IPersistentData_ObjectInitializer
    {
        object BaseTargetValue { get; }
        Type ValueType { get; }
    }

    public interface IPersistentData_UnityObject : IPersistentData_ComplexData<UnityEngine.Object>
    {
    }
    public interface IPersistentData_SO : IPersistentData_ComplexData<ScriptableObject>
    {
    }

    public enum PersistentChangeState
    {
        Load,//Init
        Set,//Update
        Delete,//Delete
    }

    public class FilePathModifier_PD : FilePathModifier
    {
        /// <summary>
        /// PS:
        /// -使用PersistentDirPath作为parentDir
        /// -使用Key作为SubDir
        /// </summary>
        /// <param name="persistentData"></param>
        public FilePathModifier_PD(IPersistentData persistentData) : base(persistentData.PersistentDirPath, persistentData.Key) { }
    }
}