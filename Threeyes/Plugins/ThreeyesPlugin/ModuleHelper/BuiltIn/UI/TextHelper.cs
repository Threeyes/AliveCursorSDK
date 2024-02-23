using System.Collections;
using System.Collections.Generic;
using Threeyes.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Threeyes.ModuleHelper
{
    /// <summary>
    /// PS:
    /// -Text组件可以为空，通过onValueChanged来获取当前值
    /// </summary>
    public class TextHelper : ComponentHelperBase<Text>
    {
        #region Property & Field
        public string CompText
        {
            get
            {
                if (Comp)
                    return Comp.text;
                return null;
            }
            set
            {
                if (Comp)
                    Comp.text = value;
                onValueChanged.Invoke(value);//通知事件
            }
        }
        public bool isAppend = false;//Append to existing strings
        public DisplayType displayType = DisplayType.Instant;


        ///Common format type (Ref: https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings):
        ///-Number:
        ///     -{0:FX}: 取小数。其中X代表小数位，设置为{0:0}相当于只取整数
        ///     -{0:D2}: 十进制位数，如果不足10则用0占位（如00，01）
        public string format;//字符串格式。如：当前数量为{0} 或 当前进度为{0}/{1}
        public float tweenDuration = 1f;//Tween的总时间

        public StringEvent onValueChanged;//值更新通知

        //Runtime
        string newContent = "";
        #endregion

        #region Set

        //配合format使用，适用于显示页数、进度
        public void Set(Vector2 value)
        {
            SetObjs(value.x, value.y);//将值进行拆分，以便使用format处理
        }
        public void Set(int value)
        {
            SetObj(value);
        }
        public void Set(float value)
        {
            SetObj(value);
        }
        public void Set(bool value)
        {
            SetObj(value);
        }
        public void Set(string value)
        {
            SetObj(value);
        }

        public void SetObj(object value)
        {
            SetObjs(value);
        }

        /// <summary>
        /// 内部的格式化字符串方法
        /// </summary>
        /// <param name="values">所有字符串数组，当数组元素大于1时需要配合format使用</param>
        public void SetObjs(params object[] values)
        {
            string curContent = CompText;

            newContent = "";
            if (values.Length == 0)
                return;

            object valueFirst = values[0];
            if (format.NotNullOrEmpty())
            {
                try
                {
                    newContent = string.Format(format, values);//使用所有输入值
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Wrong format: {format}!\r\n" + e);
                    newContent = valueFirst.ToString();//使用Fallback值
                }
            }
            else
            {
                newContent = valueFirst.ToString();
            }

            switch (displayType)
            {
                case DisplayType.Instant:
                    CompText = isAppend ? curContent + newContent : newContent; break;
                case DisplayType.Tween:
                    TryStopCoroutine();
                    cacheEnum = StartCoroutine(IETweenAppendText(newContent, isAppend ? curContent : ""));
                    break;
            }
        }
        #endregion

        #region Utility
        protected UnityEngine.Coroutine cacheEnum;
        protected void TryStopCoroutine()
        {
            if (cacheEnum != null)
                StopCoroutine(cacheEnum);
        }
        IEnumerator IETweenAppendText(string content, string beginContent = "")
        {
            CompText = beginContent;

            foreach (char c in content)
            {
                CompText += c;
                yield return new WaitForSeconds(tweenDuration / content.Length);
            }
        }
        #endregion

        #region Define
        /// <summary>
        /// How to display new content
        /// </summary>
        public enum DisplayType
        {
            Instant,//立即更新
            Tween,//逐渐显示
        }
        #endregion
    }
}