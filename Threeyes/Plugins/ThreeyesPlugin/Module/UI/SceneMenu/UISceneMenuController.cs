using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISceneMenuController : UIContentPanelBase<UISceneButton, SOSceneInfo>
{
    //如果ListData为空，那就优先使用该数据
    [Header("优先使用SOSceneInfoGroup中的场景列表")]
    public SOSceneInfoGroup soSceneInfoGroup;

    #region Public

    [ContextMenu("ClickCurrentButton")]
    public void ClickCurrentButton()
    {
        if (!IsShowing)
            return;

        if (listUIElement[CurIndex])
            listUIElement[CurIndex].OnClick();
    }

    #endregion


    #region Override 

    public override void InitUI()
    {
        //初始化ListData
        if (soSceneInfoGroup)
            ListData = soSceneInfoGroup.ListData;

        base.InitUI();
    }

    protected override bool SetFunc(int index)
    {
        listUIElement[index].button.SelectWithoutNotify();
        //listUIElement[index].button.Select();//Select会调用Click事件
        return base.SetFunc(index);
    }

    #endregion
}
