using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TextHelper : ComponentHelperBase<Text>
{
    //数字格式化： https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/formatting-numeric-results-table
    //示例： http://www.csharp-examples.net/string-format-double/

    //数字格式:
    //{0:FX}，其中FX中的X代表小数位，设置为{0:0}相当于只取整数
    //0:D2 十进制位数（如00，01）

    //日期格式：
    //分:秒:毫秒 = mm\:ss\:f = 00:00:0
    //分:秒 = mm\:ss = 00:00
    public string format;//如：当前数量为{0} 或 当前进度为{0}/{1}
    public bool isTimeFormat = false;//是否为时间格式

    public float sumAppendTime = 1f;//逐渐显示的总时间
    public DisplayType displayType = DisplayType.Instant;

    public string desireContent;//指定的内容
    public BoolEvent onMatchContent;//检查内容是否为指定的
    public StringEvent onValueChanged;//更改值

    public string CompText
    {
        get
        {
            return Comp.text;
        }
        set
        {
            Comp.text = value;
            onValueChanged.Invoke(value);
        }
    }

    public enum DisplayType
    {
        Instant,//立即更换
        Append,//在后面累加
        TweenAppend,//逐渐显示
    }

    /// <summary>
    /// 检查内容是否一致
    /// </summary>
    public void CheckIfContentMatch()
    {
        if (CompText.NotNullOrEmpty() && desireContent.NotNullOrEmpty())
        {
            onMatchContent.Invoke(CompText == desireContent);
        }
    }

    //配合使用，适用于显示页数、进度
    public void Set(Vector2 value)
    {
        SetObjMulti(value.x, value.y);
        //try
        //{
        //    string str = string.Format(format, value.x, value.y);
        //    Comp.text = str;
        //}
        //catch (System.Exception e)
        //{
        //    Debug.LogError(e);
        //}
    }
    public void Set(int value)
    {
        SetObj(value);
    }
    public void Set(float value)
    {
        SetObj(value);
    }


    public void Set(string value)
    {
        SetObj(value);
    }

    public void SetObj(object value)
    {
        SetObjMulti(value);
    }

    public void SetObjMulti(params object[] values)
    {
        string str = "";
        if (values.Length == 0)
            return;

        object valueFirst = values[0];
        if (format.NotNullOrEmpty())
        {
            try
            {
                if (isTimeFormat)
                {
                    double value = double.Parse(valueFirst.ToString());
                    str = FormatTime(value);
                }
                else
                {
                    str = string.Format(format, values);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("格式错误！" + e);
                str = valueFirst.ToString();
            }
        }
        else
        {
            str = valueFirst.ToString();
        }

        switch (displayType)
        {
            case DisplayType.Instant:
                CompText = str; break;
            case DisplayType.Append:
                CompText += str; break;
            case DisplayType.TweenAppend:
                TryStopCoroutine();
                cacheEnum = StartCoroutine(IETweenAppendText(str));
                break;
        }
    }

    protected Coroutine cacheEnum;

    protected void TryStopCoroutine()
    {
        if (cacheEnum != null)
            StopCoroutine(cacheEnum);
    }


    IEnumerator IETweenAppendText(string content)
    {
        CompText = "";

        foreach (char c in content)
        {
            CompText += c;
            yield return new WaitForSeconds(sumAppendTime / content.Length);
        }
    }


    #region Utility

    string FormatTime(double time)
    {
        TimeSpan timeSpan = TimeSpan.FromMilliseconds(time * 1000);
        DateTime dateTime = new DateTime(timeSpan.Ticks);
        return dateTime.ToString(format);

    }

    #endregion
}
