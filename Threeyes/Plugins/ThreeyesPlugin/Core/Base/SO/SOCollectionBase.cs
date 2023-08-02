using System.Collections;
using UnityEngine;
/// <summary>
/// 
/// </summary>
public abstract class SOCollectionBase : ScriptableObject
{
    //使SODataGroupBase可以使用foreach（PS：遍历时可以通过指定子元素类型进行转换）（PS：Newtonsoft.Json不支持继承IEnumerable的数据类，否则会报错（https://stackoverflow.com/questions/19080326/deserialize-to-ienumerable-class-using-newtonsoft-json-net））
    public abstract IEnumerator GetEnumerator();
}

/// <summary>
///  Mapping SO by Enum
/// </summary>
/// <typeparam name="TEnum"></typeparam>
/// <typeparam name="TSO"></typeparam>
public abstract class SOCollectionBase<TEnum, TSO> : SOCollectionBase
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