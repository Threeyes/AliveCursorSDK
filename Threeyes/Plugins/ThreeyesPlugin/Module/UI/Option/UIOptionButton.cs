using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 答题
/// </summary>
public class UIOptionButton : ElementBase<OptionData>
{
    public Graphic graphicDisplay;//代表选择正确/错误的背景
    public Toggle toggle;
    public Text textIndex;//序号
    public Text text;//文本
    public Image image;//图像
    public AnswerIndexType answerIndexType = AnswerIndexType.Independence;

    public int Index = -1;


    public void OnToggle(bool isOn)
    {
        //toggle.interactable = false;//禁止多次选中（ToDelete）
    }

    public void SetInteractable(bool isOn)
    {
        toggle.interactable = isOn;//禁止多次选中
    }

    [ContextMenu("SelectWithoutNotify")]
    public void SelectWithoutNotify()
    {
        toggle.SetValueWithoutNotify(true);
    }

    public bool hasUpdateUI = false;
    public float fadePercentAutoSelect = 0.5f;//变暗，系统自动选择                                        

    /// <summary>
    /// 更新UI被选中后的状态
    /// 
    /// 用途：系统自动选中正确选项
    /// </summary>
    /// <param name="isMarkSelectCorrect">是否标识为选择正确</param>
    /// <param name="isFixSelect">是否修正选择（适用于选错后系统自动选中）</param>
    public void UpdateUI(bool isMarkSelectCorrect, bool isFixSelect = false)
    {
        //Todo：将toggle.ison与 hasUpdateUI 同步，或者直接用ison代替，删掉isSelected
        if (hasUpdateUI)
            return;

        graphicDisplay.color = isMarkSelectCorrect ? data.colorIsDesire : data.colorNotDesire;

        if (isFixSelect)
            graphicDisplay.color *= fadePercentAutoSelect;//变暗，代表是系统选的

        hasUpdateUI = true;
    }

    public override void InitFunc(OptionData optionData)
    {
        base.InitFunc(optionData);

        //序号
        string answerPrefix = "";//答案的前缀，如序号
        if (optionData.optionTipsInfo.optionType != SOOptionTipsInfo.OptionType.Judgment)//判断题不需要加序号
        {
            switch (answerIndexType)
            {
                case AnswerIndexType.Independence://独立的序号元素
                    if (textIndex)
                        textIndex.text = optionData.strIndex;
                    break;
                case AnswerIndexType.StayWithAnswer://与答案统一
                    answerPrefix = optionData.strIndex + ".";
                    break;
            }
        }

        //答案
        if (optionData.NotNull())
        {
            if (optionData.tipsInfo)
            {
                text.gameObject.SetActive(optionData.tipsInfo.tips.NotNullOrEmpty() && optionData.displayContentOverride.isShowText);
                text.text = answerPrefix + optionData.tipsInfo.tips;

                if (image)
                {
                    image.gameObject.SetActive(optionData.tipsInfo.sprite.NotNull() && optionData.displayContentOverride.isShowSprite);
                    image.sprite = optionData.tipsInfo.sprite;
                }
            }
        }
    }

    /// <summary>
    /// 答案序号的显示方式
    /// </summary>
    public enum AnswerIndexType
    {
        Independence,//单独显示
        StayWithAnswer//与答案一起显示
    }
}

[System.Serializable]
public class OptionData
{
    public Color colorIsDesire = Color.green;
    public Color colorNotDesire = Color.red;

    public SOOptionTipsInfo optionTipsInfo;
    public SOTipsInfo tipsInfo;
    public string strIndex;//序号的内容

    public TipsDisplayContent displayContentOverride = new TipsDisplayContent();//显示类型（替换SOTipsInfo中的配置）
}