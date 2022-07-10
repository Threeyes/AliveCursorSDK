using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
/// <summary>
/// 存储多选/单选选项集
/// Todo:可以通过预设批量设置子物体的图像
/// </summary>
[DefaultExecutionOrder(200)]//迟于InterfaceAnimManager执行
public class UIOptionTips : UITipsBase<SOOptionTipsInfo>
{
    [Header("事件")]
    public StringEvent onSetOptionType;//设置题目类型名称（单选题、多选题等）
    public BoolEvent onJuege;//选中的答案是否正确
    public BoolEvent onSelectAll;//是否选中指定数量的答案，适用于选中所有后才能激活【下一题】按钮


    [Header("按钮")]
    public Transform tfButtonPanel;
    public CanvasGroup cgButtonPanel;//管理UI的可交互
    public GameObject preOptionElement;

    [Header("Config")]
    public bool isAutoJudge = true;//True：达到触发条件后立即判断；False：需要手动调用判断
    public AutoJudgeTriggerType autoJudgeTrigger = AutoJudgeTriggerType.NumberMatchedOrSelectWrong;

    //Runtime
    List<UIOptionButton> listUIOptionButton = new List<UIOptionButton>();

    #region Override


    /// <summary>
    /// 从自身或Desire获取数据(自身的数据作为Override)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="getFunc"></param>
    /// <returns></returns>
    T GetTargetFromSelfOrDesire<T>(Func<SOTipsInfo, T> getFunc, Func<T, bool> JudgeIfNotNull)
    {
        if (tipsInfo)
        {
            if (JudgeIfNotNull(getFunc(tipsInfo)))
                return getFunc(tipsInfo);
            else
            {
                if (tipsInfo.tipsInfoDesire)//返回目标选项的Sprite
                {
                    if (JudgeIfNotNull(getFunc(tipsInfo.tipsInfoDesire)))
                        return getFunc(tipsInfo.tipsInfoDesire);
                }
            }
        }
        return default(T);
    }

    protected override string GetTips()
    {
        return GetTargetFromSelfOrDesire((tips) => tips.tips, (str) => !str.IsNullOrEmpty());
    }
    protected override Sprite GetSprite()
    {
        return GetTargetFromSelfOrDesire((tips) => tips.sprite, (spr) => spr != null);
    }

    protected override void SetTipsFunc(SOOptionTipsInfo tipsInfo, bool isPlayAudio)
    {
        if (!Application.isPlaying)
            return;

        //Init
        listUIOptionButton.Clear();

        base.SetTipsFunc(tipsInfo, isPlayAudio);

        if (tipsInfo)
        {
            if (tfButtonPanel && preOptionElement)
            {
                //Init Choice List
                tfButtonPanel.DestroyAllChild();
                ToggleGroup toggleGroup = tfButtonPanel.GetComponent<ToggleGroup>();
                List<SOTipsInfo> listOptionData = tipsInfo.ListTipsInfo.SimpleClone();

                if (tipsInfo.isShuffle)
                    listOptionData.Shuffle();

                for (int i = 0; i != listOptionData.Count; i++)
                {
                    OptionData optionData = new OptionData()
                    {
                        optionTipsInfo = tipsInfo,
                        tipsInfo = listOptionData[i],
                        colorIsDesire = tipsInfo.colorIsDesire,
                        colorNotDesire = tipsInfo.colorNotDesire,
                        strIndex = tipsInfo.GetStrIndex(i),
                        displayContentOverride = tipsInfo.optionDisplayContent
                    };

                    Transform tfChoiceButton = preOptionElement.InstantiatePrefab(tfButtonPanel).transform;

                    UIOptionButton uIOptionButton = tfChoiceButton.GetComponent<UIOptionButton>();
                    uIOptionButton.Init(optionData);
                    uIOptionButton.Index = i;

                    uIOptionButton.toggle.onValueChanged.AddListener((isOn) => OnSelectElement(uIOptionButton));

                    if (tipsInfo.optionType == SOOptionTipsInfo.OptionType.SingleChoice || tipsInfo.optionType == SOOptionTipsInfo.OptionType.Judgment)
                    {
                        uIOptionButton.toggle.group = toggleGroup;//强制只能单选
                    }
                    listUIOptionButton.Add(uIOptionButton);
                }
            }
            else
            {
                Debug.LogError("tfButtonPanel or preChoiceButton is Null!");
            }

            onSetOptionType.Invoke(tipsInfo.strOptionType);
        }

        //Init UI Finish
        if (cgButtonPanel)
            cgButtonPanel.interactable = true;

    }

    #endregion

    /// <summary>
    /// （外部调用）选中指定按键
    /// </summary>
    /// <param name="selectIndex"></param>
    public void SelectElement(int selectIndex)
    {
        if (selectIndex < listUIOptionButton.Count)
        {
            var selectedUIButton = listUIOptionButton[selectIndex];
            OnSelectElement(selectedUIButton);
        }
    }

    private void OnSelectElement(UIOptionButton uiOptionButton)
    {
        if (uiOptionButton.hasUpdateUI)
            return;

        try
        {
            bool isCurSelectCorrect = false;//当前按键是否选择正确（ToUpdate判断）
            bool isCountMatch = false;//是否数量已经选全(不管对错）
            bool isAllMatch = false;//是否已经选对
            isCurSelectCorrect = tipsInfo.IsDesire(ref isAllMatch, ref isCountMatch, GetAllSelected());//PS:会自动根据类型判断所选是否正确

            //满足AutoJudge触发条件后，立即调用Judge方法
            if (isAutoJudge)
            {
                ///ToAdd：应该是针对选中的Button，调用其UpdateUI
                bool shouldAutoJudge = false;
                switch (autoJudgeTrigger)
                {
                    case AutoJudgeTriggerType.NumberMatchedOrSelectWrong:
                        if (isCountMatch || !isCurSelectCorrect)
                            shouldAutoJudge = true; break;
                    case AutoJudgeTriggerType.NumberMatched:
                        if (isCountMatch)
                            shouldAutoJudge = true; break;
                }
                if (shouldAutoJudge)
                    Judge();
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }


    //【提交键】
    public void Judge()
    {
        var listSelectBut = listUIOptionButton.FindAll((but) => but.toggle.isOn);

        bool isCurSelectCorrect = false;//当前按键是否选择正确（ToUpdate判断）
        bool isCountMatch = false;//是否数量已经选全(不管对错）
        bool isAllMatch = false;//是否已经选对
        isCurSelectCorrect = tipsInfo.IsDesire(ref isAllMatch, ref isCountMatch, GetAllSelected());//PS:会自动根据类型判断所选是否正确


        foreach (var but in listSelectBut)
        {
            but.UpdateUI(tipsInfo.ActiveListTipsInfoDesire.Contains(but.data.tipsInfo), true);
        }

        onSelectAll.Invoke(isCountMatch);//用途：
        onJuege.Invoke(isAllMatch);
        //防止再次点击
        if (cgButtonPanel)
            cgButtonPanel.interactable = false;

        ///ToAdd:
        ///1.如果是判断题，那就弄成ToggleGroup

        ///Todo：
        ///1.禁用所有按钮
        //listUIOptionButton.ForEach(bu => bu.SetInteractable(false));
        ///2.判断（从上面抽取判断方法）
        ///3.调用optionButton.UpdateUI高亮对应的按钮，同时通过StringEvent提示正确的选项
    }
    private SOTipsInfo[] GetAllSelected()
    {
        return listUIOptionButton.FindAll((but) => but.toggle.isOn).ConvertAll(
           (but) =>
           {
               if (but.data.tipsInfo)
               {
                   return but.data.tipsInfo;
               }
               Debug.LogError(but + " 的tipsinfo为空！");
               return null;
           }).ToArray();
        //查看所有选中的TipsInfo
    }


    /// <summary>
    /// 自动判断的触发条件
    /// </summary>
    public enum AutoJudgeTriggerType
    {
        NumberMatchedOrSelectWrong,//数量匹配或选中任意错误选项
        NumberMatched,//数量匹配
    }

    #region Obsolete
    [System.Obsolete("Use onBeforeSet Instead")] [HideInInspector] public BoolEvent onSelect;//选中的答案是否正确
    [System.Obsolete("Use onBeforeSet Instead")] [HideInInspector] public BoolEvent onSelectCorrect;//是否选中指定数量的答案，适用于选中所有后才能激活【下一题】按钮（ToRename）
#if UNITY_EDITOR
    void OnValidate()
    {
#pragma warning disable CS0618
        Threeyes.Editor.EditorVersionUpdateTool.TransferUnityEvent(this, ref onSelect, ref onJuege);
        Threeyes.Editor.EditorVersionUpdateTool.TransferUnityEvent(this, ref onSelectCorrect, ref onSelectAll);
#pragma warning restore CS0618
    }
#endif
    #endregion

#if UNITY_EDITOR

    [ContextMenu("UpdateTips")]
    public override void UpdateTips()
    {
        base.UpdateTips();
    }

#endif

}
