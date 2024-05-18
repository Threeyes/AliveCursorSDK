using System;
using Threeyes.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Threeyes.Steamworks
{
    /// <summary>
    /// 
    /// 
    /// PS：
    /// -提供多个可选组件以及暴露UnityEvent，方便兼容其他文本呈现组件
    /// -如果不希望更改Text的颜色，可以直接通过onSetText来设置，取消text字段的引用
    /// </summary>
    public class TextController : ConfigurableComponentBase<TextController, SOTextControllerConfig, TextController.ConfigInfo, TextController.PropertyBag>
    {
        #region Property & Field
        public Text text;//[Optional]
        public TMP_Text tMP_Text;//[Optional]

        public StringEvent onSetText;
        public ColorEvent onSetTextColor;
        #endregion

        #region IModHandler
        public override void UpdateSetting()
        {
            if (text)
            {
                text.text = Config.text;
                text.color = Config.textColor;
            }
            if (tMP_Text)
            {
                tMP_Text.text = Config.text;
                tMP_Text.color = Config.textColor;
            }
            onSetText.Invoke(Config.text);
            onSetTextColor.Invoke(Config.textColor);
        }
        #endregion

        #region Define
        [Serializable]
        public class ConfigInfo : SerializableComponentConfigInfoBase
        {
            public string text;
            public Color textColor = Color.white;
            ///ToAdd（其他通用的文本相关的参数）：
            ///-Align（使用自定义的枚举）

            public ConfigInfo()
            {
            }
        }

        public class PropertyBag : ConfigurableComponentPropertyBagBase<TextController, ConfigInfo> { }
        #endregion
    }
}