using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if USE_JsonDotNet
using Newtonsoft.Json;
#endif
/// <summary>
/// 根据模板生成选择题
/// 
/// 名词说明：
/// --题库：一系列题目
/// --题目：一道题目
/// --选项：题目中的选项如A、B、C
/// 
/// 功能：
/// 1.随机排序
/// </summary>
public abstract class QuestionGeneratorBase : MonoBehaviour
{
    public Sequence_SOOptionTipsInfo optionTipsInfoSequenceGroup;
    public ValueChanger_Float valueChangerScore;//计算得分

    public bool isInitOnAwake = true;//Awakes时初始化题目

    [Header("Config")]
    //自定义显示的题目数(大于0才有效；为0时就取默认数量）(可通过配置表读取）
    public int sumSingleChoiceQuestion = 0;
    public int sumMultipleChoiceQuestion = 0;
    public int sumJudgmentQuestion = 0;
    public int sumQuestion { get { return sumSingleChoiceQuestion + sumMultipleChoiceQuestion + sumJudgmentQuestion; } }//总题数(Todo:改为get only)

    //各项分数(大于0才有效）(可通过配置表读取）
    public bool isSameScorePerQuestion = true;//是否每道题分值都一样（如果为true，那以下的分支无效）
    public int scoreSingleChoiceQuestion = 0;
    public int scoreMultipleChoiceQuestion = 0;
    public int scoreJudgmentQuestion = 0;

    public int sumScore = 100; //总分
    public bool isQuestionShuffle = true;//是否题目乱序


    [Header("Debug")]
    public bool isEditorShowAllQuestion = false;//编辑器模式下，显示所有的题目，方便测试
    public bool isEditorStopSuffle = false;//编辑器模式下，禁止乱序

    protected virtual void Awake()
    {
        if (isInitOnAwake)
            InitData();
    }

    public void InitData()
    {
        InitData(ListOptionTips);
    }

    /// <summary>
    /// 初始化数据
    /// </summary>
    public void InitData(List<SOOptionTipsInfo> listRawOptionTipsInfo, UnityAction<string> actErrorOccur = null)
    {
        //根据配置，对题库进行筛选
        List<SOOptionTipsInfo> listResultOptionTipsInfo = new List<SOOptionTipsInfo>();

        //对题目进行分类
        List<SOOptionTipsInfo> listSingleChoiceTipsInfo = listRawOptionTipsInfo.FindAll((oti) => oti.optionType == SOOptionTipsInfo.OptionType.SingleChoice);
        List<SOOptionTipsInfo> listMultipleChoiceTipsInfo = listRawOptionTipsInfo.FindAll((oti) => oti.optionType == SOOptionTipsInfo.OptionType.MultipleChoice);
        List<SOOptionTipsInfo> listJudgmentTipsInfo = listRawOptionTipsInfo.FindAll((oti) => oti.optionType == SOOptionTipsInfo.OptionType.Judgment);

        //计算数量
        if (sumSingleChoiceQuestion <= 0)
            sumSingleChoiceQuestion = listSingleChoiceTipsInfo.Count;
        if (sumMultipleChoiceQuestion <= 0)
            sumMultipleChoiceQuestion = listMultipleChoiceTipsInfo.Count;
        if (sumJudgmentQuestion <= 0)
            sumJudgmentQuestion = listJudgmentTipsInfo.Count;

        //  检测题库是否正常（Todo:改为针对每个题目分别检测）
        if (sumSingleChoiceQuestion > listSingleChoiceTipsInfo.Count)
        {
            actErrorOccur.Execute("单选题数量不足: " + listSingleChoiceTipsInfo.Count + "/" + sumSingleChoiceQuestion);
            return;
        }
        else if (sumMultipleChoiceQuestion > listMultipleChoiceTipsInfo.Count)
        {
            actErrorOccur.Execute("多选题数量不足: " + listSingleChoiceTipsInfo.Count + " / " + sumMultipleChoiceQuestion);
            return;
        }
        else if (sumJudgmentQuestion > listJudgmentTipsInfo.Count)
        {
            actErrorOccur.Execute("判断题数量不足: " + listSingleChoiceTipsInfo.Count + " / " + sumJudgmentQuestion);
            return;
        }

        //乱序
        if (Application.isEditor && isEditorStopSuffle)//调试
        {
            Debug.Log("编辑器下取消乱序！");
        }
        else if (isQuestionShuffle)//Todo:将设置放到QuestionGenerator
        {
            listSingleChoiceTipsInfo.Shuffle();
            listMultipleChoiceTipsInfo.Shuffle();
            listJudgmentTipsInfo.Shuffle();
        }

        //截取题目数量
        bool isShowAllQuestion = Application.isEditor && isEditorShowAllQuestion;
        if (!isShowAllQuestion && sumSingleChoiceQuestion > 0 && sumSingleChoiceQuestion <= listSingleChoiceTipsInfo.Count)
            listSingleChoiceTipsInfo = listSingleChoiceTipsInfo.GetRange(0, sumSingleChoiceQuestion);
        if (!isShowAllQuestion && sumMultipleChoiceQuestion > 0 && sumMultipleChoiceQuestion <= listMultipleChoiceTipsInfo.Count)
            listMultipleChoiceTipsInfo = listMultipleChoiceTipsInfo.GetRange(0, sumMultipleChoiceQuestion);
        if (!isShowAllQuestion && sumJudgmentQuestion > 0 && sumJudgmentQuestion <= listJudgmentTipsInfo.Count)
            listJudgmentTipsInfo = listJudgmentTipsInfo.GetRange(0, sumJudgmentQuestion);

        //整合
        listResultOptionTipsInfo.AddRange(listSingleChoiceTipsInfo);
        listResultOptionTipsInfo.AddRange(listMultipleChoiceTipsInfo);
        listResultOptionTipsInfo.AddRange(listJudgmentTipsInfo);
        optionTipsInfoSequenceGroup.ListData = listResultOptionTipsInfo;
        optionTipsInfoSequenceGroup.SetFirst();


        //缓存数据，重置计分
        ListOptionTips = listRawOptionTipsInfo;
        if (valueChangerScore)
        {
            valueChangerScore.CurValue = 0;
        }
    }



    public void AddScore()
    {
        if (valueChangerScore)
            valueChangerScore.Add(GetScore(optionTipsInfoSequenceGroup.CurData.optionType));
    }


    /// <summary>
    /// 获取题目的得分
    /// </summary>
    /// <param name="optionType"></param>
    /// <returns></returns>
    public int GetScore(SOOptionTipsInfo.OptionType optionType)
    {
        if (isSameScorePerQuestion)
        {
            if (sumQuestion > 0)
                return sumScore / sumQuestion;
            else
                return 0;
        }
        else
        {
            int score = 0;
            switch (optionType)
            {
                case SOOptionTipsInfo.OptionType.SingleChoice:
                    score = scoreSingleChoiceQuestion; break;
                case SOOptionTipsInfo.OptionType.MultipleChoice:
                    score = scoreMultipleChoiceQuestion; break;
                case SOOptionTipsInfo.OptionType.Judgment:
                    score = scoreJudgmentQuestion; break;
                default:
                    Debug.LogError("Not Define!"); break;
            }
            if (score == 0)
            {
                Debug.LogError(optionType.GetAttributeOfType<JsonPropertyAttribute>().PropertyName + " 选项分数为0!");
            }
            return score;
        }
    }



    protected abstract List<SOOptionTipsInfo> ListOptionTips { get; set; }
}
