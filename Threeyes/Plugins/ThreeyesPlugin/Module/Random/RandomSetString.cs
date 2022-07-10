using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RandomSetString : MonoBehaviour
{
    public StringEvent onSetValue;

    public List<string> listValue = new List<string>();//所有可用的值



    [ContextMenu("SetValue")]
    public void SetValue()
    {
        OnSet();
    }

    protected virtual void OnSet()
    {
        Debug.Assert(listValue.Count > 0);

        string newValue = listValue.GetRandom();
        onSetValue.Invoke(newValue);
    }

    #region Utility

    [Header("Editor")]
    public int editorTotalListCount = 10;//用于自动化生成
    [ContextMenu("AutoSetListValue")]
    public void AutoSetSeqValue()
    {
        string firstValue = listValue.FirstOrDefault();
        listValue.Clear();
        for (int i = 0; i != editorTotalListCount; i++)
        {
            listValue.Add(firstValue + "0" + i.ToString());
        }
    }

    #endregion

}
