using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if USE_JsonDotNet
using Newtonsoft.Json;
#endif

/// <summary>
/// Contain same type of Data
/// 
/// PS:
/// 1.因为含有非通用代码，而且使用了宏定义，所以不能放到Core文件夹中
/// </summary>
/// <typeparam name="TData"></typeparam>
public class SOGroupBase<TData> : SOCollectionBase
{
    [JsonIgnore] public virtual List<TData> ListData { get { return listData; } set { listData = value; } }

    [JsonProperty("内容列表")] [SerializeField] protected List<TData> listData = new List<TData>();

    public override IEnumerator GetEnumerator()
    {
        return ListData.GetEnumerator();
    }

    #region VersionUpdate
    protected const string outdateWarningTips = "Param is outdated! Please use ListData instead! (Will be auto convert to newer version)";
    #endregion
}

