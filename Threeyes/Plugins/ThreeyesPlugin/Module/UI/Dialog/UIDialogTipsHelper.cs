using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 用于保存确认和取消的事件
/// 
/// 注意：泛型设置为SOTipsInfo，能够兼容更多的类型
/// </summary>
public class UIDialogTipsHelper : UITipsHelperBase<UIDialogTips, SOTipsInfo>
{
    public BoolEvent onConfirmCancel;
    public UnityEvent onConfirm;
    public UnityEvent onCancel;

    public bool isHideOnConfirm = true;
    public bool isHideOnCancel = true;

    public override void SetTips(SOTipsInfo tipsInfo)
    {
        Comp.isHideOnConfirm = isHideOnConfirm;
        Comp.isHideOnCancel = isHideOnCancel;

        base.SetTips(tipsInfo);

        Comp.actConfirmCancel += OnConfirmCancel;
        Comp.actHide += OnHide;

        ////设置事件(应该是监听）
        //Comp.onConfirmCancel = onConfirmCancel;
        //Comp.onConfirm = onConfirm;
        //Comp.onCancel = onCancel;
    }

    public void OnConfirmCancel(bool isOn)
    {
        onConfirmCancel.Invoke(isOn);
        if (isOn)
        {
            onConfirm.Invoke();
        }
        else
        {
            onCancel.Invoke();
        }
    }

    public void OnHide()
    {
        Comp.actConfirmCancel -= OnConfirmCancel;
        Comp.actHide -= OnHide;
    }

    public override void Hide()
    {
        base.Hide();
        Comp.onConfirmCancel.RemoveAllListeners();
        Comp.onConfirm.RemoveAllListeners();
        Comp.onCancel.RemoveAllListeners();
    }
}
