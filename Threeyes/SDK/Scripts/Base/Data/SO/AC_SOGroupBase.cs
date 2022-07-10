using System.Collections;
using System.Collections.Generic;
#if USE_JsonDotNet
using Newtonsoft.Json;
using UnityEngine;
#endif

public class AC_SOGroupBase<TData> : SOCollectionBase
{
    [JsonIgnore] public virtual List<TData> ListData { get { return listData; } set { listData = value; } }
    [SerializeField] protected List<TData> listData = new List<TData>();

    public override IEnumerator GetEnumerator()
    {
        return ListData.GetEnumerator();
    }
}
