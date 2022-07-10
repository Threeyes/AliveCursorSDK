using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 选择题
/// </summary>
[CreateAssetMenu(menuName = "SO/TipsInfo/OptionTipsInfo")]
public class SOOptionTipsInfo : SOTipsInfo
{

    #region Property & Field

    [Header("选项")]
    public OptionType optionType = OptionType.SingleChoice;
    public ActionOnUserChooseWrongType actionOnUserChooseWrong = ActionOnUserChooseWrongType.AutoShowCorrect;
    [SerializeField]
    protected List<SOTipsInfo> listOptionTips = new List<SOTipsInfo>();//可选项

    /// <summary>
    /// 是否选项乱序排列
    /// </summary>
    public bool isShuffle = false;

    //Todo：加listDesireIndex，用于多选
    public List<SOTipsInfo> ActiveListTipsInfoDesire//根据当前选择的Option类型，自动返回对应的DesireTipsInfo
    {
        get
        {
            List<SOTipsInfo> cacheListSOTipsInfo = new List<SOTipsInfo>();
            switch (optionType)
            {
                case OptionType.SingleChoice:
                case OptionType.Judgment:
                    cacheListSOTipsInfo.Add(tipsInfoDesire); break;
                case OptionType.MultipleChoice:
                    cacheListSOTipsInfo.AddRange(listTipsInfoDesire); break;
            }
            return cacheListSOTipsInfo;
        }
    }
    public SOTipsInfo tipsInfoDesire;//目标选项（单选或判断）
    public List<SOTipsInfo> listTipsInfoDesire = new List<SOTipsInfo>();//所有目标选项（多选）

    public InputField.CharacterValidation indexType = InputField.CharacterValidation.Alphanumeric;//序号的类型，默认是A、B、C、D

    public TipsDisplayContent optionDisplayContent = new TipsDisplayContent();//答案的显示内容

    public Color colorIsDesire = Color.green;//选中的颜色
    public Color colorNotDesire = Color.red;//选错的颜色


    //Todo：记录用户的选择

    /// <summary>
    /// 可选项
    /// </summary>
    public List<SOTipsInfo> ListTipsInfo
    {
        get
        {
            return listOptionTips;
        }

        set
        {
            listOptionTips = value;
        }
    }

    public string strOptionType
    {
        get
        {
            NameAttribute nameAttribute = optionType.GetAttributeOfType<NameAttribute>();
            if (nameAttribute != null)
            {
                return nameAttribute.Name;
            }
            return "";
        }
    }
    /// <summary>
    /// 目标选项数
    /// </summary>
    public int DesireCount
    {
        get { return 1; }
    }

    /// <summary>
    /// 目标选项所在的序号（因为选项可能随意排序，所以建议在运行时获取）
    /// </summary>
    public int DesireIndex
    {
        get
        {
            int targetIndex = -1;
            if (tipsInfoDesire != null && ListTipsInfo.Contains(tipsInfoDesire))
            {
                targetIndex = ListTipsInfo.IndexOf(tipsInfoDesire);
            }
            return targetIndex;
        }
    }

    public bool IsCorrect(int index)
    {
        return DesireIndex != -1 && DesireIndex == index;
    }

    #endregion


    #region Method

    /// <summary>
    /// 获取序号对应的选项(0对应A)
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public string GetStrIndex(int index)
    {
        int indexOfA = (int)'A';
        string result = " ";
        switch (indexType)
        {
            case InputField.CharacterValidation.Alphanumeric:
                result = ((char)(indexOfA + index)).ToString(); break;
            case InputField.CharacterValidation.None:
                result = ""; break;
        }
        return result;
    }

    /// <summary>
    /// 判断所选选项是否匹配
    /// </summary>
    /// <param name="isAllMatch">是否全部元素都匹配</param>
    /// <param name="isCountMatch">数量匹配</param>
    /// <param name="arrTipsInfoSelected"></param>
    /// <returns>当前的所有选择是否仍是正确</returns>
    public bool IsDesire(ref bool isAllMatch, ref bool isCountMatch, params SOTipsInfo[] arrTipsInfoSelected)
    {
        //Todo:更换判断条件，避免因为克隆导致的引用不匹配

        switch (optionType)
        {
            case OptionType.SingleChoice:
            case OptionType.Judgment:
                if (tipsInfoDesire.NotNull())
                {
                    if (listOptionTips.Contains(tipsInfoDesire))
                    {
                        isCountMatch = arrTipsInfoSelected.Length == 1;
                        if (arrTipsInfoSelected.Length > 0)
                        {
                            isAllMatch = tipsInfoDesire == arrTipsInfoSelected[0];
                            return isAllMatch;
                        }
                    }
                    else
                    {
                        Debug.LogError("目标选项不在选项列表中！");
                    }
                }
                else
                {
                    Debug.LogError("目标选项为空！");
                }
                break;

            case OptionType.MultipleChoice:
                if (listOptionTips.IsContainAll(listTipsInfoDesire))
                {
                    isCountMatch = arrTipsInfoSelected.Length == listTipsInfoDesire.Count;

                    if (arrTipsInfoSelected.Length > 0)
                    {
                        isAllMatch = listTipsInfoDesire.IsElementEqual(arrTipsInfoSelected);//检查两个列表是否有相同元素（忽略顺序）

                        return listTipsInfoDesire.IsContainAll(arrTipsInfoSelected);
                    }
                }
                else
                {
                    Debug.LogError("目标选项不在选项列表中！");
                }
                break;
        }

        return false;
    }

    public void AddAnswerIfNotNull(string content)
    {
        if (content.NotNullOrEmpty())
        {
            SOTipsInfo sOTipsInfo = SOTipsInfo.CreateInstance<SOTipsInfo>();
            sOTipsInfo.tips = content;
            listOptionTips.Add(sOTipsInfo);
        }
    }

    #endregion

    /// <summary>
    /// 选项类型
    /// </summary>
    public enum OptionType
    {
        [Name("单选题")]
        SingleChoice,//单选
        [Name("多选题")]
        MultipleChoice,//多选
        [Name("判断题")]
        Judgment//判断
    }

    /// <summary>
    /// 当用户选择错误时的系统操作
    /// </summary>
    public enum ActionOnUserChooseWrongType
    {
        Null = 0,//一直等到用户选择正确为止，只适用于单选和判断
        AutoShowCorrect = 1,//系统自动选择正确
    }

    #region Editor

    [ContextMenu("UseDesireNameAsTips")]
    public void UseDesireNameAsTips()
    {
        if (tipsInfoDesire)
            tips = tipsInfoDesire.tips;
    }

    [ContextMenu("UseFirstOptionAsDesire")]
    public void UseFirstOptionAsDesire()
    {
        if (listOptionTips.Count > 0)
            tipsInfoDesire = listOptionTips[0];
    }

    /// <summary>
    /// 检查选项是否合法
    /// </summary>
    [ContextMenu("DetectIfOptionValid")]
    public void DetectIfOptionValid()
    {
        if (listOptionTips.Count == 0)
        {
            Debug.LogError("无选项！");
            return;
        }
        else
        {
            if (tipsInfoDesire.IsNull())
            {
                Debug.LogError(name + " 没有目标选项！");
                return;
            }
            else
            {
                if (!listOptionTips.Contains((tipsInfoDesire)))
                {
                    Debug.LogError(name + " 可选列表中不包含目标选项！");
                    return;
                }
            }
            for (int i = 0; i != listOptionTips.Count; i++)
            {
                var optionTips = listOptionTips[i];
                if (listOptionTips.FindAll((tips) => tips == optionTips).Count > 1)
                {
                    Debug.LogError(name + " 的第" + (i + 1) + "个选项重复！");
                }

                if (optionTips.IsNull())
                {
                    Debug.LogError("第" + (i + 1) + "个为空！");
                }

            }
        }
    }
    #endregion
}


/// <summary>
/// 题目
/// </summary>
[System.Serializable]
public class QuestionContentData
{
#if USE_UnityCsvUtil
    [CSVHeader("标题")]
    public string title;

    [CSVHeader("问题")]
    public string question;

    [CSVHeader("答案A")]
    public string answerA;

    [CSVHeader("答案B")]
    public string answerB;

    [CSVHeader("答案C")]
    public string answerC;

    [CSVHeader("答案D")]
    public string answerD;

    [CSVHeader("答案E")]
    public string answerE;

    [CSVHeader("答案F")]
    public string answerF;

    [CSVHeader("正确答案")]
    public string correctAnswer;

    /// <summary>
    /// OverrideConfigData中的分值，可空
    /// </summary>
    [CSVHeader("分值")]
    public string score;

    [CSVHeader("提示")]
    public string hint;

    [CSVHeader("解释")]
    public string AnswerExplain;

    [CSVHeader("题目类型")]
    public string questionType;

    //要忽略
    public SOOptionTipsInfo.OptionType GetQuestionType()
    {
        var result = SOOptionTipsInfo.OptionType.SingleChoice;
        if (questionType == "单选题")
            result = SOOptionTipsInfo.OptionType.SingleChoice;
        else if (questionType == "多选题")
            result = SOOptionTipsInfo.OptionType.MultipleChoice;
        else if (questionType == "判断题")
            result = SOOptionTipsInfo.OptionType.Judgment;
        else
            Debug.LogError("该题目类型未定义: " + questionType);
        return result;
    }
#endif
}

