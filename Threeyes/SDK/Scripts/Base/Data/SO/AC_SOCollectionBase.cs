using System.Collections;
using UnityEngine;
/// <summary>
///  Mapping SO by Enum
/// </summary>
/// <typeparam name="TEnum"></typeparam>
/// <typeparam name="TSO"></typeparam>
public abstract class AC_SOCollectionBase<TEnum, TSO> : SOCollectionBase
where TEnum : System.Enum
where TSO : ScriptableObject
{
    /// <summary>
    /// Get the relate SO
    /// </summary>
    /// <param name="en"></param>
    /// <returns>null if not exist</returns>
    public abstract TSO this[TEnum en] { get; }

    #region IEnumerable
    public override IEnumerator GetEnumerator()
    {
        foreach (TEnum item in System.Enum.GetValues(typeof(TEnum)))
        {
            yield return this[item];
        }
    }
    #endregion
}