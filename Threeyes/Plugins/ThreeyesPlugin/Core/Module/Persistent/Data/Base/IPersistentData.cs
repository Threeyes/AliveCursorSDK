using System;
using Threeyes.Data;
using Threeyes.External;
using UnityEngine;
using UnityEngine.Events;

namespace Threeyes.Persistent
{
    public partial interface IPersistentData : IDisposable
    {
        string Key { get; set; }//唯一标识（针对每个Controller管理的区域内（如文件夹））
        string Tooltip { get; set; }
        IDataOption BaseDataOption { get; }
        bool IsValid { get; }//用于判断Option等设置是否有效
        string PersistentDirPath { get; set; }//The parent dir path for this PersistentData file (Setup on [Runtime])

        void Init();//Controller初始化时调用
    }

    /// <summary>
    /// 存储持久化数据
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public interface IPersistentData<TValue> : IPersistentData
    {
        UnityEvent<TValue> EventOnValueChanged { get; }// Notify value changed
        UnityEvent<TValue> EventOnUIChanged { get; }// Notify relate UI change

        /// <summary>
        /// The real type of Value
        /// PS: TValue may be the base type if the this class is for common usage (eg: Enum, SO))
        /// </summary>
        Type RealValueType { get; }

        TValue DefaultValue { get; set; }
        TValue PersistentValue { get; set; }
        bool HasChanged { get; set; }

        void OnValueChanged(TValue value, PersistentChangeState persistentChangeState);
        void OnUIChanged(TValue value);
        void OnDefaultValueSaved();
    }

    public interface IPersistentData_File : IPersistentData
    {
        FilePathModifier FilePathModifier { get; }

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

    public interface IPersistentData_SO : IPersistentData
    {
        FilePathModifier FilePathModifier { get; }
        ScriptableObject TargetValue { get; set; }
    }

    public enum PersistentChangeState
    {
        Load,
        Set,
        Delete,
    }


    public class FilePathModifier_PD : FilePathModifier
    {
        public FilePathModifier_PD(IPersistentData persistentData) : base(persistentData.PersistentDirPath, persistentData.Key) { }//使用Key作为SubDir
    }
}