using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 从答案中抽取并随机生成题目
/// 适用于：物品识别
/// 
/// 原理：根据模板soOptionTipsInfoTemplate，从soTipsInfoGroup中抽取并生成题目，并且随机混淆
/// </summary>
public class RandomQuestionGenerator : QuestionGeneratorBase
{
    public SOOptionTipsInfo soOptionTipsInfoTemplate;//题目模板（决定答案的数量、需要显示的模块）

    public SOTipsInfoGroup soTipsInfoGroup;//一系列的选项
    public List<SOTipsInfo> listTipsInfo = new List<SOTipsInfo>();
    protected override List<SOOptionTipsInfo> ListOptionTips
    {
        get
        {
            List<SOOptionTipsInfo> listSoChoiceTipsInfos = new List<SOOptionTipsInfo>();

            List<SOTipsInfo> listTipsInfoDesire = (soTipsInfoGroup ? soTipsInfoGroup.ListData : listTipsInfo).SimpleClone();

            for (int i = 0; i != listTipsInfoDesire.Count; i++)//遍历待选选项
            {
                SOTipsInfo sOTipsInfoDesire = listTipsInfoDesire[i];//正确的答案

                //创建实例
                SOOptionTipsInfo soOptionTipsInfo = soOptionTipsInfoTemplate.CloneInstantiate();

                //随机抽取剩余的选项
                var listCurTipsInfoToChoose = new List<SOTipsInfo>();
                listCurTipsInfoToChoose.Add(sOTipsInfoDesire);//添加正确的选项

                var listLeft = listTipsInfoDesire.SimpleClone();
                listLeft.Shuffle();
                for (int j = 0; j < soOptionTipsInfoTemplate.ListTipsInfo.Count - 1; j++)//添加剩余的错误选项
                {
                    var tipsInfoToSelect = listLeft.Find((sTI) => sTI != null && !listCurTipsInfoToChoose.Contains(sTI));
                    if (tipsInfoToSelect)
                        listCurTipsInfoToChoose.Add(tipsInfoToSelect);
                }

                soOptionTipsInfo.ListTipsInfo = listCurTipsInfoToChoose;
                soOptionTipsInfo.tipsInfoDesire = sOTipsInfoDesire;//将标题设置为正确选项的文本
                soOptionTipsInfo.sprite = sOTipsInfoDesire.sprite;
                soOptionTipsInfo.isShuffle = soOptionTipsInfoTemplate.isShuffle;//是否乱序排列

                listSoChoiceTipsInfos.Add(soOptionTipsInfo);
            }
            return listSoChoiceTipsInfos;
        }
        set
        {
            //ToAdd
        }
    }
}
