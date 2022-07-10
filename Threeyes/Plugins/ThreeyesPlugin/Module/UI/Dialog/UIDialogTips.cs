using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
/// <summary>
/// 带按键交互的对话框UI
/// </summary>
public class UIDialogTips : UITips
{
    [Header("对话框")]
    public Button butConfirm;
    public Button butCancel;
    public Text textConfirm;
    public Text textCancel;

    public BoolEvent onConfirmCancel;
    public UnityEvent onConfirm;
    public UnityEvent onCancel;

    public UnityAction<bool> actConfirmCancel;
    public UnityAction actHide;

    public bool isHideOnConfirm = true;
    public bool isHideOnCancel = true;

    public BoolEvent onHasConfrimCancelUI;//是否包含 确认/取消 键，适用于设置UI组

    /// <summary>
    /// 尝试转换为特定的TipsInfo
    /// </summary>
    public SODialogTipsInfo DialogTipsInfo
    {
        get
        {
            if (TipsInfo)
            {
                var tips = TipsInfo as SODialogTipsInfo;
                if (tips)
                    return tips;
            }
            return null;
        }

        set
        {
            tipsInfo = value;
        }
    }
    protected override void Start()
    {
        base.Start();

        if (butCancel)
            butCancel.onClick.AddListener(OnButtonCancelClick);
        if (butConfirm)
            butConfirm.onClick.AddListener(OnButtonConfirmClick);
    }

    private void OnButtonConfirmClick()
    {
        if (!DialogTipsInfo)
            return;

        onConfirm.Invoke();
        onConfirmCancel.Invoke(true);
        if (actConfirmCancel != null)
            actConfirmCancel.Invoke(true);

        if (isHideOnConfirm)
            Hide();
    }

    private void OnButtonCancelClick()
    {
        if (!DialogTipsInfo)
            return;

        onCancel.Invoke();
        onConfirmCancel.Invoke(false);
        if (actConfirmCancel != null)
            actConfirmCancel.Invoke(false);

        if (isHideOnCancel)
            Hide();
    }

    public override void Show(bool isShow)
    {
        base.Show(isShow);
        if (!isShow)
        {
            if (actHide != null)
                actHide.Invoke();
        }
    }

    protected override void SetTipsFunc(SOTipsInfo tipsInfo, bool isPlayAudio)
    {
        base.SetTipsFunc(tipsInfo, isPlayAudio);

        if (!DialogTipsInfo)//可能未赋值，或者是父类TipsInfo
        {
            onHasConfrimCancelUI.Invoke(false);
            return;
        }

        onHasConfrimCancelUI.Invoke(DialogTipsInfo.dialogType != SODialogTipsInfo.DialogType.Null);
        //设置UI
        if (DialogTipsInfo)
        {
            if (butConfirm)
                butConfirm.gameObject.SetActive(DialogTipsInfo.dialogType == SODialogTipsInfo.DialogType.Confirm || DialogTipsInfo.dialogType == SODialogTipsInfo.DialogType.ConfirmOrCancel);
            if (butCancel)
                butCancel.gameObject.SetActive(DialogTipsInfo.dialogType == SODialogTipsInfo.DialogType.ConfirmOrCancel);
            if (textConfirm)
                textConfirm.text = DialogTipsInfo.strConfirm;
            if (textCancel)
                textCancel.text = DialogTipsInfo.strCancel;
        }
        else
        {
            butConfirm.gameObject.SetActive(false);
            butCancel.gameObject.SetActive(false);
        }
    }

}
