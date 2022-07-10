using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
/// <summary>
/// 对话框
/// </summary>
[CreateAssetMenu(menuName = "SO/TipsInfo/DialogTipsInfo")]
public class SODialogTipsInfo : SOTipsInfo
{
    [Header("Dialog")]
    public DialogType dialogType = DialogType.Null;

    public string strConfirm="好的";
    public string strCancel ="取消";


    public enum DialogType
    {
        Null,//无选择项
        Confirm,//确认
        ConfirmOrCancel,//确认或取消
    }
}
[System.Serializable]
public class DialogTipsInfoEvent : UnityEvent<SODialogTipsInfo>
{

}