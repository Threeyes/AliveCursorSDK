#if USE_UnityCsvUtil
using Sinbad;
#endif
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 从外部读取题库
/// </summary>
public class ExternQuestionLoaderBase<T> : InstanceBase<T>
    where T : ExternQuestionLoaderBase<T>
{
    public BytesRunTimeLoader bytesRunTimeLoaderQuestionContent;//单选题
    public QuestionGenerator questionGenerator;

    public List<QuestionContentData> listQuestionData;
    public List<SOOptionTipsInfo> listRawOptionTipsInfo;


    public void Load()
    {
#if USE_UnityCsvUtil
        StartCoroutine(DeserializeQuestion());
#else
        Debug.LogError("请激活CSV插件！");
#endif
    }


    public virtual IEnumerator DeserializeQuestion()
    {
        yield return null;
#if USE_UnityCsvUtil
        yield return StartCoroutine(IELoadData<QuestionContentData>(bytesRunTimeLoaderQuestionContent, (result) => listQuestionData = result));

        try
        {
            //读取所有问题
            listRawOptionTipsInfo = new List<SOOptionTipsInfo>();
            listRawOptionTipsInfo = listQuestionData.ConvertAll<SOOptionTipsInfo>(
                (qd) =>
                {
                    SOOptionTipsInfo soOptionTipsInfo = SOOptionTipsInfo.CreateInstance<SOOptionTipsInfo>();
                    soOptionTipsInfo.name = qd.question;
                    soOptionTipsInfo.title = qd.title;
                    soOptionTipsInfo.tips = qd.question;
                    soOptionTipsInfo.optionType = qd.GetQuestionType();

                    //Answer(A~D)
                    soOptionTipsInfo.AddAnswerIfNotNull(qd.answerA);
                    soOptionTipsInfo.AddAnswerIfNotNull(qd.answerB);
                    soOptionTipsInfo.AddAnswerIfNotNull(qd.answerC);
                    soOptionTipsInfo.AddAnswerIfNotNull(qd.answerD);
                    soOptionTipsInfo.AddAnswerIfNotNull(qd.answerE);
                    soOptionTipsInfo.AddAnswerIfNotNull(qd.answerF);


                    //Find and Add Correct Answer
                    List<string> listLetter = new List<string>() { "A", "B", "C", "D", "E", "F" };
                    if (qd.correctAnswer.NotNullOrEmpty())
                    {
                        if (soOptionTipsInfo.optionType == SOOptionTipsInfo.OptionType.Judgment)//与答案同名
                        {
                            soOptionTipsInfo.tipsInfoDesire = soOptionTipsInfo.ListTipsInfo.Find((tI) => tI.tips == qd.correctAnswer);
                        }
                        else if (soOptionTipsInfo.optionType == SOOptionTipsInfo.OptionType.SingleChoice)//与答案序号相同
                        {
                            int correctAnswer = 0;
                            if (listLetter.Contains(qd.correctAnswer.ToUpper()))
                            {
                                correctAnswer = listLetter.IndexOf(qd.correctAnswer.ToUpper());
                                soOptionTipsInfo.tipsInfoDesire = soOptionTipsInfo.ListTipsInfo[correctAnswer];
                                Debug.LogWarning("题目 [" + soOptionTipsInfo.tips + "]指定答案！其答案为：" + qd.correctAnswer.ToUpper());
                            }
                            else
                            {
                                Debug.LogError("题目 [" + soOptionTipsInfo.tips + "]没有指定答案！其答案为：" + qd.correctAnswer.ToUpper());
                            }
                        }
                        else if (soOptionTipsInfo.optionType == SOOptionTipsInfo.OptionType.MultipleChoice)//与答案序号相同
                        {
                            var arrAnswer = qd.correctAnswer.ToUpper().ToCharArray();
                            for (int i = 0; i != arrAnswer.Length; i++)
                            {
                                string Answer = char.ToUpper(arrAnswer[i]).ToString();
                                int targetIndex = listLetter.IndexOf(Answer);
                                if (soOptionTipsInfo.ListTipsInfo.Count > targetIndex)
                                    soOptionTipsInfo.listTipsInfoDesire.Add(soOptionTipsInfo.ListTipsInfo[targetIndex]);
                                else
                                {
                                    string errMsg = "[" + soOptionTipsInfo.tips + "] 的 (" + Answer + ") 答案出现问题！";
                                    Debug.LogError(errMsg);
                                    ShowMsg(errMsg);
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError(qd.question + "\r\n答案为空！"); ;
                    }

                    return soOptionTipsInfo;
                });


            //初始化UI及Data
            questionGenerator.InitData(listRawOptionTipsInfo, ShowMsg);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            ShowMsg("初始化题库出错！请检查题库的单元格内是否有多余的换行，或是否有英文逗号！");
        }

#endif
    }


    #region Utility

    protected IEnumerator IELoadData<TData>(BytesRunTimeLoader bytesRunTimeLoader, UnityAction<List<TData>> actSetlistData)
      where TData : new()
    {
        return bytesRunTimeLoader.IELoadAsset(
     (bytes) =>
     {
         try
         {
#if USE_UnityCsvUtil
             //读取资源
             //默认的文本编码格式为ANSI（gb2312）
             MemoryStream ms = new MemoryStream(bytes);
             StreamReader sr = null;
             sr = new StreamReader(ms, System.Text.Encoding.GetEncoding("gb2312"));
             actSetlistData.Execute(CsvUtil.LoadObjects<TData>(sr));
             sr.Dispose();
             ms.Dispose();
#endif
         }
         catch
         {
             Debug.LogError("解析失败！");
         }
     });
    }

    #endregion

    /// <summary>
    /// 提示信息
    /// </summary>
    /// <param name="info"></param>
    public virtual void ShowMsg(string info)
    {
    }
}

