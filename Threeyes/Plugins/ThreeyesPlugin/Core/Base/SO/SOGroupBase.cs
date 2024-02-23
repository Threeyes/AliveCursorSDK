using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if USE_JsonDotNet
using Newtonsoft.Json;
#endif

namespace Threeyes.Core
{

    /// <summary>
    /// Contain same type of Data
    /// 
    /// Warning:
    /// 1.改为作为Core类，非通用的参数（如自定义JsonProperty名称）不应该存在，若有需要应自行创建类似的父类
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public class SOGroupBase<TData> : SOCollectionBase
    {
        [JsonIgnore] public virtual List<TData> ListData { get { return listData; } set { listData = value; } }
        [SerializeField] protected List<TData> listData = new List<TData>();

        public override IEnumerator GetEnumerator()
        {
            return ListData.GetEnumerator();
        }

        #region VersionUpdate
        protected const string outdateWarningTips = "Param is outdated! Please use ListData instead! (Will be auto convert to newer version)";
        #endregion
    }
}