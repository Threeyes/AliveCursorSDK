using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 从题库中抽取题目
/// </summary>
public class QuestionGenerator : QuestionGeneratorBase
{
    [Header("从以下数据中二选一")]
    public SOOptionTipsInfoGroup soOptionTipsInfoGroup;//题库
    public List<SOOptionTipsInfo> listOptionTipsInfo = new List<SOOptionTipsInfo>();//题目组

    protected override List<SOOptionTipsInfo> ListOptionTips
    {
        get
        {
            return soOptionTipsInfoGroup ? soOptionTipsInfoGroup.ListData : listOptionTipsInfo;
        }
        set
        {
            listOptionTipsInfo = value;
        }
    }
}
