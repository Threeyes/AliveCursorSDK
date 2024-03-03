using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.Localization
{
    /// <summary>
    /// PS:
    /// -仅需要提供接口即可，实现代码可以放在任意位置
    /// 
    /// ToAdd:
    /// -可以（通过读取csv等文件），让用户在运行时添加多语言
    /// </summary>
    public interface ILocalizationManager
    {
        string GetFormatTranslationText(string translationName, string[] args = null);
        string GetTranslationText(string name, string fallback = null, bool replaceTokens = true);
    }
}