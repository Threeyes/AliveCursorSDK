using System;
using UnityEngine.Events;

namespace Threeyes.Persistent
{
    /// <summary>
    /// Manage PersistentData
    /// </summary>
    public interface IPersistentController : IDisposable
    {
        IPersistentData PersistentData { get; }
        PersistentControllerOption BaseOption { get; }
        bool HasKey { get; }

        /// <summary>
        /// Load value from source and notify events
        /// </summary>
        void LoadValue();

        /// <summary>
        /// Save default value
        /// (Useful for save the default data into storage, so that the user can modify it later)
        /// </summary>
        void SaveDefaultValue();

        /// <summary>
        /// Save current value to source
        /// </summary>
        void SaveValue();

        /// <summary>
        /// Delete key from source
        /// </summary>
        void DeleteKey();
    }

    public interface IPersistentController<TValue> : IPersistentController
    {
        event UnityAction<TValue> ValueChanged;
        event UnityAction<TValue> UIChanged;
        IPersistentData<TValue> RealPersistentData { get; }

        TValue PersistentValue { get; }
        /// <summary>
        /// Load Value with overrideDefaultValue
        /// </summary>
        /// <param name="overrideDefaultValue"></param>
        void LoadValue(TValue overrideDefaultValue);

        /// <summary>
        /// Set new value (Invoke by external, eg: UIToggle)
        /// </summary>
        /// <param name="value"></param>
        void SetValue(TValue value);

        /// <summary>
        /// Get value from source
        /// </summary>
        /// <returns></returns>
        TValue GetValue();
    }
}