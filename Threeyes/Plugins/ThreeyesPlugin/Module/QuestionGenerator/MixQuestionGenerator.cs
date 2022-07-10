using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 从多个题库中抽取题目
/// </summary>
public class MixQuestionGenerator : QuestionGeneratorBase
{
    public List<SOOptionTipsInfoGroup> listSoOptionTipsInfoGroup = new List<SOOptionTipsInfoGroup>();//多个题库
    protected override List<SOOptionTipsInfo> ListOptionTips
    {
        get
        {
            var listData = new List<SOOptionTipsInfo>();
            foreach (var optionTipsInfoGroup in listSoOptionTipsInfoGroup)
            {
                if (optionTipsInfoGroup)
                    listData.AddRange(optionTipsInfoGroup.ListData);
                else
                {
                    Debug.LogError("列表有空数据！");
                }
            }
            return listData;
        }
        set
        {
            //ToAdd
        }
    }
}
